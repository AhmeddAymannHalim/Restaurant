using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Commons;

public class JournalEntryRepository : IJournalEntryRepository
{
    private static readonly Dictionary<JournalSourceType, (EntryType Type, string ArName, string EnName)> EntryTypes = new()
    {
        [JournalSourceType.CustomerPaymentVoucher] = (EntryType.CustomerPayment, "دفعات عملاء", "Customer payment"),
        [JournalSourceType.SupplierPaymentVoucher] = (EntryType.SupplierPayment, "دفعات موردين", "Supplier payment"),
        [JournalSourceType.ExitExpense] = (EntryType.Expenses, "مصروفات", "Expenses"),
        [JournalSourceType.Deposit] = (EntryType.CashReceipt, "قبض نقدي", "Cash receipt"),
        [JournalSourceType.Salary] = (EntryType.AdvancesAndEmployeeSalaries, "مرتبات", "Salaries"),
        [JournalSourceType.Scrounge] = (EntryType.AdvancesAndEmployeeSalaries, "سلف وحوافز", "Advances and incentives"),
        [JournalSourceType.CustomerOpeningBalance] = (EntryType.CustomerOpeningBalance, "رصيد افتتاحى للعملاء", "Opening Balance of Customers"),
        [JournalSourceType.SupplierOpeningBalance] = (EntryType.SupplierOpeningBalance, "رصيد افتتاحى للموردين", "Opening Balance for Suppliers"),
        [JournalSourceType.CustomerRefund] = (EntryType.CashPayment, "سند صرف نقدي", "Cash Pay Entry"),
        [JournalSourceType.SupplierRefund] = (EntryType.CashReceipt, "سند قبض نقدي", "Cash Receipt Entry"),
        [JournalSourceType.Manual] = (EntryType.Journal, "قيد يومي", "Journal")
    };

    private readonly ApplicationDbContext _context;

