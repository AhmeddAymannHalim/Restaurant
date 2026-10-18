using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class BillSupport
{
    public const string ItemCodePrefix = "RST-P";
    private const string UnitCodePrefix = "RST-U";

    public const string OrderKind = "ORD";
    public const string TransferCode = "TRF";

    public static readonly Dictionary<InvoiceType, (string Code, BillType BillType, string ArName, string EnName, EntryType Entry)> Types = new()
    {
        [InvoiceType.Purchases] = ("PUR", BillType.Purchases, "مشتريات", "Purchases", EntryType.JournalPurchases),
        [InvoiceType.Returns] = ("PRET", BillType.PurchaseReturn, "مرتجع مشتريات", "Purchases Return", EntryType.ReturnJournalPurchases),
        [InvoiceType.Sales] = ("SAL", BillType.Sales, "مبيعات", "Sales", EntryType.JournalSales),
        [InvoiceType.SalesReturn] = ("SRET", BillType.SalesReturn, "مرتجع مبيعات", "Sales Return", EntryType.ReturnJournalSales),
        [InvoiceType.Shortage] = ("SHRT", BillType.InventoryDeficit, "عجز مخزون", "Inventory Shortage", EntryType.Journal),
        [InvoiceType.Surplus] = ("SRPL", BillType.InventoryIncrease, "زيادة مخزون", "Inventory Surplus", EntryType.Journal),
        [InvoiceType.OpeningInventory] = ("OPEN", BillType.OpeningBalance, "رصيد افتتاحي مخزون", "Opening Inventory", EntryType.Journal)
    };

    public static BillType[] InvoiceBillTypes => Types.Values.Select(a => a.BillType).ToArray();

    public static InvoiceType TypeOf(BillType billType)
    {
        return Types.First(a => a.Value.BillType == billType).Key;
    }

    public static bool IsInvoiceBillType(BillType billType)
    {
        return Types.Values.Any(a => a.BillType == billType);
    }

    public static BillType? BillTypeOfKind(string kind)
    {
        return kind switch
        {
            OrderKind => BillType.Sales,
            TransferCode => BillType.StockTransfers,
            _ => Types.Values.Where(a => a.Code == kind).Select(a => (BillType?)a.BillType).FirstOrDefault()
        };
    }

    // cashier bills are sales bills that have a Orders row; the ERP BillSetting has no code to tell them apart
    public static async Task<bool> IsInvoiceAsync(ApplicationDbContext context, BillMaster bill)
    {
        return bill.BillSetting is not null
            && IsInvoiceBillType(bill.BillSetting.BillType)
            && !await context.Orders.AnyAsync(a => a.Id == bill.Id);
    }

    public static IQueryable<BillMaster> OfKind(this IQueryable<BillMaster> query, ApplicationDbContext context, params string[] kinds)
    {
        var wantPos = kinds.Contains(OrderKind);
        var types = kinds.Where(a => a != OrderKind).Select(BillTypeOfKind).Where(a => a.HasValue).Select(a => a.Value).ToList();

        return query.Where(a => (wantPos && context.Orders.Any(p => p.Id == a.Id))
            || (types.Contains(a.BillSetting.BillType) && !context.Orders.Any(p => p.Id == a.Id)));
    }

    public static async Task<BillSetting> GetBillSettingAsync(ApplicationDbContext context, BillType billType, string arName, string enName)
    {
        var setting = await context.BillSettings.OrderBy(a => a.Id).FirstOrDefaultAsync(a => a.BillType == billType);
        if (setting is null)
        {
            var sellTypeId = await context.SellTypes.OrderBy(a => a.Id).Select(a => (int?)a.Id).FirstOrDefaultAsync();
            if (!sellTypeId.HasValue)
            {
                var sellType = new SellType { ArName = "قطاعى", EnName = "Retail", Show = true, CreatedAt = DateTime.Now };
                await context.SellTypes.AddAsync(sellType);
                await context.SaveChangesAsync();
                sellTypeId = sellType.Id;
            }

            var hasEntry = IsInvoiceBillType(billType);
            setting = new BillSetting
            {
                ArName = arName,
                EnName = enName,
                ArAbrvation = arName,
                EnAbrvation = enName,
                BillType = billType,
                EntryGenerate = hasEntry ? EntryGenerate.AutomaticEntryCreated : EntryGenerate.NoEntryCreated,
                EntryAutoPosted = hasEntry,
                StorageEffect = StorageEffect.AutoEffected,
                CurrencyRate = 1.0,
                DefaultPayType = PayType.Cash,
                SellTypeId = sellTypeId.Value,
                CreatedAt = DateTime.Now
            };
            await context.BillSettings.AddAsync(setting);
            await context.SaveChangesAsync();
        }

        return setting;
    }

    public static async Task<string> NextBillCodeAsync(ApplicationDbContext context, int billSettingId)
    {
        var codes = await context.BillMasters.AsNoTracking()
            .Where(a => a.BillSettingId == billSettingId)
            .OrderByDescending(a => a.Id)
            .Select(a => a.Code)
            .Take(500)
            .ToListAsync();

        var last = codes
            .Select(a => System.Text.RegularExpressions.Regex.Match(a ?? string.Empty, @"(\d+)$"))
            .Where(a => a.Success && int.TryParse(a.Value, out _))
            .Select(a => int.Parse(a.Value))
            .DefaultIfEmpty(0)
            .Max();

        return (last + 1).ToString();
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
            await context.ItemUnits.AddAsync(new ItemUnit { ItemId = itemId, UnitId = unitId, TransRate = 1, IsDefault = true, CreatedAt = DateTime.Now });
        }
    }

    public static async Task RecalculateInvoicePaymentAsync(ApplicationDbContext context, int billMasterId)
    {
        var bill = await context.BillMasters.FirstOrDefaultAsync(a => a.Id == billMasterId)
            ?? context.BillMasters.Local.FirstOrDefault(a => a.Id == billMasterId);
        if (bill is null)
        {
            return;
        }

        var paidBySuppliers = await context.PaymentInvoiceSupplier
            .Where(a => a.BillMasterId == billMasterId)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;
        var paidByCustomers = await context.PaymentInvoiceCustomer
            .Where(a => a.BillMasterId == billMasterId)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        // the invoice's own down payment (entered at invoice creation/edit, not through the
        // dedicated payment screens) is tracked separately in InitialPaidAmount so it isn't lost
        // here, since this method is the only place that recomputes AmountPaided afterwards.
        var paidToDate = bill.InitialPaidAmount + paidBySuppliers + paidByCustomers;

        bill.AmountPaided = paidToDate;
        bill.CasherRemainingAmount = bill.TotalBillAmountAfterDiscountAndExtra - paidToDate;
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
                Quantity = 0,
                CreatedAt = DateTime.Now
            };
            await context.StoreItemUnits.AddAsync(row);

            await EnsureItemUnitAsync(context, itemId, unitId);
        }

        return row;
    }
}
