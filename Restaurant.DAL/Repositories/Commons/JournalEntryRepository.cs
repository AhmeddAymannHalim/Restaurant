using Restaurant.DAL.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Commons;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ApplicationDbContext _context;

    public JournalEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextEntryNumberAsync()
    {
        var last = await _context.JournalEntries.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.EntryNumber)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
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

        // Parents are linked in a second pass so the roots already have identities.
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

    public async Task<ResultMessage> PostEntryAsync(JournalEntry entry, bool saveChanges = true)
    {
        var lines = (entry.Lines ?? new List<JournalEntryLine>()).ToList();
        if (lines.Count == 0)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "JournalNotBalanced"
            };
        }

        if (lines.Any(a => (a.Debit > 0) == (a.Credit > 0) || a.Debit < 0 || a.Credit < 0))
        {
            return new ResultMessage
            {
                Success = false,
                Message = "JournalLineInvalid"
            };
        }

        if (Math.Round(lines.Sum(a => a.Debit), 2) != Math.Round(lines.Sum(a => a.Credit), 2))
        {
            return new ResultMessage
            {
                Success = false,
                Message = "JournalNotBalanced"
            };
        }

        if (string.IsNullOrWhiteSpace(entry.EntryNumber))
        {
            entry.EntryNumber = await GetNextEntryNumberAsync();
        }

        entry.Lines = lines;
        await _context.JournalEntries.AddAsync(entry);
        await ErpEntryMirror.AddAsync(_context, entry);

        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return new ResultMessage
        {
            Success = true,
            Data = entry.EntryNumber
        };
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

        var codes = requested.Select(a => a.AccountCode).Distinct().ToList();
        var accounts = await _context.Accounts
            .Where(a => codes.Contains(a.Code))
            .Select(a => new { a.Id, a.Code })
            .ToListAsync();

        if (accounts.Count != codes.Count)
        {
            await EnsureDefaultAccountsAsync();
            accounts = await _context.Accounts
                .Where(a => codes.Contains(a.Code))
                .Select(a => new { a.Id, a.Code })
                .ToListAsync();
        }

        if (accounts.Count != codes.Count)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        var accountIds = accounts.ToDictionary(a => a.Code, a => a.Id);

        var entry = new JournalEntry
        {
            Date = date,
            Description = description,
            SourceType = sourceType,
            SourceId = sourceId,
            Lines = requested.Select(a => new JournalEntryLine
            {
                AccountId = accountIds[a.AccountCode],
                Debit = Math.Round(a.Debit, 2),
                Credit = Math.Round(a.Credit, 2),
                Description = a.Description
            }).ToList()
        };

        return await PostEntryAsync(entry, saveChanges);
    }

    public async Task RemoveSourceEntriesAsync(JournalSourceType sourceType, int sourceId)
    {
        await ErpEntryMirror.RemoveAsync(_context, sourceType, sourceId);

        var entries = await _context.JournalEntries
            .Include(a => a.Lines)
            .Where(a => a.SourceType == sourceType && a.SourceId == sourceId)
            .ToListAsync();

        if (entries.Count == 0)
        {
            return;
        }

        _context.JournalEntryLines.RemoveRange(entries.SelectMany(a => a.Lines));
        _context.JournalEntries.RemoveRange(entries);
    }

    public async Task<IEnumerable<TrialBalanceLine>> GetTrialBalanceAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.JournalEntryLines.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(a => a.JournalEntry.Date >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.JournalEntry.Date < to.Value.Date.AddDays(1));
        }

        return await query
            .GroupBy(a => new { a.AccountId, a.Account.Code, a.Account.ArName, a.Account.EnName, a.Account.AccountType })
            .Select(a => new TrialBalanceLine
            {
                AccountId = a.Key.AccountId,
                Code = a.Key.Code,
                NameAr = a.Key.ArName,
                NameEn = a.Key.EnName,
                AccountType = a.Key.AccountType,
                TotalDebit = a.Sum(b => b.Debit),
                TotalCredit = a.Sum(b => b.Credit)
            })
            .OrderBy(a => a.Code)
            .ToListAsync();
    }
}
