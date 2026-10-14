using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class ErpBillSupport
{
    public const string ItemCodePrefix = "RST-P";
    private const string UnitCodePrefix = "RST-U";

    public static readonly Dictionary<InvoiceType, (string Code, string ArName, string EnName, EntryType Entry)> Types = new()
    {
        [InvoiceType.Purchases] = ("PUR", "فاتورة مشتريات", "Purchase invoice", EntryType.JournalPurchases),
        [InvoiceType.Returns] = ("PRET", "مرتجع مشتريات", "Purchase return", EntryType.ReturnJournalPurchases),
        [InvoiceType.Sales] = ("SAL", "فاتورة مبيعات", "Sales invoice", EntryType.JournalSales),
        [InvoiceType.SalesReturn] = ("SRET", "مرتجع مبيعات", "Sales return", EntryType.ReturnJournalSales)
    };

    public static string[] InvoiceSettingCodes => Types.Values.Select(a => a.Code).ToArray();

    public static InvoiceType TypeOf(string billSettingCode)
    {
        return Types.First(a => a.Value.Code == billSettingCode).Key;
    }

    public static bool IsInvoiceSetting(string billSettingCode)
    {
        return Types.Values.Any(a => a.Code == billSettingCode);
    }

    public static async Task<BillSetting> GetBillSettingAsync(ApplicationDbContext context, string code, string arName, string enName)
    {
        var setting = await context.BillSettings.FirstOrDefaultAsync(a => a.Code == code);
        if (setting is null)
        {
            setting = new BillSetting { Code = code, ArName = arName, EnName = enName, CreatedAt = DateTime.Now };
            await context.BillSettings.AddAsync(setting);
            await context.SaveChangesAsync();
        }

        return setting;
    }

    public static async Task<EntrySetting> GetEntrySettingAsync(ApplicationDbContext context, EntryType type, string arName, string enName)
    {
        var setting = await context.EntrySettings.FirstOrDefaultAsync(a => a.EntryType == type);
        if (setting is null)
        {
            setting = new EntrySetting
            {
                EntryType = type,
                ArName = arName,
                EnName = enName,
                ArAbbreviation = arName,
                EnAbbreviation = enName,
                AutoPosting = true,
                CreatedAt = DateTime.Now
            };
            await context.EntrySettings.AddAsync(setting);
            await context.SaveChangesAsync();
        }

        return setting;
    }

    public static async Task<int> GetItemIdAsync(ApplicationDbContext context, int productId)
    {
        var code = ItemCodePrefix + productId;
        var product = await context.Products.AsNoTracking().FirstAsync(a => a.Id == productId);
        var item = await context.Items.FirstOrDefaultAsync(a => a.Code == code);
        if (item is null)
        {
            item = new Item { Code = code, CreatedAt = DateTime.Now };
            await context.Items.AddAsync(item);
        }

        item.ArName = product.ArName;
        item.EnName = product.EnName;
        await context.SaveChangesAsync();

        return item.Id;
    }

    public static async Task<int> GetUnitIdAsync(ApplicationDbContext context, int unitPriceId)
    {
        var code = UnitCodePrefix + unitPriceId;
        var source = await context.UnitPrices.AsNoTracking().FirstAsync(a => a.Id == unitPriceId);
        var unit = await context.Units.FirstOrDefaultAsync(a => a.Code == code);
        if (unit is null)
        {
            unit = new Unit { Code = code, CreatedAt = DateTime.Now };
            await context.Units.AddAsync(unit);
        }

        unit.ArName = source.ArName;
        unit.EnName = source.EnName;
        await context.SaveChangesAsync();

        return unit.Id;
    }

    public static int ProductIdOf(string itemCode)
    {
        return itemCode is not null && itemCode.StartsWith(ItemCodePrefix) && int.TryParse(itemCode.Substring(ItemCodePrefix.Length), out var id) ? id : 0;
    }

    public static int UnitPriceIdOf(string unitCode)
    {
        return unitCode is not null && unitCode.StartsWith(UnitCodePrefix) && int.TryParse(unitCode.Substring(UnitCodePrefix.Length), out var id) ? id : 0;
    }

    public static async Task EnsureItemUnitAsync(ApplicationDbContext context, int itemId, int unitId)
    {
        var hasUnit = await context.ItemUnits.AnyAsync(a => a.ItemId == itemId && a.UnitId == unitId)
            || context.ItemUnits.Local.Any(a => a.ItemId == itemId && a.UnitId == unitId);
        if (!hasUnit)
        {
            await context.ItemUnits.AddAsync(new ItemUnit { ItemId = itemId, UnitId = unitId, Factor = 1, CreatedAt = DateTime.Now });
        }
    }

    public static async Task<StoreItemUnit> GetStockRowAsync(ApplicationDbContext context, int storeId, int itemId, int unitId)
    {
        var row = await context.StoreItemUnits.FirstOrDefaultAsync(a => a.StoreId == storeId && a.ItemId == itemId && a.UnitId == unitId)
            ?? context.StoreItemUnits.Local.FirstOrDefault(a => a.StoreId == storeId && a.ItemId == itemId && a.UnitId == unitId);
        if (row is null)
        {
            row = new StoreItemUnit
            {
                StoreId = storeId,
                ItemId = itemId,
                UnitId = unitId,
                CreatedAt = DateTime.Now
            };
            await context.StoreItemUnits.AddAsync(row);

            await EnsureItemUnitAsync(context, itemId, unitId);
        }

        return row;
    }
}
