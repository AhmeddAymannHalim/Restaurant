using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Repositories.Definitions;

public class AccountProvisionRepository : IAccountProvisionRepository
{
    private const int CodeSuffixLength = 4;

    private readonly ApplicationDbContext _context;

    public AccountProvisionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account> CreateAsync(string arName, string enName, AccountType type, string parentCode, decimal openingBalance = 0)
    {
        var parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == parentCode);
        if (parent == null)
        {
            await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();
            parent = await _context.Accounts.FirstAsync(a => a.Code == parentCode);
        }

        var parentLevel = parent.Level ?? 1;
        var subAccounts = parent.NumberOfSubAccounts ?? 0;

        var account = new Account
        {
            Code = await NextCodeAsync(parent),
            ArName = arName,
            EnName = enName,
            AccountType = type,
            BalanceType = GetBalanceType(type),
            ParentAccountId = parent.Id,
            Level = parentLevel + 1,
            AccDate = DateTime.Today,
            AccCheckDate = DateTime.Today
        };

        account.Adjust(openingBalance);

        parent.NumberOfSubAccounts = subAccounts + 1;

        await _context.Accounts.AddAsync(account);
        return account;
    }

    private AccountBalanceType GetBalanceType(AccountType type)
    {
        if (type == AccountType.Customer || type == AccountType.Box || type == AccountType.Bank)
        {
            return AccountBalanceType.Debit;
        }

        return AccountBalanceType.Credit;
    }

    private async Task<string> NextCodeAsync(Account parent)
    {
        var childCodes = await _context.Accounts.AsNoTracking()
            .Where(a => a.ParentAccountId == parent.Id)
            .Select(a => a.Code)
            .ToListAsync();

        var highest = 0;
        foreach (var code in childCodes)
        {
            var isChildCode = code.Length > parent.Code.Length && code.StartsWith(parent.Code);
            if (!isChildCode)
            {
                continue;
            }

            int.TryParse(code.Substring(parent.Code.Length), out var number);
            if (number > highest)
            {
                highest = number;
            }
        }

        return parent.Code + (highest + 1).ToString().PadLeft(CodeSuffixLength, '0');
    }
}
