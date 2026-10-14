using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class PosBillLedger
{
    public const string SettingCode = "POS";
    private const string ItemCodePrefix = "RST-M";

    public static async Task<BillSetting> GetSettingAsync(ApplicationDbContext context)
    {
        var setting = await context.BillSettings.FirstOrDefaultAsync(a => a.Code == SettingCode);
        if (setting is null)
        {
            setting = new BillSetting { Code = SettingCode, ArName = "فاتورة كاشير", EnName = "Cashier sales invoice", CreatedAt = DateTime.Now };
            await context.BillSettings.AddAsync(setting);
            await context.SaveChangesAsync();
        }

        return setting;
    }

    public static async Task<int?> GetSettingIdAsync(ApplicationDbContext context)
    {
        return await context.BillSettings.AsNoTracking()
            .Where(a => a.Code == SettingCode)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
    }

    public static async Task<Dictionary<int, int>> GetItemIdsAsync(ApplicationDbContext context, IEnumerable<MenuItem> dishes)
    {
        var ids = new Dictionary<int, int>();
        foreach (var dish in dishes)
        {
            var code = ItemCodePrefix + dish.Id;
            var item = await context.Items.FirstOrDefaultAsync(a => a.Code == code);
            if (item is null)
            {
                item = new Item { Code = code, CreatedAt = DateTime.Now };
                await context.Items.AddAsync(item);
            }

            item.ArName = dish.ArName;
            item.EnName = dish.EnName;
            await context.SaveChangesAsync();
            ids[dish.Id] = item.Id;
        }

        return ids;
    }

    public static async Task PostEntryAsync(ApplicationDbContext context, PosBill pos)
    {
        await RemoveEntryAsync(context, pos.Id);
        await new JournalEntryRepository(context).EnsureDefaultAccountsAsync();

        var bill = pos.Bill;
        var total = Math.Round(bill.TotalBillAmountAfterDiscountAndExtra, 2);
        if (total <= 0)
        {
            return;
        }

        var entrySetting = await ErpBillSupport.GetEntrySettingAsync(context, EntryType.JournalSales, "مبيعات يومية", "Daily sales");
        var codes = new[] { AccountCodes.Cash, AccountCodes.AccountsReceivable, AccountCodes.SalesRevenue };
        var accounts = await context.Accounts.AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .ToDictionaryAsync(a => a.Code, a => a.Id);

        int? customerAccountId = null;
        if (pos.OnAccount && pos.CustomerId.HasValue)
        {
            customerAccountId = await context.Customers.AsNoTracking()
                .Where(a => a.Id == pos.CustomerId.Value)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        var debitAccountId = customerAccountId
            ?? (pos.OnAccount ? accounts.GetValueOrDefault(AccountCodes.AccountsReceivable) : accounts.GetValueOrDefault(AccountCodes.Cash));
        if (debitAccountId == 0 || !accounts.TryGetValue(AccountCodes.SalesRevenue, out var salesId))
        {
            return;
        }

        await context.EntryMasters.AddAsync(new EntryMaster
        {
            Code = pos.OrderCode,
            EntryDate = bill.Date,
            TotalBalance = total,
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = "Order " + pos.OrderCode,
            AccountBoxOrBankId = pos.OnAccount ? null : debitAccountId,
            BillId = bill.Id,
            EntrySetting = entrySetting,
            CreatedAt = DateTime.Now,
            EntryDetails = new List<EntryDetail>
            {
                new() { EntryRowCode = 1, AccountId = debitAccountId, Debit = total, Credit = 0, EntryDetailsRemarks = pos.OrderCode, CreatedAt = DateTime.Now },
                new() { EntryRowCode = 2, AccountId = salesId, Debit = 0, Credit = total, EntryDetailsRemarks = pos.OrderCode, CreatedAt = DateTime.Now }
            }
        });
    }

    public static async Task RemoveEntryAsync(ApplicationDbContext context, int billId)
    {
        var entries = await context.EntryMasters
            .Include(a => a.EntryDetails)
            .Where(a => a.BillId == billId)
            .ToListAsync();

        context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        context.EntryMasters.RemoveRange(entries);
    }
}
