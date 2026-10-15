using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Helpers;

public static class StoreStock
{
    public const string BaseUnitCode = "RST-BASE";

    public static async Task<int> BaseUnitIdAsync(ApplicationDbContext context)
    {
        var unit = await context.Units.FirstOrDefaultAsync(a => a.Code == BaseUnitCode);
        if (unit is null)
        {
            unit = new Unit { Code = BaseUnitCode, ArName = "وحدة", EnName = "Unit", CreatedAt = DateTime.Now };
            await context.Units.AddAsync(unit);
            await context.SaveChangesAsync();
        }

        return unit.Id;
    }

    public static async Task<StoreItemUnit> RowAsync(ApplicationDbContext context, int storeId, int productId)
    {
        var itemId = await BillSupport.GetItemIdAsync(context, productId);
        var unitId = await BaseUnitIdAsync(context);

        return await BillSupport.GetStockRowAsync(context, storeId, itemId, unitId);
    }

    public static async Task<StoreItemUnit> FindAsync(ApplicationDbContext context, int storeId, int productId)
    {
        var code = BillSupport.ItemCodePrefix + productId;

        return await context.StoreItemUnits.FirstOrDefaultAsync(a => a.StoreId == storeId
            && a.Item.Code == code
            && a.Unit.Code == BaseUnitCode);
    }

    public static async Task<bool> AdjustAsync(ApplicationDbContext context, int storeId, int productId, decimal delta, bool allowNegative = false, bool createIfMissing = true)
    {
        var row = createIfMissing
            ? await RowAsync(context, storeId, productId)
            : await FindAsync(context, storeId, productId);
        if (row is null)
        {
            return false;
        }

        if (context.Entry(row).State == EntityState.Added)
        {
            await context.SaveChangesAsync();
        }

        var id = row.Id;
        var affected = await context.StoreItemUnits
            .Where(a => a.Id == id && (allowNegative || (a.Quantity ?? 0) + delta >= 0))
            .ExecuteUpdateAsync(a => a.SetProperty(b => b.Quantity, b => (b.Quantity ?? 0) + delta));
        if (affected > 0)
        {
            await context.Entry(row).ReloadAsync();
        }

        return affected > 0;
    }

    public static IQueryable<StoreItemUnit> Rows(ApplicationDbContext context)
    {
        return context.StoreItemUnits.Where(a => a.Item.Code.StartsWith(BillSupport.ItemCodePrefix) && a.Unit.Code == BaseUnitCode);
    }

    public static async Task<Dictionary<(int StoreId, int ProductId), decimal>> QuantitiesAsync(ApplicationDbContext context, int? storeId = null)
    {
        var rows = await Rows(context).AsNoTracking()
            .Where(a => storeId == null || a.StoreId == storeId)
            .Select(a => new { a.StoreId, a.Item.Code, a.Quantity })
            .ToListAsync();

        return rows
            .Where(a => a.StoreId.HasValue)
            .ToDictionary(a => (a.StoreId.Value, BillSupport.ProductIdOf(a.Code)), a => a.Quantity ?? 0);
    }
}
