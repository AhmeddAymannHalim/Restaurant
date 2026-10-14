using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class ErpEntryMirror
{
    private static readonly Dictionary<JournalSourceType, (EntryType Type, string ArName, string EnName)> Types = new()
    {
        [JournalSourceType.CustomerPaymentVoucher] = (EntryType.CustomerPayment, "دفعات عملاء", "Customer payment"),
        [JournalSourceType.SupplierPaymentVoucher] = (EntryType.SupplierPayment, "دفعات موردين", "Supplier payment"),
        [JournalSourceType.PaymentVoucher] = (EntryType.CashPayment, "صرف", "Cash payment"),
        [JournalSourceType.Manual] = (EntryType.Journal, "قيد يومي", "Journal")
    };

    public static async Task AddAsync(ApplicationDbContext context, JournalEntry journal)
    {
        if (journal.SourceType == JournalSourceType.Invoice || !Types.TryGetValue(journal.SourceType, out var type))
        {
            return;
        }

        var setting = await ErpBillMirror.GetEntrySettingAsync(context, type.Type, type.ArName, type.EnName);

        var cashAccountId = await context.Accounts.AsNoTracking()
            .Where(a => a.Code == AccountCodes.Cash)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
        var usesCash = cashAccountId.HasValue && journal.Lines.Any(a => a.AccountId == cashAccountId.Value);

        var entry = new EntryMaster
        {
            Code = journal.EntryNumber,
            EntryDate = journal.Date,
            TotalBalance = journal.Lines.Sum(a => a.Debit),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = journal.Description,
            CollectEntryCode = SourceTag(journal.SourceType, journal.SourceId),
            AccountBoxOrBankId = usesCash ? cashAccountId : null,
            EntrySetting = setting,
            CreatedAt = DateTime.Now,
            EntryDetails = journal.Lines.Select((line, index) => new EntryDetail
            {
                EntryRowCode = (short)(index + 1),
                AccountId = line.AccountId,
                Debit = line.Debit,
                Credit = line.Credit,
                EntryDetailsRemarks = line.Description,
                CreatedAt = DateTime.Now
            }).ToList()
        };

        await context.EntryMasters.AddAsync(entry);
    }

    public static async Task RemoveAsync(ApplicationDbContext context, JournalSourceType sourceType, int sourceId)
    {
        if (sourceType == JournalSourceType.Invoice)
        {
            return;
        }

        var tag = SourceTag(sourceType, sourceId);
        var entries = await context.EntryMasters
            .Include(a => a.EntryDetails)
            .Where(a => a.CollectEntryCode == tag)
            .ToListAsync();

        context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        context.EntryMasters.RemoveRange(entries);
    }

    private static string SourceTag(JournalSourceType sourceType, int? sourceId)
    {
        return $"{sourceType}:{sourceId}";
    }
}
