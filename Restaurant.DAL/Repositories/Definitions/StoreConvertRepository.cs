using Shared.Helpers;
using Restaurant.DAL.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class StoreConvertRepository : IStoreConvertRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IStockRepository _stock;

    public StoreConvertRepository(ApplicationDbContext context)
    {
        _context = context;
        _stock = new StockRepository(context);
    }

    public async Task<PaginatedResult<StoreConvert>> GetPagedConvertsAsync(PageRequest request)
    {
        var query = _context.StoreConverts.AsNoTracking()
            .Include(a => a.FromStore)
            .Include(a => a.ToStore)
            .Include(a => a.StoreConvertProducts)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ConvertCode.ToLower().Contains(search)
                || a.FromStore.ArName.ToLower().Contains(search) || a.FromStore.EnName.ToLower().Contains(search)
                || a.ToStore.ArName.ToLower().Contains(search) || a.ToStore.EnName.ToLower().Contains(search));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.ConvertDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.ConvertDate < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.ConvertDate)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<StoreConvert>.Create(data, total, request);
    }

    public async Task<StoreConvert> GetConvertByIdAsync(int id)
    {
        return await _context.StoreConverts.AsNoTracking()
            .Include(a => a.FromStore)
            .Include(a => a.ToStore)
            .Include(a => a.StoreConvertProducts).ThenInclude(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var last = await _context.StoreConverts.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.ConvertCode)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
    }

    public async Task<ResultMessage> CreateConvertAsync(StoreConvert convert)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (convert.FromStoreId == convert.ToStoreId)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "SameStore"
                };
            }

            if (convert.StoreConvertProducts is null || convert.StoreConvertProducts.Count == 0)
            {
                return new ResultMessage { Success = false, Message = "AtLeastOneItem" };
            }

            // a negative quantity would add stock to the source store out of nothing
            if (convert.StoreConvertProducts.Any(a => a.Quantity <= 0))
            {
                return new ResultMessage { Success = false, Message = "Val.GreaterThanZero" };
            }

            var lines = convert.StoreConvertProducts
                .GroupBy(a => a.ProductId)
                .Select(g => new StoreConvertProduct
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            var movements = new List<StoreProductMovement>();
            foreach (var item in lines)
            {
                if (!await _stock.AdjustAsync(convert.FromStoreId, item.ProductId, -item.Quantity, createIfMissing: false))
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "InsufficientStock"
                    };
                }

                await _stock.AdjustAsync(convert.ToStoreId, item.ProductId, item.Quantity);
                movements.Add(new StoreProductMovement
                {
                    StoreId = convert.FromStoreId,
                    ProductId = item.ProductId,
                    QuantityDelta = -item.Quantity,
                    Source = StockMovementSource.StoreConvert
                });
                movements.Add(new StoreProductMovement
                {
                    StoreId = convert.ToStoreId,
                    ProductId = item.ProductId,
                    QuantityDelta = item.Quantity,
                    Source = StockMovementSource.StoreConvert
                });
            }

            var entity = new StoreConvert
            {
                ConvertCode = await GetNextCodeAsync(),
                ConvertDate = DateTime.Now,
                FromStoreId = convert.FromStoreId,
                ToStoreId = convert.ToStoreId,
                StoreConvertProducts = lines
            };

            foreach (var movement in movements)
            {
                movement.ReferenceCode = entity.ConvertCode;
                await _context.StoreProductMovements.AddAsync(movement);
            }

            await _context.StoreConverts.AddAsync(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage
            {
                Success = true,
                Data = entity.ConvertCode
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "ConcurrentUpdate"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteConvertAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var convert = await _context.StoreConverts
                .Include(a => a.StoreConvertProducts)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (convert is null)
            {
                return new ResultMessage { Success = true };
            }

            foreach (var line in convert.StoreConvertProducts)
            {
                if (!await _stock.AdjustAsync(convert.ToStoreId, line.ProductId, -line.Quantity, createIfMissing: false))
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "InsufficientStock"
                    };
                }

                await _stock.AdjustAsync(convert.FromStoreId, line.ProductId, line.Quantity);

                await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                {
                    StoreId = convert.ToStoreId,
                    ProductId = line.ProductId,
                    QuantityDelta = -line.Quantity,
                    Source = StockMovementSource.StoreConvert,
                    ReferenceCode = convert.ConvertCode
                });
                await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                {
                    StoreId = convert.FromStoreId,
                    ProductId = line.ProductId,
                    QuantityDelta = line.Quantity,
                    Source = StockMovementSource.StoreConvert,
                    ReferenceCode = convert.ConvertCode
                });
            }
            _context.StoreConverts.Remove(convert);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "ConcurrentUpdate"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
