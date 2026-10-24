using Shared.Helpers;
using Restaurant.DAL.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
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
                .Select(a => new { a.Id, a.Unit.Code, a.TransRate, a.IsDefault, a.Barcode, a.LastPurchase })
                .ToListAsync();
            var unitPrices = await _context.UnitPrices.AsNoTracking().ToListAsync();
            var unitRowIds = unitRows.Select(a => a.Id).ToList();
            var unitPriceRows = await _context.ItemUnitPrices.AsNoTracking().Where(a => unitRowIds.Contains(a.ItemUnitId)).ToListAsync();
            foreach (var row in unitRows)
            {
                var unitPriceId = _billSupport.UnitPriceIdOf(row.Code);
                product.Units.Add(new ProductUnit
                {
                    UnitPriceId = unitPriceId,
                    UnitPrice = unitPrices.FirstOrDefault(a => a.Id == unitPriceId),
                    TransRate = (decimal)row.TransRate,
                    IsDefault = row.IsDefault,
                    Barcode = row.Barcode,
                    LastPurchase = row.LastPurchase ?? 0,
                    Prices = unitPriceRows.Where(a => a.ItemUnitId == row.Id).Select(a => new ProductUnitPrice { SellTypeId = a.SellTypeId, Price = a.Price }).ToList()
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

    public async Task<ResultMessage> UpsertProductAsync(Product entity, IEnumerable<ProductStock> stocks)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (entity.Id == 0)
            {
                entity.Code = await NewProductCodeAsync();
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

                entity.Code = existing.Code;
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }

            // stock: quantities here are in the base unit (the unit whose rate is 1)
            var current = await _stock.QuantitiesAsync();
            foreach (var stock in stocks)
            {
                var oldQuantity = _stock.FindQuantity(current, stock.StoreId, entity.Id);
                var oldMin = _stock.FindMinQuantity(current, stock.StoreId, entity.Id);
                if (oldQuantity == stock.Quantity && oldMin == stock.MinQuantity)
                {
                    continue;
                }

                // move first: it reloads the row, which would drop an unsaved minimum
                if (oldQuantity != stock.Quantity)
                {
                    await _stock.MoveAsync(stock.StoreId, entity.Id, stock.Quantity - oldQuantity, StockMovementSource.OpeningBalance, entity.Code);
                }

                var row = await _stock.RowAsync(stock.StoreId, entity.Id);
                row.MinQuantityOrder = stock.MinQuantity;
            }

            // a store taken off the list no longer holds this product
            var keptStoreIds = stocks.Select(a => a.StoreId).ToList();
            foreach (var old in current.Where(a => a.ProductId == entity.Id && !keptStoreIds.Contains(a.StoreId) && a.Quantity != 0))
            {
                await _stock.MoveAsync(old.StoreId, entity.Id, -old.Quantity, StockMovementSource.OpeningBalance, entity.Code);
            }

            await _context.SaveChangesAsync();

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
                var removedUnitIds = removedRows.Select(a => _billSupport.UnitPriceIdOf(a.Unit.Code)).ToList();
                if (await _context.MenuItemComponents.AnyAsync(a => a.ProductId == entity.Id && a.UnitPriceId.HasValue && removedUnitIds.Contains(a.UnitPriceId.Value)))
                {
                    _context.ChangeTracker.Clear();
                    return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
                }

                _context.ItemUnits.RemoveRange(removedRows);

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
                    row.Barcode = unit.Barcode;
                    row.LastPurchase = unit.LastPurchase;
                    await _context.SaveChangesAsync();

                    // a recipe keeps the rate it was saved with, so it follows the unit when its rate changes
                    await _context.MenuItemComponents
                        .Where(a => a.ProductId == entity.Id && a.UnitPriceId == unit.UnitPriceId)
                        .ExecuteUpdateAsync(a => a.SetProperty(b => b.TransRate, unit.TransRate));

                    // a price type that is not posted (hidden or deleted) keeps whatever it had
                    var oldPrices = await _context.ItemUnitPrices.Where(a => a.ItemUnitId == row.Id).ToListAsync();
                    foreach (var price in unit.Prices)
                    {
                        var oldPrice = oldPrices.FirstOrDefault(a => a.SellTypeId == price.SellTypeId);
                        if (oldPrice is null)
                        {
                            await _context.ItemUnitPrices.AddAsync(new ItemUnitPrice { ItemUnitId = row.Id, SellTypeId = price.SellTypeId, Price = price.Price });
                        }
                        else
                        {
                            oldPrice.Price = price.Price;
                        }
                    }
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
