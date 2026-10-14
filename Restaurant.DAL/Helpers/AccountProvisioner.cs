using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Helpers;

public static class AccountProvisioner
{
    private const int CodeSuffixLength = 4;

    public static async Task<Account> CreateAsync(
        ApplicationDbContext context,
        string arName,
        string enName,
        AccountType type,
        string parentCode,
        decimal openingBalance = 0)
    {
        var parent = await context.Accounts.FirstOrDefaultAsync(a => a.Code == parentCode);
        if (parent is null)
        {
            await new JournalEntryRepository(context).EnsureDefaultAccountsAsync();
            parent = await context.Accounts.FirstAsync(a => a.Code == parentCode);
        }

        var account = new Account
        {
            Code = await NextCodeAsync(context, parent),
            ArName = arName,
            EnName = enName,
            AccountType = type,
            BalanceType = BalanceTypeOf(type),
            ParentAccountId = parent.Id,
            Level = (parent.Level ?? 1) + 1,
            AccDate = DateTime.Today,
            AccCheckDate = DateTime.Today
        };

        account.Adjust(openingBalance);

        parent.NumberOfSubAccounts = (parent.NumberOfSubAccounts ?? 0) + 1;

        await context.Accounts.AddAsync(account);
        return account;
    }

    public static AccountBalanceType BalanceTypeOf(AccountType type)
    {
        return type == AccountType.Customer || type == AccountType.Box || type == AccountType.Bank
            ? AccountBalanceType.Debit
            : AccountBalanceType.Credit;
    }

    private static async Task<string> NextCodeAsync(ApplicationDbContext context, Account parent)
    {
        var childCodes = await context.Accounts.AsNoTracking()
            .Where(a => a.ParentAccountId == parent.Id)
            .Select(a => a.Code)
            .ToListAsync();

        var last = childCodes
            .Where(code => code.Length > parent.Code.Length && code.StartsWith(parent.Code))
            .Select(code => int.TryParse(code.Substring(parent.Code.Length), out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();

        return parent.Code + (last + 1).ToString().PadLeft(CodeSuffixLength, '0');
    }
}
