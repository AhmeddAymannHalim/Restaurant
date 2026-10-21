using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
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

    public async Task<Account> CreateAsync(string arName, string enName, AccountType type, decimal openingBalance = 0)
    {
        var setting = await _context.AccountSettings.AsNoTracking().OrderByDescending(a => a.Id).FirstOrDefaultAsync();

        int? parentId = null;
        if (type == AccountType.Customer)
        {
            parentId = setting?.ParentCustomerId;
        }
        else if (type == AccountType.Supplier)
        {
            parentId = setting?.ParentSupplierId;
        }
        else if (type == AccountType.Employee)
        {
            parentId = setting?.ParentEmployeeId;
        }

        var account = new Account
        {
            ArName = arName,
            EnName = enName,
            AccountType = type,
            BalanceType = type == AccountType.Customer ? AccountBalanceType.Debit : AccountBalanceType.Credit,
            AccDate = DateTime.Today,
            AccCheckDate = DateTime.Today
        };

        var parent = parentId.HasValue ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == parentId.Value) : null;
        if (parent == null)
        {
            // no parent in AccountSetting: the account is a main account with the next free number
            // only main accounts count, a child code like 30001 must not push the next main number
            var rootCodes = await _context.Accounts.AsNoTracking()
                                                   .Where(a => a.ParentAccountId == null)
                                                   .Select(a => a.Code)
                                                   .ToListAsync();

            var nextRoot = rootCodes.Where(code => int.TryParse(code, out _))
                                    .Select(int.Parse)
                                    .DefaultIfEmpty(0)
                                    .Max() + 1;

            while (await _context.Accounts.AsNoTracking()
                                          .AnyAsync(a => a.Code == nextRoot
                                          .ToString()))
            {
                nextRoot++;
            }

            account.Code = nextRoot.ToString();
            account.Level = 1;
        }
        else
        {
            // the code is the parent code followed by the next number of its children
            var childCodes = await _context.Accounts.AsNoTracking()
                .Where(a => a.ParentAccountId == parent.Id)
                .Select(a => a.Code)
                .ToListAsync();

            var lastNumber = 0;
            foreach (var code in childCodes)
            {
                if (code.Length > parent.Code.Length && code.StartsWith(parent.Code))
                {
                    int.TryParse(code.Substring(parent.Code.Length), out var number);
                    lastNumber = Math.Max(lastNumber, number);
                }
            }

            account.Code = parent.Code + (lastNumber + 1).ToString().PadLeft(CodeSuffixLength, '0');
            account.ParentAccountId = parent.Id;
            account.Level = (parent.Level ?? 1) + 1;
            parent.NumberOfSubAccounts = (parent.NumberOfSubAccounts ?? 0) + 1;
        }

        account.Adjust(openingBalance);
        await _context.Accounts.AddAsync(account);

        return account;
    }
}
