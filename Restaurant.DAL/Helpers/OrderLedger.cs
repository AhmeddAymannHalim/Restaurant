using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class OrderLedger
{
    private const string ItemCodePrefix = "RST-M";

    public static async Task<BillSetting> GetSettingAsync(ApplicationDbContext context)
    {
        return await BillSupport.GetBillSettingAsync(context, BillType.Sales, "مبيعات", "Sales");
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

    public static async Task PostEntryAsync(ApplicationDbContext context, Order pos)
    {
        await RemoveEntryAsync(context, pos.Id);
        await new JournalEntryRepository(context).EnsureDefaultAccountsAsync();

        var bill = pos.Bill;
        var total = Math.Round(bill.TotalBillAmountAfterDiscountAndExtra, 2);
        if (total <= 0)
        {
            return;
        }

        var entrySetting = await BillSupport.GetEntrySettingAsync(context, EntryType.JournalSales, "مبيعات يومية", "Daily sales");
        var codes = new[] { AccountCodes.Cash, AccountCodes.AccountsReceivable, AccountCodes.SalesRevenue };
        var accountEntities = await context.Accounts
            .Where(a => codes.Contains(a.Code))
            .ToListAsync();
        var accounts = accountEntities.ToDictionary(a => a.Code, a => a.Id);

        Account customerAccount = null;
        if (pos.OnAccount && pos.CustomerId.HasValue)
        {
            var customerAccountId = await context.Customers.AsNoTracking()
                .Where(a => a.Id == pos.CustomerId.Value)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync();
            if (customerAccountId.HasValue)
            {
                customerAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Id == customerAccountId.Value);
            }
        }

        var debitAccountId = customerAccount?.Id
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
        if (customerAccount is null)
        {
            var debitAccount = accountEntities.First(a => a.Id == debitAccountId);
            debitAccount.Adjust(total);
        }

        var salesAccount = accountEntities.First(a => a.Id == salesId);
        salesAccount.Adjust(-total);
    }

    public static async Task RemoveEntryAsync(ApplicationDbContext context, int billId)
    {
        var entries = await context.EntryMasters
            .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
            .Where(a => a.BillId == billId)
            .ToListAsync();

        foreach (var detail in entries.SelectMany(a => a.EntryDetails))
        {
            if (detail.Account is not null && detail.Account.AccountType is not (AccountType.Customer or AccountType.Supplier))
            {
                detail.Account.Adjust(-(detail.Debit - detail.Credit));
            }
        }

        context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        context.EntryMasters.RemoveRange(entries);
    }
}
