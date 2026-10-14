using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class ErpBillMirror
{
    private const string ItemCodePrefix = "P";
    private const string UnitCodePrefix = "U";

    private static readonly Dictionary<InvoiceType, (string Code, string ArName, string EnName, EntryType Entry)> Types = new()
    {
        [InvoiceType.Purchases] = ("PUR", "فاتورة مشتريات", "Purchase invoice", EntryType.JournalPurchases),
        [InvoiceType.Returns] = ("PRET", "مرتجع مشتريات", "Purchase return", EntryType.ReturnJournalPurchases),
        [InvoiceType.Sales] = ("SAL", "فاتورة مبيعات", "Sales invoice", EntryType.JournalSales),
        [InvoiceType.SalesReturn] = ("SRET", "مرتجع مبيعات", "Sales return", EntryType.ReturnJournalSales)
    };

    public static async Task SyncAsync(ApplicationDbContext context, Invoice invoice, IEnumerable<JournalLineRequest> journalLines)
    {
        await RemoveAsync(context, invoice);

        var (code, arName, enName, entryType) = Types[invoice.InvoiceType];
        var setting = await GetBillSettingAsync(context, code, arName, enName);
        var entrySetting = await GetEntrySettingAsync(context, entryType, arName, enName);

        var itemIds = new Dictionary<int, int>();
        var unitIds = new Dictionary<int, int>();
        foreach (var line in invoice.InvoiceItems)
        {
            if (!itemIds.ContainsKey(line.ProductId))
            {
                itemIds[line.ProductId] = await GetItemIdAsync(context, line.ProductId);
            }

            if (!unitIds.ContainsKey(line.UnitPriceId))
            {
                unitIds[line.UnitPriceId] = await GetUnitIdAsync(context, line.UnitPriceId);
            }
        }

        var costs = await context.Products.AsNoTracking()
            .Where(a => itemIds.Keys.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Cost);

        var details = invoice.InvoiceItems.Select((line, index) => new BillDetail
        {
            RowNumber = (byte)(index + 1),
            ItemId = itemIds[line.ProductId],
            UnitId = unitIds[line.UnitPriceId],
            StoreId = invoice.StoreId,
            Quantity = line.Quantity,
            UnitPrice = line.Price,
            TransRate = 1,
            TotalAmount = line.Total,
            UnitCostAtPosting = costs.GetValueOrDefault(line.ProductId),
            CreatedAt = DateTime.Now
        }).ToList();

        var itemsTotal = details.Sum(a => a.TotalAmount ?? 0);
        var cost = IsSale(invoice) ? details.Sum(a => (a.UnitCostAtPosting ?? 0) * (a.Quantity ?? 0)) : (decimal?)null;

        var bill = new BillMaster
        {
            Code = invoice.InvoiceCode,
            CodeWithoutSuffix = invoice.InvoiceCode,
            Date = invoice.InvoiceDate,
            CurrencyRate = 1,
            BillSettingId = setting.Id,
            StoreId = invoice.StoreId,
            CustomerOrSupplierAccountId = await GetPartnerAccountIdAsync(context, invoice),
            TotalAmountBeforeDiscountOrExtra = itemsTotal,
            TotalItemsAmountAfterDiscountAndExtra = itemsTotal,
            BillDiscountValue = invoice.Discount,
            TotalDiscountValueOnBillAndItems = invoice.Discount,
            TotalBillAmountAfterDiscountAndExtra = invoice.TotalPrice,
            PayType = invoice.PayType == PayTypes.Later ? Shared.Enums.PayType.Later : Shared.Enums.PayType.Cash,
            AmountPaided = invoice.Paid,
            CasherRemainingAmount = invoice.Remaining,
            BillDailyNo = await context.BillMasters.CountAsync(a => a.BillSettingId == setting.Id && a.Date.Date == invoice.InvoiceDate.Date) + 1,
            IsStorageEffected = true,
            IsEntryPosted = true,
            BillCost = cost,
            BillProfit = cost.HasValue ? invoice.TotalPrice - cost : null,
            CreatedAt = DateTime.Now,
            BillDetails = details
        };

        await context.BillMasters.AddAsync(bill);

        var sign = StockSign(invoice.InvoiceType);
        foreach (var detail in details)
        {
            var stock = await GetStockRowAsync(context, invoice.StoreId, detail.ItemId.Value, detail.UnitId.Value);
            stock.Quantity += sign * detail.Quantity.Value;
        }

        await AddEntryAsync(context, invoice, bill, entrySetting, journalLines);

        await context.SaveChangesAsync();

        invoice.BillMasterId = bill.Id;
        await context.SaveChangesAsync();
    }

    /// <summary>Takes the stock effect back and deletes the ERP records of an invoice (used by edit and delete).</summary>
    public static async Task RemoveAsync(ApplicationDbContext context, Invoice invoice)
    {
        if (invoice.BillMasterId is null)
        {
            return;
        }

        var billId = invoice.BillMasterId.Value;
        invoice.BillMasterId = null;
        await context.SaveChangesAsync();

        var bill = await context.BillMasters
            .Include(a => a.BillDetails)
            .Include(a => a.BillSetting)
            .FirstOrDefaultAsync(a => a.Id == billId);
        if (bill is null)
        {
            return;
        }

        var sign = StockSign(TypeOf(bill.BillSetting.Code));
        foreach (var detail in bill.BillDetails)
        {
            var stock = await GetStockRowAsync(context, detail.StoreId.Value, detail.ItemId.Value, detail.UnitId.Value);
            stock.Quantity -= sign * detail.Quantity.Value;
        }

        var entries = await context.EntryMasters
            .Include(a => a.EntryDetails)
            .Where(a => a.BillId == billId)
            .ToListAsync();

        context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        context.EntryMasters.RemoveRange(entries);
        context.BillDetails.RemoveRange(bill.BillDetails);
        context.BillMasters.Remove(bill);

        await context.SaveChangesAsync();
    }

    private static async Task AddEntryAsync(
        ApplicationDbContext context,
        Invoice invoice,
        BillMaster bill,
        EntrySetting entrySetting,
        IEnumerable<JournalLineRequest> journalLines)
    {
        var requested = (journalLines ?? Enumerable.Empty<JournalLineRequest>())
            .Where(a => a.Debit > 0 || a.Credit > 0)
            .ToList();
        if (requested.Count == 0)
        {
            return;
        }

        var codes = requested.Select(a => a.AccountCode).Distinct().ToList();
        var accounts = await context.Accounts.AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .ToDictionaryAsync(a => a.Code, a => a.Id);

        var entry = new EntryMaster
        {
            Code = invoice.InvoiceCode,
            EntryDate = invoice.InvoiceDate,
            TotalBalance = requested.Sum(a => Math.Round(a.Debit, 2)),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = $"{invoice.InvoiceType} {invoice.InvoiceCode}",
            AccountBoxOrBankId = invoice.Paid > 0 && accounts.TryGetValue(AccountCodes.Cash, out var cashId) ? cashId : null,
            Bill = bill,
            EntrySetting = entrySetting,
            CreatedAt = DateTime.Now,
            EntryDetails = requested.Select((line, index) => new EntryDetail
            {
                EntryRowCode = (short)(index + 1),
                AccountId = accounts[line.AccountCode],
                Debit = Math.Round(line.Debit, 2),
                Credit = Math.Round(line.Credit, 2),
                EntryDetailsRemarks = line.Description,
                CreatedAt = DateTime.Now
            }).ToList()
        };

        await context.EntryMasters.AddAsync(entry);
    }

    private static bool IsSale(Invoice invoice)
    {
        return invoice.InvoiceType is InvoiceType.Sales or InvoiceType.SalesReturn;
    }

    // Purchases and sales returns bring stock in, sales and purchase returns take it out (same rule as the invoice itself).
    private static int StockSign(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.SalesReturn ? 1 : -1;
    }

    private static InvoiceType TypeOf(string billSettingCode)
    {
        return Types.First(a => a.Value.Code == billSettingCode).Key;
    }

    private static async Task<BillSetting> GetBillSettingAsync(ApplicationDbContext context, string code, string arName, string enName)
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

    internal static async Task<EntrySetting> GetEntrySettingAsync(ApplicationDbContext context, EntryType type, string arName, string enName)
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

    private static async Task<int> GetItemIdAsync(ApplicationDbContext context, int productId)
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

    private static async Task<int> GetUnitIdAsync(ApplicationDbContext context, int unitPriceId)
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

    private static async Task<int?> GetPartnerAccountIdAsync(ApplicationDbContext context, Invoice invoice)
    {
        if (invoice.SupplierId.HasValue)
        {
            return await context.Suppliers.AsNoTracking()
                .Where(a => a.Id == invoice.SupplierId.Value)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        if (invoice.CustomerId.HasValue)
        {
            return await context.Customers.AsNoTracking()
                .Where(a => a.Id == invoice.CustomerId.Value)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        return null;
    }

    private static async Task<StoreItemUnit> GetStockRowAsync(ApplicationDbContext context, int storeId, int itemId, int unitId)
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
        }

        return row;
    }
}
