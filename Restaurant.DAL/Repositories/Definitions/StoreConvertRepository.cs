using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class StoreConvertRepository : IStoreConvertRepository
{
    private readonly ApplicationDbContext _context;

    public StoreConvertRepository(ApplicationDbContext context)
    {
        _context = context;
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
            .Include(a => a.StoreConvertProducts).ThenInclude(a => a.StoreProduct).ThenInclude(a => a.Product)
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

            var wanted = convert.StoreConvertProducts
                .GroupBy(a => a.StoreProduct.ProductId)
                .Select(g => new
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                .ToList();

            var lines = new List<StoreConvertProduct>();
            foreach (var item in wanted)
            {
                var from = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == convert.FromStoreId
                    && a.ProductId == item.ProductId);
                if (from is null || from.Quantity < item.Quantity)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "InsufficientStock"
                    };
                }

                var to = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == convert.ToStoreId
                    && a.ProductId == item.ProductId);
                if (to is null)
                {
                    to = new StoreProduct
                    {
                        StoreId = convert.ToStoreId,
                        ProductId = item.ProductId
                    };
                    await _context.StoreProducts.AddAsync(to);
                }

                from.Quantity -= item.Quantity;
                to.Quantity += item.Quantity;
                lines.Add(new StoreConvertProduct
                {
                    StoreProduct = from,
                    Quantity = item.Quantity
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

            await _context.StoreConverts.AddAsync(entity);
            await _context.SaveChangesAsync();

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
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeleteConvertAsync(int id)
    {
        try
        {
            var convert = await _context.StoreConverts
                .Include(a => a.StoreConvertProducts).ThenInclude(a => a.StoreProduct)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (convert is null)
            {
                return new ResultMessage { Success = true };
            }

            foreach (var line in convert.StoreConvertProducts)
            {
                var to = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == convert.ToStoreId
                    && a.ProductId == line.StoreProduct.ProductId);
                if (to is null || to.Quantity < line.Quantity)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "InsufficientStock"
                    };
                }

                to.Quantity -= line.Quantity;
                line.StoreProduct.Quantity += line.Quantity;
            }

            _context.StoreConverts.Remove(convert);
            await _context.SaveChangesAsync();

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
                Message = ex.Message
            };
        }
    }
}
