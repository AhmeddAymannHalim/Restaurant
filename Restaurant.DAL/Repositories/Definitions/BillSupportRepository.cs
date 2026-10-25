using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Repositories.Definitions;

public class BillSupportRepository : IBillSupportRepository
{
    public const string ItemCodePrefix = "RST-P";
    public const string UnitCodePrefix = "RST-U";

    public const string OrderKind = "ORD";
    public const string TransferCode = "TRF";

    private readonly ApplicationDbContext _context;

    public BillSupportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<InvoiceTypeInfo> GetAllInvoiceTypes()
    {
        var types = new List<InvoiceTypeInfo>();

        types.Add(new InvoiceTypeInfo { Type = InvoiceType.Purchases, Code = "PUR", BillType = BillType.Purchases, ArName = "مشتريات", EnName = "Purchases", Entry = EntryType.JournalPurchases });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.Returns, Code = "PRET", BillType = BillType.PurchaseReturn, ArName = "مرتجع مشتريات", EnName = "Purchases Return", Entry = EntryType.ReturnJournalPurchases });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.Sales, Code = "SAL", BillType = BillType.Sales, ArName = "مبيعات", EnName = "Sales", Entry = EntryType.JournalSales });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.SalesReturn, Code = "SRET", BillType = BillType.SalesReturn, ArName = "مرتجع مبيعات", EnName = "Sales Return", Entry = EntryType.ReturnJournalSales });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.Shortage, Code = "SHRT", BillType = BillType.InventoryDeficit, ArName = "عجز مخزون", EnName = "Inventory Shortage", Entry = EntryType.Journal });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.Surplus, Code = "SRPL", BillType = BillType.InventoryIncrease, ArName = "زيادة مخزون", EnName = "Inventory Surplus", Entry = EntryType.Journal });
        types.Add(new InvoiceTypeInfo { Type = InvoiceType.OpeningInventory, Code = "OPEN", BillType = BillType.OpeningBalance, ArName = "رصيد افتتاحي مخزون", EnName = "Opening Inventory", Entry = EntryType.Journal });

        return types;
    }

    public InvoiceTypeInfo GetInvoiceType(InvoiceType type)
    {
        foreach (var info in GetAllInvoiceTypes())
        {
            if (info.Type == type)
            {
                return info;
            }
        }

        return null;
    }

    // The bill types that are invoices (not cashier orders or transfers).
    public List<BillType> GetInvoiceBillTypes()
    {
        var billTypes = new List<BillType>();

        foreach (var info in GetAllInvoiceTypes())
        {
            billTypes.Add(info.BillType);
        }

        return billTypes;
    }

    public InvoiceType TypeOf(BillType billType)
    {
        foreach (var info in GetAllInvoiceTypes())
        {
            if (info.BillType == billType)
            {
                return info.Type;
            }
        }

        throw new InvalidOperationException("No invoice type for bill type " + billType);
    }

    public bool IsInvoiceBillType(BillType billType)
    {
        foreach (var info in GetAllInvoiceTypes())
        {
            if (info.BillType == billType)
            {
                return true;
            }
        }

        return false;
    }

    public BillType? BillTypeOfKind(string kind)
    {
        if (kind == OrderKind)
        {
            return BillType.Sales;
        }

        if (kind == TransferCode)
        {
            return BillType.StockTransfers;
        }

        foreach (var info in GetAllInvoiceTypes())
        {
            if (info.Code == kind)
            {
                return info.BillType;
            }
        }

        return null;
    }

    public async Task<bool> IsInvoiceAsync(BillMaster bill)
    {
        return bill.BillSetting is not null
            && IsInvoiceBillType(bill.BillSetting.BillType)
            && !await _context.Orders.AnyAsync(a => a.Id == bill.Id);
    }

    public IQueryable<BillMaster> OfKind(IQueryable<BillMaster> query, params string[] kinds)
    {
        var wantOrders = kinds.Contains(OrderKind) || kinds.Contains("SAL");

        var billTypes = new List<BillType>();
        foreach (var kind in kinds)
        {
            if (kind == OrderKind)
            {
                continue;
            }

            var billType = BillTypeOfKind(kind);
            if (billType.HasValue)
            {
                billTypes.Add(billType.Value);
            }
        }

        return query.Where(a => (wantOrders && _context.Orders.Any(p => p.Id == a.Id))
            || (billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id)));
    }

    public async Task<BillSetting> GetBillSettingAsync(BillType billType, string arName, string enName)
    {
        var setting = await _context.BillSettings.OrderBy(a => a.Id).FirstOrDefaultAsync(a => a.BillType == billType);
        if (setting is null)
        {
            var sellTypeId = await _context.SellTypes.OrderBy(a => a.Id).Select(a => (int?)a.Id).FirstOrDefaultAsync();
            if (!sellTypeId.HasValue)
            {
                var sellType = new SellType { ArName = "قطاعى", EnName = "Retail", Show = true, CreatedAt = DateTime.Now };
                await _context.SellTypes.AddAsync(sellType);
                await _context.SaveChangesAsync();
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
            await _context.BillSettings.AddAsync(setting);
            await _context.SaveChangesAsync();
        }

        return setting;
    }

    public async Task<string> NextBillCodeAsync(int billSettingId)
    {
        var codes = await _context.BillMasters.AsNoTracking()
            .Where(a => a.BillSettingId == billSettingId)
            .OrderByDescending(a => a.Id)
            .Select(a => a.Code)
            .Take(500)
            .ToListAsync();

        var highest = 0;
        foreach (var code in codes)
        {
            var number = 0;
            if (!string.IsNullOrEmpty(code))
            {
                var digits = "";
                for (var i = code.Length - 1; i >= 0 && char.IsDigit(code[i]); i--)
                {
                    digits = code[i] + digits;
                }

                int.TryParse(digits, out number);
            }
            if (number > highest)
            {
                highest = number;
            }
        }

        return (highest + 1).ToString();
    }

    public async Task<EntrySetting> GetEntrySettingAsync(EntryType type, string arName, string enName)
    {
        var setting = await _context.EntrySettings.FirstOrDefaultAsync(a => a.EntryType == type);
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
            await _context.EntrySettings.AddAsync(setting);
            await _context.SaveChangesAsync();
        }

        return setting;
    }

    public async Task<int> GetItemIdAsync(int productId)
    {
        var code = ItemCodePrefix + productId;
        var product = await _context.Products.AsNoTracking().FirstAsync(a => a.Id == productId);
        var item = await _context.Items.FirstOrDefaultAsync(a => a.Code == code);
        if (item is null)
        {
            item = new Item { Code = code, CreatedAt = DateTime.Now };
            await _context.Items.AddAsync(item);
        }

        item.ArName = product.ArName;
        item.EnName = product.EnName;
        await _context.SaveChangesAsync();

        return item.Id;
    }

    public async Task<int> GetUnitIdAsync(int unitPriceId)
    {
        var code = UnitCodePrefix + unitPriceId;
        var source = await _context.UnitPrices.AsNoTracking().FirstAsync(a => a.Id == unitPriceId);
        var unit = await _context.Units.FirstOrDefaultAsync(a => a.Code == code);
        if (unit is null)
        {
            unit = new Unit { Code = code, CreatedAt = DateTime.Now };
            await _context.Units.AddAsync(unit);
        }

        unit.ArName = source.ArName;
        unit.EnName = source.EnName;
        await _context.SaveChangesAsync();

        return unit.Id;
    }

    public int ProductIdOf(string itemCode)
    {
        var number = 0;
        if (itemCode != null && itemCode.StartsWith(ItemCodePrefix))
        {
            int.TryParse(itemCode.Substring(ItemCodePrefix.Length), out number);
        }

        return number;
    }

    // ERP price types: SellType id 1..6 is Whole, HalfWhole, Retail, EndUsers, WeightedAverage, Export (id 7 "Last Buy" is LastPurchase)
    public decimal? PriceOf(ItemUnit row, int sellTypeId)
    {
        switch (sellTypeId)
        {
            case 1: return row.Whole;
            case 2: return row.HalfWhole;
            case 3: return row.Retail;
            case 4: return row.EndUsers;
            case 5: return row.WeightedAverage;
            case 6: return row.Export;
            default: return null;
        }
    }

    public void SetPriceOf(ItemUnit row, int sellTypeId, decimal price)
    {
        switch (sellTypeId)
        {
            case 1: row.Whole = price; break;
            case 2: row.HalfWhole = price; break;
            case 3: row.Retail = price; break;
            case 4: row.EndUsers = price; break;
            case 5: row.WeightedAverage = price; break;
            case 6: row.Export = price; break;
        }
    }

    public int UnitPriceIdOf(string unitCode)
    {
        var number = 0;
        if (unitCode != null && unitCode.StartsWith(UnitCodePrefix))
        {
            int.TryParse(unitCode.Substring(UnitCodePrefix.Length), out number);
        }

        return number;
    }

    public async Task EnsureItemUnitAsync(int itemId, int unitId)
    {
        var hasUnit = await _context.ItemUnits.AnyAsync(a => a.ItemId == itemId && a.UnitId == unitId)
            || _context.ItemUnits.Local.Any(a => a.ItemId == itemId && a.UnitId == unitId);
        if (!hasUnit)
        {
            // only the first unit of an item is its default
            var hasAnyUnit = await _context.ItemUnits.AnyAsync(a => a.ItemId == itemId)
                || _context.ItemUnits.Local.Any(a => a.ItemId == itemId);
            await _context.ItemUnits.AddAsync(new ItemUnit { ItemId = itemId, UnitId = unitId, TransRate = 1, IsDefault = !hasAnyUnit, CreatedAt = DateTime.Now });
        }
    }

    // the ERP keeps the id of the bill's journal entry on the bill (BillMaster.EntryId); the entry points back with BillId
    public async Task LinkEntryAsync(BillMaster bill)
    {
        var entryId = await _context.EntryMasters.AsNoTracking()
            .Where(a => a.BillId == bill.Id)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
        if (bill.EntryId != entryId)
        {
            bill.EntryId = entryId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RecalculateInvoicePaymentAsync(int billMasterId)
    {
        var bill = await _context.BillMasters.FirstOrDefaultAsync(a => a.Id == billMasterId)
            ?? _context.BillMasters.Local.FirstOrDefault(a => a.Id == billMasterId);
        if (bill is null)
        {
            return;
        }

        var paidBySuppliers = await _context.PaymentInvoiceSupplier
            .Where(a => a.BillMasterId == billMasterId)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;
        var paidByCustomers = await _context.PaymentInvoiceCustomer
            .Where(a => a.BillMasterId == billMasterId)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        // the invoice's own down payment (entered at invoice creation/edit, not through the
        // dedicated payment screens) is its payment lines, so it isn't lost here, since this
        // method is the only place that recomputes AmountPaided afterwards.
        var downPayment = await _context.PaymentMethods
            .Where(a => a.BillMasterId == billMasterId)
            .SumAsync(a => (decimal?)a.AmountPaided) ?? 0;
        var paidToDate = downPayment + paidBySuppliers + paidByCustomers;

        bill.AmountPaided = paidToDate;
        bill.CasherRemainingAmount = bill.TotalBillAmountAfterDiscountAndExtra - paidToDate;
    }

    public async Task<StoreItemUnit> GetStockRowAsync(int storeId, int itemId, int unitId)
    {
        var row = await _context.StoreItemUnits.FirstOrDefaultAsync(a => a.StoreId == storeId && a.ItemId == itemId && a.UnitId == unitId)
            ?? _context.StoreItemUnits.Local.FirstOrDefault(a => a.StoreId == storeId && a.ItemId == itemId && a.UnitId == unitId);
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
            await _context.StoreItemUnits.AddAsync(row);

            await EnsureItemUnitAsync(itemId, unitId);
        }

        return row;
    }
}
