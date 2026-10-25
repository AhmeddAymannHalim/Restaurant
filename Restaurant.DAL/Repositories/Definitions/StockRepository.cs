using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class StockRepository : IStockRepository
{
    public const string BaseUnitCode = "RST-BASE";

    private readonly ApplicationDbContext _context;
    private readonly IBillSupportRepository _billSupport;

    public StockRepository(ApplicationDbContext context)
    {
        _context = context;
        _billSupport = new BillSupportRepository(context);
    }

    public async Task<int> BaseUnitIdAsync()
    {
        var unit = await _context.Units.FirstOrDefaultAsync(a => a.Code == BaseUnitCode);
        if (unit is null)
        {
            unit = new Unit { Code = BaseUnitCode, ArName = "وحدة", EnName = "Unit", CreatedAt = DateTime.Now };
            await _context.Units.AddAsync(unit);
            await _context.SaveChangesAsync();
        }

        return unit.Id;
    }

    public async Task<StoreItemUnit> RowAsync(int storeId, int productId)
    {
        var itemId = await _billSupport.GetItemIdAsync(productId);
        var unitId = await BaseUnitIdAsync();

        return await _billSupport.GetStockRowAsync(storeId, itemId, unitId);
    }

    public async Task<StoreItemUnit> FindAsync(int storeId, int productId)
    {
        var code = BillSupportRepository.ItemCodePrefix + productId;

        return await _context.StoreItemUnits.FirstOrDefaultAsync(a => a.StoreId == storeId
            && a.Item.Code == code
            && a.Unit.Code == BaseUnitCode);
    }

    public async Task<bool> AdjustAsync(int storeId, int productId, decimal delta, bool allowNegative = false, bool createIfMissing = true)
    {
        var row = createIfMissing
            ? await RowAsync(storeId, productId)
            : await FindAsync(storeId, productId);
        if (row is null)
        {
            return false;
        }

        if (_context.Entry(row).State == EntityState.Added)
        {
            await _context.SaveChangesAsync();
        }

        var id = row.Id;
        var affected = await _context.StoreItemUnits
            .Where(a => a.Id == id && (allowNegative || (a.Quantity ?? 0) + delta >= 0))
            .ExecuteUpdateAsync(a => a.SetProperty(b => b.Quantity, b => (b.Quantity ?? 0) + delta));
        if (affected > 0)
        {
            await _context.Entry(row).ReloadAsync();
        }

        return affected > 0;
    }

    // changes the quantity (it may go below zero here, the caller checks) and writes the movement line for it
    public async Task MoveAsync(int storeId, int productId, decimal delta, StockMovementSource source, string referenceCode)
    {
        await AdjustAsync(storeId, productId, delta, allowNegative: true);
        await _context.StoreProductMovements.AddAsync(new StoreProductMovement
        {
            StoreId = storeId,
            ProductId = productId,
            QuantityDelta = delta,
            Source = source,
            ReferenceCode = referenceCode
        });
    }

    // every product with what one store holds of it, and the units (rate and sell prices) it can be moved in
    public async Task<List<StoreProductLookupItem>> ProductsOfStoreAsync(int storeId)
    {
        var store = await _context.Stores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == storeId);
        var storeArName = store?.ArName;
        var storeEnName = store?.EnName;
        var quantities = await QuantitiesAsync(storeId);
        var products = await _context.Products.AsNoTracking()
            .Select(a => new StoreProductLookupItem
            {
                Id = a.Id, ProductId = a.Id, ProductCategoryId = a.ProductCategoryId, StoreId = storeId,
                ProductArName = a.ArName, ProductEnName = a.EnName, StoreArName = storeArName, StoreEnName = storeEnName, Cost = a.Cost
            })
            .ToListAsync();

        var unitRows = await _context.ItemUnits.AsNoTracking()
            .Where(a => a.Item.Code.StartsWith(BillSupportRepository.ItemCodePrefix) && a.Unit.Code.StartsWith(BillSupportRepository.UnitCodePrefix))
            .Select(a => new { Row = a, ItemCode = a.Item.Code, UnitCode = a.Unit.Code })
            .ToListAsync();
        var sellTypeIds = await _context.SellTypes.AsNoTracking().Where(a => a.Show).OrderBy(a => a.Id).Select(a => a.Id).ToListAsync();
        var unitNames = await _context.UnitPrices.AsNoTracking().ToListAsync();

        foreach (var product in products)
        {
            product.Quantity = FindQuantity(quantities, storeId, product.ProductId);
            foreach (var row in unitRows.Where(a => _billSupport.ProductIdOf(a.ItemCode) == product.ProductId))
            {
                var unitName = unitNames.FirstOrDefault(a => a.Id == _billSupport.UnitPriceIdOf(row.UnitCode));
                if (unitName is null)
                {
                    continue;
                }

                product.Units.Add(new ProductUnitViewModel
                {
                    UnitPriceId = unitName.Id,
                    UnitArName = unitName.ArName,
                    UnitEnName = unitName.EnName,
                    TransRate = (decimal)row.Row.TransRate,
                    IsDefault = row.Row.IsDefault,
                    Prices = sellTypeIds.Where(a => _billSupport.PriceOf(row.Row, a) != null).Select(a => new ProductUnitPriceViewModel { SellTypeId = a, Price = _billSupport.PriceOf(row.Row, a).Value }).ToList()
                });
            }
        }

        return products;
    }

    public IQueryable<StoreItemUnit> Rows()
    {
        return _context.StoreItemUnits.Where(a => a.Item.Code.StartsWith(BillSupportRepository.ItemCodePrefix) && a.Unit.Code == BaseUnitCode);
    }

    // The quantity of every product in every store (or in one store).
    public async Task<List<StockQuantity>> QuantitiesAsync(int? storeId = null)
    {
        var rows = await Rows().AsNoTracking()
            .Where(a => storeId == null || a.StoreId == storeId)
            .Select(a => new { a.StoreId, a.Item.Code, a.Quantity, a.MinQuantityOrder })
            .ToListAsync();

        var quantities = new List<StockQuantity>();
        foreach (var row in rows)
        {
            if (!row.StoreId.HasValue)
            {
                continue;
            }

            quantities.Add(new StockQuantity
            {
                StoreId = row.StoreId.Value,
                ProductId = _billSupport.ProductIdOf(row.Code),
                Quantity = row.Quantity ?? 0,
                MinQuantity = row.MinQuantityOrder ?? 0
            });
        }

        return quantities;
    }

    // The reorder limit of a product in one store. 0 when none was set.
    public decimal FindMinQuantity(List<StockQuantity> quantities, int storeId, int productId)
    {
        foreach (var stock in quantities)
        {
            if (stock.StoreId == storeId && stock.ProductId == productId)
            {
                return stock.MinQuantity;
            }
        }

        return 0;
    }

    // How much of a product one store holds. A product that was never stocked there has 0.
    public decimal FindQuantity(List<StockQuantity> quantities, int storeId, int productId)
    {
        foreach (var stock in quantities)
        {
            if (stock.StoreId == storeId && stock.ProductId == productId)
            {
                return stock.Quantity;
            }
        }

        return 0;
    }
}
