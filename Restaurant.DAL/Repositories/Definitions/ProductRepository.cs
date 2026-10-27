using Shared.Helpers;
using Restaurant.DAL.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IStockRepository _stock;
    private readonly IBillSupportRepository _billSupport;

    public ProductRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
        _stock = new StockRepository(context);
        _billSupport = new BillSupportRepository(context);
    }

    public async Task<PaginatedResult<Product>> GetPagedProductsAsync(PageRequest request)
    {
        var query = _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search) || a.EnName.ToLower().Contains(search) || a.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        var pagedStores = await _context.Stores.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var pagedQuantities = await _stock.QuantitiesAsync();

        foreach (var product in data)
        {
            product.Stocks = pagedStores.Select(store => new ProductStock
            {
                StoreId = store.Id,
                Store = store,
                Quantity = _stock.FindQuantity(pagedQuantities, store.Id, product.Id),
                MinQuantity = _stock.FindMinQuantity(pagedQuantities, store.Id, product.Id)
            }).ToList();
        }

        return PaginatedResult<Product>.Create(data, total, request);
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (product is not null)
        {
            var stores = await _context.Stores.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
            var quantities = await _stock.QuantitiesAsync();

            product.Stocks = stores.Select(store => new ProductStock
            {
                StoreId = store.Id,
                Store = store,
                Quantity = _stock.FindQuantity(quantities, store.Id, product.Id),
                MinQuantity = _stock.FindMinQuantity(quantities, store.Id, product.Id)
            }).ToList();

            var itemCode = BillSupportRepository.ItemCodePrefix + id;
            var unitRows = await _context.ItemUnits.AsNoTracking()
                .Where(a => a.Item.Code == itemCode && a.Unit.Code.StartsWith(BillSupportRepository.UnitCodePrefix))
                .Select(a => new { Row = a, a.Unit.Code })
                .ToListAsync();

            var unitPrices = await _context.UnitPrices.AsNoTracking().ToListAsync();
            foreach (var unitRow in unitRows)
            {
                var row = unitRow.Row;
                var unitPriceId = _billSupport.UnitPriceIdOf(unitRow.Code);
                product.Units.Add(new ProductUnit
                {
                    UnitPriceId = unitPriceId,
                    UnitPrice = unitPrices.FirstOrDefault(a => a.Id == unitPriceId),
                    TransRate = (decimal)row.TransRate,
                    IsDefault = row.IsDefault,
                    Price = row.LastPurchase ?? 0
                });
            }
        }

        return product;
    }

    public async Task<string> NewProductCodeAsync()
    {
        var codes = await _context.Products.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertProductAsync(Product entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var costUnit = entity.Units.Where(a => a.Price > 0)
                                   .OrderBy(a => a.TransRate)
                                   .FirstOrDefault();

        try
        {
            if (entity.Id == 0)
            {
                entity.Code = await NewProductCodeAsync();
                entity.Cost = costUnit is null ? 0 : costUnit.Price / costUnit.TransRate;
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
                await _context.Products.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Products.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                // once the stock moved, its quantities and cost are in the saved rates: changing a rate would change what they mean
                var hasMovements = await _context.StoreProductMovements.AnyAsync(a => a.ProductId == entity.Id);
                if (hasMovements)
                {
                    var savedItemCode = BillSupportRepository.ItemCodePrefix + entity.Id;
                    var savedUnits = await _context.ItemUnits.AsNoTracking()
                        .Include(a => a.Unit)
                        .Where(a => a.Item.Code == savedItemCode && a.Unit.Code.StartsWith(BillSupportRepository.UnitCodePrefix))
                        .ToListAsync();

                    foreach (var unit in entity.Units)
                    {
                        var saved = savedUnits.FirstOrDefault(a => a.Unit.Code == BillSupportRepository.UnitCodePrefix + unit.UnitPriceId);
                        if (saved is not null && (decimal)saved.TransRate != unit.TransRate)
                        {
                            return new ResultMessage { Success = false, Message = "UnitRateLockedByMovements" };
                        }
                    }
                }

                entity.Code = existing.Code;
                entity.Cost = costUnit is null || hasMovements ? existing.Cost : costUnit.Price / costUnit.TransRate;
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }

            var unitItemCode = BillSupportRepository.ItemCodePrefix + entity.Id;
            var hadUnits = await _context.ItemUnits.AnyAsync(a => a.Item.Code == unitItemCode && a.Unit.Code.StartsWith(BillSupportRepository.UnitCodePrefix));
            if (entity.Units.Count > 0 || hadUnits)
            {
                var itemId = await _billSupport.GetItemIdAsync(entity.Id);
                var oldRows = await _context.ItemUnits
                    .Include(a => a.Unit)
                    .Where(a => a.ItemId == itemId && a.Unit.Code.StartsWith(BillSupportRepository.UnitCodePrefix))
                    .ToListAsync();

                // the stock unit is added automatically as the first unit; once the product has its own default it must stop being one
                if (entity.Units.Count > 0)
                {
                    var stockUnitRows = await _context.ItemUnits
                        .Where(a => a.ItemId == itemId && a.Unit.Code == StockRepository.BaseUnitCode && a.IsDefault)
                        .ToListAsync();
                    stockUnitRows.ForEach(a => a.IsDefault = false);
                }

                // a unit that a recipe still uses cannot be taken off the product (the recipe would keep a rate nobody owns)
                var keptUnitIds = entity.Units.Select(a => a.UnitPriceId).ToList();
                var removedRows = oldRows.Where(a => !keptUnitIds.Contains(_billSupport.UnitPriceIdOf(a.Unit.Code))).ToList();
                if (removedRows.Count > 0)
                {
                    var removedUnitIds = removedRows.Select(a => _billSupport.UnitPriceIdOf(a.Unit.Code)).ToList();
                    if (await _context.MenuItemComponents.AnyAsync(a => a.ProductId == entity.Id && a.UnitPriceId.HasValue && removedUnitIds.Contains(a.UnitPriceId.Value)))
                    {
                        _context.ChangeTracker.Clear();
                        return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
                    }

                    // a unit a bill was written in keeps the meaning of that bill's quantities
                    var removedErpUnitIds = removedRows.Select(a => a.UnitId).ToList();
                    if (await _context.BillDetails.AnyAsync(a => a.ItemId == itemId && removedErpUnitIds.Contains(a.UnitId)))
                    {
                        _context.ChangeTracker.Clear();
                        return new ResultMessage { Success = false, Message = "UnitRateLockedByMovements" };
                    }

                    _context.ItemUnits.RemoveRange(removedRows);
                }

                foreach (var unit in entity.Units)
                {
                    var unitId = await _billSupport.GetUnitIdAsync(unit.UnitPriceId);
                    var row = oldRows.FirstOrDefault(a => a.UnitId == unitId);
                    if (row is null)
                    {
                        row = new ItemUnit { ItemId = itemId, UnitId = unitId, CreatedAt = DateTime.Now };
                        await _context.ItemUnits.AddAsync(row);
                    }

                    row.TransRate = (double)unit.TransRate;
                    row.IsDefault = unit.IsDefault;
                    row.LastPurchase = unit.Price;
                    await _context.SaveChangesAsync();

                    // a recipe keeps the rate it was saved with, so it follows the unit when its rate changes
                    await _context.MenuItemComponents
                        .Where(a => a.ProductId == entity.Id && a.UnitPriceId == unit.UnitPriceId)
                        .ExecuteUpdateAsync(a => a.SetProperty(b => b.TransRate, unit.TransRate));
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
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

    public async Task<ResultMessage> DeleteProductAsync(int id)
    {
        try
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            var itemCode = BillSupportRepository.ItemCodePrefix + id;
            if (await _stock.Rows().AnyAsync(a => a.Item.Code == itemCode && a.Quantity > 0))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteHasStock"
                };
            }

            if (await _context.BillDetails.AnyAsync(a => a.Item.Code == itemCode))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteLinkedRecord"
                };
            }

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
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
