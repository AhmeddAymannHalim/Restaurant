using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Commons;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IBillSupportRepository _billSupport;

    public JournalEntryRepository(ApplicationDbContext context)
    {
        _context = context;
        _billSupport = new BillSupportRepository(context);
    }

    public async Task<string> GetNextEntryNumberAsync()
    {
        var recent = await _context.EntryMasters.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.Code)
            .Take(50)
            .ToListAsync();

        var last = recent.Where(a => int.TryParse(a, out _)).Select(int.Parse).DefaultIfEmpty(0).Max();

        return (last + 1).ToString();
    }

    // the opening balance of a customer / supplier: its account against the main account of the voucher
    public async Task<ResultMessage> PostOpeningBalanceAsync(Account account, decimal delta)
    {
        if (delta == 0)
        {
            return new ResultMessage { Success = true };
        }

        var isCustomer = account.AccountType == AccountType.Customer;
        var sourceType = isCustomer ? JournalSourceType.CustomerOpeningBalance : JournalSourceType.SupplierOpeningBalance;

        var amount = Math.Abs(delta);
        var accountIsDebit = (delta >= 0) == (account.BalanceType == AccountBalanceType.Debit);

        return await PostSourceEntryAsync(sourceType, account.Id, DateTime.Today, "Opening balance " + account.Code,
            new[]
            {
                new JournalLineRequest(account.Id, accountIsDebit ? amount : 0, accountIsDebit ? 0 : amount),
                new JournalLineRequest(null, accountIsDebit ? 0 : amount, accountIsDebit ? amount : 0)
            });
    }

    // a line without an account uses the main account of the voucher (EntrySetting.Account).
    // No main account in the settings means nothing to post, so no entry is created.
    public async Task<ResultMessage> PostSourceEntryAsync(JournalSourceType sourceType,
        int sourceId,
        DateTime date,
        string description,
        IEnumerable<JournalLineRequest> lines,
        bool saveChanges = true)
    {
        var all = (lines ?? Enumerable.Empty<JournalLineRequest>()).ToList();

        // a negative amount must fail, not be dropped with the empty lines
        if (all.Any(a => a.Debit < 0 || a.Credit < 0))
        {
            return new ResultMessage { Success = false, Message = "JournalLineInvalid" };
        }

        var requested = all.Where(a => a.Debit > 0 || a.Credit > 0).ToList();
        if (requested.Count == 0)
        {
            return new ResultMessage { Success = true };
        }

        if (requested.Any(a => (a.Debit > 0) == (a.Credit > 0)))
        {
            return new ResultMessage { Success = false, Message = "JournalLineInvalid" };
        }

        if (Math.Round(requested.Sum(a => a.Debit), 2) != Math.Round(requested.Sum(a => a.Credit), 2))
        {
            return new ResultMessage { Success = false, Message = "JournalNotBalanced" };
        }

        // which entry type (and name) belongs to this kind of voucher
        EntryType entryType;
        string arName;
        string enName;
        switch (sourceType)
        {
            case JournalSourceType.CustomerPaymentVoucher:
                entryType = EntryType.CustomerPayment;
                arName = "دفعات عملاء";
                enName = "Customer payment";
                break;
            case JournalSourceType.SupplierPaymentVoucher:
                entryType = EntryType.SupplierPayment;
                arName = "دفعات موردين";
                enName = "Supplier payment";
                break;
            case JournalSourceType.CustomerOpeningBalance:
                entryType = EntryType.CustomerOpeningBalance;
                arName = "رصيد افتتاحى للعملاء";
                enName = "Opening Balance of Customers";
                break;
            case JournalSourceType.SupplierOpeningBalance:
                entryType = EntryType.SupplierOpeningBalance;
                arName = "رصيد افتتاحى للموردين";
                enName = "Opening Balance for Suppliers";
                break;
            case JournalSourceType.CustomerRefund:
                entryType = EntryType.CashPayment;
                arName = "سند صرف نقدي";
                enName = "Cash Pay Entry";
                break;
            case JournalSourceType.SupplierRefund:
                entryType = EntryType.CashReceipt;
                arName = "سند قبض نقدي";
                enName = "Cash Receipt Entry";
                break;
            case JournalSourceType.Manual:
                entryType = EntryType.Journal;
                arName = "قيد يومي";
                enName = "Journal";
                break;
            default:
                return new ResultMessage { Success = true };
        }

        var setting = await _billSupport.GetEntrySettingAsync(entryType, arName, enName);

        if (requested.Any(a => a.AccountId is null) && setting.AccountId is null)
        {
            return new ResultMessage { Success = true };
        }

        var accountIds = requested.Select(a => a.AccountId ?? setting.AccountId.Value).Distinct().ToList();
        var accounts = await _context.Accounts.Where(a => accountIds.Contains(a.Id)).ToListAsync();
        if (accounts.Count != accountIds.Count)
        {
            return new ResultMessage { Success = false, Message = "RecordNotFound" };
        }

        var number = await GetNextEntryNumberAsync();

        var entry = new EntryMaster
        {
            Code = number,
            EntryDate = date,
            TotalBalance = requested.Sum(a => Math.Round(a.Debit, 2)),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = description,
            CollectEntryCode = $"{sourceType}:{sourceId}",
            AccountBoxOrBankId = requested.Any(a => a.AccountId is null) ? setting.AccountId : null,
            EntrySetting = setting,
            CreatedAt = DateTime.Now,
            EntryDetails = requested.Select((line, index) => new EntryDetail
            {
                EntryRowCode = (short)(index + 1),
                AccountId = line.AccountId ?? setting.AccountId,
                Debit = Math.Round(line.Debit, 2),
                Credit = Math.Round(line.Credit, 2),
                EntryDetailsRemarks = line.Description,
                CreatedAt = DateTime.Now
            }).ToList()
        };

        await _context.EntryMasters.AddAsync(entry);

        // the balance of a customer / supplier account is changed by the caller, every other account here
        foreach (var line in requested)
        {
            var account = accounts.First(a => a.Id == (line.AccountId ?? setting.AccountId.Value));
            if (account.AccountType is not (AccountType.Customer or AccountType.Supplier))
            {
                account.Adjust(account.BalanceType == AccountBalanceType.Debit ? line.Debit - line.Credit : line.Credit - line.Debit);
            }
        }

        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return new ResultMessage { Success = true, Data = number };
    }

    public async Task<bool> PostBillEntryAsync(BillMaster bill, InvoiceType type, decimal paid, decimal remaining, int? partnerAccountId, EntrySetting entrySetting)
    {
        var setting = await _context.BillSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == bill.BillSettingId);
        var total = bill.TotalBillAmountAfterDiscountAndExtra;
        if (setting?.AccountItemId == null || total <= 0)
        {
            return false;
        }

        if (paid > 0 && setting.AccountPayId == null)
        {
            return false;
        }

        if (remaining > 0 && partnerAccountId == null)
        {
            return false;
        }

        var itemIsDebit = type is InvoiceType.Purchases or InvoiceType.SalesReturn;
        var itemIsCredit = type is InvoiceType.Sales or InvoiceType.Returns;
        if (!itemIsDebit && !itemIsCredit)
        {
            return false;
        }

        var lines = new List<EntryDetail>();
        lines.Add(new EntryDetail
        {
            AccountId = setting.AccountItemId,
            Debit = itemIsDebit ? Math.Round(total, 2) : 0,
            Credit = itemIsCredit ? Math.Round(total, 2) : 0
        });
        if (paid > 0)
        {
            lines.Add(new EntryDetail
            {
                AccountId = setting.AccountPayId,
                Debit = itemIsCredit ? Math.Round(paid, 2) : 0,
                Credit = itemIsDebit ? Math.Round(paid, 2) : 0
            });
        }
        if (remaining > 0)
        {
            lines.Add(new EntryDetail
            {
                AccountId = partnerAccountId,
                Debit = itemIsCredit ? Math.Round(remaining, 2) : 0,
                Credit = itemIsDebit ? Math.Round(remaining, 2) : 0
            });
        }

        for (var i = 0; i < lines.Count; i++)
        {
            lines[i].EntryRowCode = (short)(i + 1);
            lines[i].CreatedAt = DateTime.Now;
        }

        await _context.EntryMasters.AddAsync(new EntryMaster
        {
            Code = bill.Code,
            EntryDate = bill.Date,
            TotalBalance = lines.Sum(a => a.Debit),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = $"{entrySetting.EnName} {bill.Code}",
            AccountBoxOrBankId = paid > 0 ? setting.AccountPayId : null,
            Bill = bill,
            EntrySetting = entrySetting,
            CreatedAt = DateTime.Now,
            EntryDetails = lines
        });

        var accountIds = lines.Where(a => a.AccountId != partnerAccountId).Select(a => a.AccountId).ToList();
        var accounts = await _context.Accounts.Where(a => accountIds.Contains(a.Id)).ToListAsync();
        foreach (var line in lines.Where(a => a.AccountId != partnerAccountId))
        {
            var account = accounts.First(a => a.Id == line.AccountId);
            account.Adjust(account.BalanceType == AccountBalanceType.Debit ? line.Debit - line.Credit : line.Credit - line.Debit);
        }

        return true;
    }

    public async Task RemoveSourceEntriesAsync(JournalSourceType sourceType, int sourceId)
    {
        var tag = $"{sourceType}:{sourceId}";
        var entries = await _context.EntryMasters
            .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
            .Where(a => a.CollectEntryCode == tag)
            .ToListAsync();

        // give back what the posting did to the balances (customers and suppliers are reversed by the caller)
        foreach (var detail in entries.SelectMany(a => a.EntryDetails))
        {
            if (detail.Account is not null && detail.Account.AccountType is not (AccountType.Customer or AccountType.Supplier))
            {
                detail.Account.Adjust(detail.Account.BalanceType == AccountBalanceType.Debit ? detail.Credit - detail.Debit : detail.Debit - detail.Credit);
            }
        }

        _context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        _context.EntryMasters.RemoveRange(entries);
    }
}