    public JournalEntryRepository(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task EnsureDefaultAccountsAsync()
    {
        var existing = await _context.Accounts.Select(a => a.Code).ToListAsync();

        foreach (var definition in AccountCodes.Default)
        {
            if (existing.Contains(definition.Code))
            {
                continue;
            }

            await _context.Accounts.AddAsync(new Account
            {
                Code = definition.Code,
                ArName = definition.NameAr,
                EnName = definition.NameEn,
                AccountType = definition.AccountType,
                BalanceType = definition.BalanceType,
                AccDate = DateTime.Today,
                AccCheckDate = DateTime.Today
            });
        }

        await _context.SaveChangesAsync();

        var accounts = await _context.Accounts.ToListAsync();
        foreach (var definition in AccountCodes.Default.Where(a => a.ParentCode is not null))
        {
            var account = accounts.FirstOrDefault(a => a.Code == definition.Code);
            var parent = accounts.FirstOrDefault(a => a.Code == definition.ParentCode);
            if (account is not null && parent is not null && account.ParentAccountId != parent.Id)
            {
                account.ParentAccountId = parent.Id;
            }
        }

        await _context.SaveChangesAsync();
    }

    // like the ERP opening-balance voucher: the partner account against the voucher's main account (capital)
    public async Task<ResultMessage> PostOpeningBalanceAsync(Account account, decimal delta)
    {
        if (delta == 0)
        {
            return new ResultMessage { Success = true };
        }

        var isCustomer = account.AccountType == AccountType.Customer;
        var sourceType = isCustomer ? JournalSourceType.CustomerOpeningBalance : JournalSourceType.SupplierOpeningBalance;
        var entryType = isCustomer ? EntryType.CustomerOpeningBalance : EntryType.SupplierOpeningBalance;
        var mainAccountId = await _context.EntrySettings.AsNoTracking()
            .Where(a => a.EntryType == entryType)
            .Select(a => a.AccountId)
            .FirstOrDefaultAsync();
        var amount = Math.Abs(delta);
        var accountIsDebit = (delta >= 0) == (account.BalanceType == AccountBalanceType.Debit);
        var mainLine = mainAccountId.HasValue
            ? new JournalLineRequest(null, accountIsDebit ? 0 : amount, accountIsDebit ? amount : 0, null, mainAccountId)
            : new JournalLineRequest(AccountCodes.Equity, accountIsDebit ? 0 : amount, accountIsDebit ? amount : 0);

        return await PostSourceEntryAsync(sourceType, account.Id, DateTime.Today, "Opening balance " + account.Code,
            new[]
            {
                new JournalLineRequest(null, accountIsDebit ? amount : 0, accountIsDebit ? 0 : amount, null, account.Id),
                mainLine
            });
    }

    public async Task<ResultMessage> PostSourceEntryAsync(JournalSourceType sourceType,
        int sourceId,
        DateTime date,
        string description,
        IEnumerable<JournalLineRequest> lines,
        bool saveChanges = true)
    {
        var requested = (lines ?? Enumerable.Empty<JournalLineRequest>())
            .Where(a => a.Debit > 0 || a.Credit > 0)
            .ToList();
        if (requested.Count == 0)
        {
            return new ResultMessage { Success = true };
        }

        if (requested.Any(a => (a.Debit > 0) == (a.Credit > 0) || a.Debit < 0 || a.Credit < 0))
        {
            return Fail("JournalLineInvalid");
        }

        if (Math.Round(requested.Sum(a => a.Debit), 2) != Math.Round(requested.Sum(a => a.Credit), 2))
        {
            return Fail("JournalNotBalanced");
        }

        // Invoice entries are persisted by InvoiceRepository.AddEntryAsync (different shape: linked to BillId),
        // so for Invoice we only validate the lines here (balance check above) and intentionally return
        // without saving an EntryMaster — do not "fix" this into saving, it would duplicate the invoice's entry.
        if (sourceType == JournalSourceType.Invoice || !EntryTypes.TryGetValue(sourceType, out var type))
        {
            return new ResultMessage { Success = true };
        }

        var codes = requested.Where(a => a.AccountId is null).Select(a => a.AccountCode).Distinct().ToList();
        var accounts = await _context.Accounts
            .Where(a => codes.Contains(a.Code))
            .ToListAsync();

        if (accounts.Count != codes.Count)
        {
            await EnsureDefaultAccountsAsync();
            accounts = await _context.Accounts
                .Where(a => codes.Contains(a.Code))
                .ToListAsync();
        }

        if (accounts.Count != codes.Count)
        {
            return Fail("RecordNotFound");
        }

        var accountsByCode = accounts.ToDictionary(a => a.Code, a => a);
        var accountIds = accounts.ToDictionary(a => a.Code, a => a.Id);
        var setting = await BillSupport.GetEntrySettingAsync(_context, type.Type, type.ArName, type.EnName);
        var cashId = await _context.Accounts.AsNoTracking()
            .Where(a => a.Code == AccountCodes.Cash)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
        var number = await GetNextEntryNumberAsync();

        var entry = new EntryMaster
        {
            Code = number,
            EntryDate = date,
            TotalBalance = requested.Sum(a => Math.Round(a.Debit, 2)),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = description,
            CollectEntryCode = SourceTag(sourceType, sourceId),
            AccountBoxOrBankId = cashId.HasValue && requested.Any(a => a.AccountCode == AccountCodes.Cash) ? cashId : null,
            EntrySetting = setting,
            CreatedAt = DateTime.Now,
            EntryDetails = requested.Select((line, index) => new EntryDetail
            {
                EntryRowCode = (short)(index + 1),
                AccountId = line.AccountId ?? accountIds[line.AccountCode],
                Debit = Math.Round(line.Debit, 2),
                Credit = Math.Round(line.Credit, 2),
                EntryDetailsRemarks = line.Description,
                CreatedAt = DateTime.Now
            }).ToList()
        };

        await _context.EntryMasters.AddAsync(entry);

        // the partner (customer/supplier) side of each line is already adjusted directly by the
        // caller (e.g. SupplierPaymentRepository); only the system/GL accounts resolved here by
        // AccountCode need their stored balance kept in sync with this posting.
        foreach (var line in requested.Where(a => a.AccountId is null))
        {
            accountsByCode[line.AccountCode].Adjust(line.Debit - line.Credit);
        }

        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return new ResultMessage
        {
            Success = true,
            Data = number
        };
    }

    public async Task RemoveSourceEntriesAsync(JournalSourceType sourceType, int sourceId)
    {
        if (sourceType == JournalSourceType.Invoice)
        {
            return;
        }

        var tag = SourceTag(sourceType, sourceId);
        var entries = await _context.EntryMasters
            .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
            .Where(a => a.CollectEntryCode == tag)
            .ToListAsync();

        // reverse the GL-account balances this posting applied (see PostSourceEntryAsync);
        // partner accounts are not touched here, they are reversed by the caller directly.
        foreach (var detail in entries.SelectMany(a => a.EntryDetails))
        {
            if (detail.Account is not null && detail.Account.AccountType is not (AccountType.Customer or AccountType.Supplier))
            {
                detail.Account.Adjust(-(detail.Debit - detail.Credit));
            }
        }

        _context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        _context.EntryMasters.RemoveRange(entries);
    }

    private static string SourceTag(JournalSourceType sourceType, int? sourceId)
    {
        return $"{sourceType}:{sourceId}";
    }

    private static ResultMessage Fail(string message)
    {
        return new ResultMessage
        {
            Success = false,
            Message = message
        };
    }
}
