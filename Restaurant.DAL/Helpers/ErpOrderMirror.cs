using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

/// <summary>
/// A cashier order is the restaurant's own record. When the customer pays, the ERP gets the matching sales bill
/// (BillMaster/BillDetail, one Item per menu dish) and its journal entry (cash or receivable against sales revenue).
/// The ERP bill does not touch stock: the order already deducted the dish ingredients from the restaurant's stock.
/// </summary>
public static class ErpOrderMirror
{
    private const string SettingCode = "POS";
    private const string ItemCodePrefix = "M";

    public static async Task SyncAsync(ApplicationDbContext context, Order order)
    {
        await RemoveAsync(context, order);
        await new JournalEntryRepository(context).EnsureDefaultAccountsAsync();

        var setting = await context.BillSettings.FirstOrDefaultAsync(a => a.Code == SettingCode);
        if (setting is null)
        {
            setting = new BillSetting { Code = SettingCode, ArName = "فاتورة كاشير", EnName = "Cashier sales invoice", CreatedAt = DateTime.Now };
            await context.BillSettings.AddAsync(setting);
            await context.SaveChangesAsync();
        }

        var entrySetting = await ErpBillMirror.GetEntrySettingAsync(context, EntryType.JournalSales, "مبيعات يومية", "Daily sales");

        var lines = await context.OrderMenuItems.AsNoTracking()
            .Where(a => a.OrderId == order.Id)
            .Select(a => new { a.MenuItemId, a.MenuItem.ArName, a.MenuItem.EnName, a.Quantity, a.PriceForUnit, a.UnitCost, a.Price })
            .ToListAsync();

        var details = new List<BillDetail>();
        foreach (var line in lines)
        {
            details.Add(new BillDetail
            {
                RowNumber = (byte)(details.Count + 1),
                ItemId = await GetItemIdAsync(context, line.MenuItemId, line.ArName, line.EnName),
                Quantity = line.Quantity,
                UnitPrice = line.PriceForUnit,
                TransRate = 1,
                TotalAmount = line.Price,
                UnitCostAtPosting = line.UnitCost,
                CreatedAt = DateTime.Now
            });
        }

        var itemsTotal = details.Sum(a => a.TotalAmount ?? 0);
        var cost = lines.Sum(a => a.UnitCost * a.Quantity);
        var accountCodes = new[] { AccountCodes.Cash, AccountCodes.AccountsReceivable, AccountCodes.SalesRevenue };
        var accounts = await context.Accounts.AsNoTracking()
            .Where(a => accountCodes.Contains(a.Code))
            .ToDictionaryAsync(a => a.Code, a => a.Id);
        var customerAccountId = order.OnAccount && order.CustomerId.HasValue
            ? await context.Customers.AsNoTracking().Where(a => a.Id == order.CustomerId.Value).Select(a => a.AccountId).FirstOrDefaultAsync()
            : null;
        var extras = order.Service + order.Tax + order.DeliveryFee;

        var bill = new BillMaster
        {
            Code = order.OrderCode,
            CodeWithoutSuffix = order.OrderCode,
            Date = order.OrderDate,
            CurrencyRate = 1,
            BillSettingId = setting.Id,
            CustomerOrSupplierAccountId = customerAccountId,
            TotalAmountBeforeDiscountOrExtra = itemsTotal,
            TotalItemsAmountAfterDiscountAndExtra = itemsTotal,
            BillDiscountValue = order.Discount,
            TotalDiscountValueOnBillAndItems = order.Discount,
            BillExtraValue = extras,
            TotalExtraValueOnBillAndItems = extras,
            TotalBillAmountAfterDiscountAndExtra = order.TotalPrice,
            PayType = order.OnAccount ? Shared.Enums.PayType.Later : Shared.Enums.PayType.Cash,
            AmountPaided = order.OnAccount ? 0 : order.TotalPrice,
            CasherRemainingAmount = order.OnAccount ? order.TotalPrice : 0,
            BillDailyNo = await context.BillMasters.CountAsync(a => a.BillSettingId == setting.Id && a.Date.Date == order.OrderDate.Date) + 1,
            IsStorageEffected = false,
            IsEntryPosted = true,
            BillCost = cost,
            BillProfit = order.TotalPrice - cost,
            CreatedAt = DateTime.Now,
            BillDetails = details
        };
        await context.BillMasters.AddAsync(bill);

        var debitAccountId = customerAccountId
            ?? (order.OnAccount ? accounts.GetValueOrDefault(AccountCodes.AccountsReceivable) : accounts.GetValueOrDefault(AccountCodes.Cash));
        if (order.TotalPrice > 0 && debitAccountId > 0 && accounts.TryGetValue(AccountCodes.SalesRevenue, out var salesId))
        {
            var total = Math.Round(order.TotalPrice, 2);
            await context.EntryMasters.AddAsync(new EntryMaster
            {
                Code = order.OrderCode,
                EntryDate = order.OrderDate,
                TotalBalance = total,
                CurrencyRate = 1,
                IsPosted = true,
                EntryRemarks = "Order " + order.OrderCode,
                AccountBoxOrBankId = order.OnAccount ? null : debitAccountId,
                Bill = bill,
                EntrySetting = entrySetting,
                CreatedAt = DateTime.Now,
                EntryDetails = new List<EntryDetail>
                {
                    new() { EntryRowCode = 1, AccountId = debitAccountId, Debit = total, Credit = 0, EntryDetailsRemarks = order.OrderCode, CreatedAt = DateTime.Now },
                    new() { EntryRowCode = 2, AccountId = salesId, Debit = 0, Credit = total, EntryDetailsRemarks = order.OrderCode, CreatedAt = DateTime.Now }
                }
            });
        }

        await context.SaveChangesAsync();
    }

    public static async Task RemoveAsync(ApplicationDbContext context, Order order)
    {
        var settingId = await context.BillSettings.AsNoTracking()
            .Where(a => a.Code == SettingCode)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
        if (settingId is null || string.IsNullOrEmpty(order.OrderCode))
        {
            return;
        }

        var bills = await context.BillMasters
            .Include(a => a.BillDetails)
            .Where(a => a.BillSettingId == settingId && a.Code == order.OrderCode)
            .ToListAsync();
        if (bills.Count == 0)
        {
            return;
        }

        var ids = bills.Select(a => (int?)a.Id).ToList();
        var entries = await context.EntryMasters
            .Include(a => a.EntryDetails)
            .Where(a => ids.Contains(a.BillId))
            .ToListAsync();

        context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        context.EntryMasters.RemoveRange(entries);
        context.BillDetails.RemoveRange(bills.SelectMany(a => a.BillDetails));
        context.BillMasters.RemoveRange(bills);
        await context.SaveChangesAsync();
    }

    private static async Task<int> GetItemIdAsync(ApplicationDbContext context, int menuItemId, string arName, string enName)
    {
        var code = ItemCodePrefix + menuItemId;
        var item = await context.Items.FirstOrDefaultAsync(a => a.Code == code);
        if (item is null)
        {
            item = new Item { Code = code, CreatedAt = DateTime.Now };
            await context.Items.AddAsync(item);
        }

        item.ArName = arName;
        item.EnName = enName;
        await context.SaveChangesAsync();

        return item.Id;
    }
}
