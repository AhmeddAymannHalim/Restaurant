using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class AccountDefinitionRepository : IAccountDefinitionRepository
{
    private const int CodeSuffixLength = 4;

    private readonly ApplicationDbContext _context;

    public AccountDefinitionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Account>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly)
    {
        // these accounts belong to the customer, supplier, employee and bank screens
        var ownedTypes = new List<AccountType>
        {
            AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
        };

        // the expense accounts are the accounts under the expenses parent of AccountSetting
        var expensesCode = await _context.AccountSettings.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.ParentExpenses.Code)
            .FirstOrDefaultAsync();
        var hasExpenses = !string.IsNullOrEmpty(expensesCode);

        var query = _context.Accounts.AsNoTracking().Include(a => a.ParentAccount).Include(a => a.AccountDetails).AsQueryable();

        if (assetsOnly)
        {
            query = query.Where(a => a.AccountDetails != null);
        }
        else if (Enum.TryParse<AccountKind>(request.Type, out var kind))
        {
            if (kind == AccountKind.Bank)
            {
                query = query.Where(a => a.AccountType == AccountType.Bank);
            }
            else if (kind == AccountKind.Safe)
            {
                query = query.Where(a => a.AccountType == AccountType.Box);
            }
            else if (kind == AccountKind.Asset)
            {
                query = query.Where(a => a.AccountDetails != null);
            }
            else if (kind == AccountKind.Owned)
            {
                query = query.Where(a => ownedTypes.Contains(a.AccountType));
            }
            else if (kind == AccountKind.Expense)
            {
                query = query.Where(a => a.AccountDetails == null
                    && !ownedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && hasExpenses && a.Code.StartsWith(expensesCode));
            }
            else
            {
                query = query.Where(a => a.AccountDetails == null
                    && !ownedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && (!hasExpenses || !a.Code.StartsWith(expensesCode)));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Code.ToLower().Contains(search)
                || a.ArName.ToLower().Contains(search)
                || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var accounts = await query
            .OrderBy(a => a.Code)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        foreach (var account in accounts)
        {
            if (account.AccountDetails != null)
            {
                account.Kind = AccountKind.Asset;
            }
            else if (ownedTypes.Contains(account.AccountType))
            {
                account.Kind = AccountKind.Owned;
            }
            else if (account.AccountType == AccountType.Bank)
            {
                account.Kind = AccountKind.Bank;
            }
            else if (account.AccountType == AccountType.Box)
            {
                account.Kind = AccountKind.Safe;
            }
            else if (hasExpenses && account.Code.StartsWith(expensesCode))
            {
                account.Kind = AccountKind.Expense;
            }
            else
            {
                account.Kind = AccountKind.GeneralLedger;
            }
        }

        return PaginatedResult<Account>.Create(accounts, total, request);
    }

    public async Task<Account> GetAccountByIdAsync(int id)
    {
        var ownedTypes = new List<AccountType>
        {
            AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
        };

        var expensesCode = await _context.AccountSettings.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.ParentExpenses.Code)
            .FirstOrDefaultAsync();
        var hasExpenses = !string.IsNullOrEmpty(expensesCode);

        var account = await _context.Accounts.AsNoTracking()
            .Include(a => a.ParentAccount)
            .Include(a => a.AccountDetails)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (account is null)
        {
            return null;
        }

        if (account.AccountDetails != null)
        {
            account.Kind = AccountKind.Asset;
        }
        else if (ownedTypes.Contains(account.AccountType))
        {
            account.Kind = AccountKind.Owned;
        }
        else if (account.AccountType == AccountType.Bank)
        {
            account.Kind = AccountKind.Bank;
        }
        else if (account.AccountType == AccountType.Box)
        {
            account.Kind = AccountKind.Safe;
        }
        else if (hasExpenses && account.Code.StartsWith(expensesCode))
        {
            account.Kind = AccountKind.Expense;
        }
        else
        {
            account.Kind = AccountKind.GeneralLedger;
        }

        return account;
    }

    public async Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind)
    {
        // no parent chosen: the default parent of this kind in AccountSetting
        if (!parentId.HasValue)
        {
            var setting = await _context.AccountSettings.AsNoTracking().OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            if (kind == AccountKind.Asset)
            {
                parentId = setting?.ParentFixedAssetsId;
            }
            else if (kind == AccountKind.Expense)
            {
                parentId = setting?.ParentExpensesId;
            }
            else if (kind == AccountKind.Bank)
            {
                parentId = setting?.ParentBankId;
            }
            else if (kind == AccountKind.Safe)
            {
                parentId = setting?.ParentBoxId;
            }
        }

        var parent = parentId.HasValue ? await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == parentId.Value) : null;

        // a main account: the next free number
        if (parent is null)
        {
            var allCodes = await _context.Accounts.AsNoTracking().Select(a => a.Code).ToListAsync();
            var lastRootNumber = allCodes.Where(code => int.TryParse(code, out _)).Select(int.Parse).DefaultIfEmpty(0).Max();

            return (lastRootNumber + 1).ToString();
        }

        var childCodes = await _context.Accounts.AsNoTracking()
            .Where(a => a.ParentAccountId == parent.Id)
            .Select(a => a.Code)
            .ToListAsync();

        var lastNumber = childCodes
            .Where(code => code.Length > parent.Code.Length && code.StartsWith(parent.Code))
            .Select(code => int.TryParse(code.Substring(parent.Code.Length), out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();

        return parent.Code + (lastNumber + 1).ToString().PadLeft(CodeSuffixLength, '0');
    }

    public async Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? exceptId)
    {
        var ownedTypes = new List<AccountType>
        {
            AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
        };

        return await _context.Accounts.AsNoTracking()
            .Where(a => !ownedTypes.Contains(a.AccountType)
                && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                && a.AccountDetails == null
                && a.Id != exceptId)
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        var ownedTypes = new List<AccountType>
        {
            AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
        };

        return await _context.Accounts.AsNoTracking()
            .Where(a => !ownedTypes.Contains(a.AccountType) && a.AccountType != AccountType.Collective)
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();
    }

    public async Task<ResultMessage> UpsertAccountAsync(Account account, AccountDetails details, AccountKind kind, decimal openingBalance)
    {
        try
        {
            var ownedTypes = new List<AccountType>
            {
                AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
            };

            if (account.Id == 0)
            {
                // no parent chosen: the default parent of this kind in AccountSetting
                if (!account.ParentAccountId.HasValue)
                {
                    var setting = await _context.AccountSettings.AsNoTracking().OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                    if (kind == AccountKind.Asset)
                    {
                        account.ParentAccountId = setting?.ParentFixedAssetsId;
                    }
                    else if (kind == AccountKind.Expense)
                    {
                        account.ParentAccountId = setting?.ParentExpensesId;
                    }
                    else if (kind == AccountKind.Bank)
                    {
                        account.ParentAccountId = setting?.ParentBankId;
                    }
                    else if (kind == AccountKind.Safe)
                    {
                        account.ParentAccountId = setting?.ParentBoxId;
                    }
                }

                var parent = account.ParentAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.ParentAccountId.Value)
                    : null;

                if (parent is null)
                {
                    // a main account: the next free number
                    var allCodes = await _context.Accounts.AsNoTracking().Select(a => a.Code).ToListAsync();
                    var lastRootNumber = allCodes.Where(code => int.TryParse(code, out _)).Select(int.Parse).DefaultIfEmpty(0).Max();

                    account.ParentAccountId = null;
                    account.Code = (lastRootNumber + 1).ToString();
                    account.BalanceType = AccountBalanceType.Debit;
                    account.Level = 1;
                }
                else
                {
                    var childCodes = await _context.Accounts.AsNoTracking()
                        .Where(a => a.ParentAccountId == parent.Id)
                        .Select(a => a.Code)
                        .ToListAsync();

                    var lastNumber = childCodes
                        .Where(code => code.Length > parent.Code.Length && code.StartsWith(parent.Code))
                        .Select(code => int.TryParse(code.Substring(parent.Code.Length), out var number) ? number : 0)
                        .DefaultIfEmpty(0)
                        .Max();

                    account.ParentAccountId = parent.Id;
                    account.Code = parent.Code + (lastNumber + 1).ToString().PadLeft(CodeSuffixLength, '0');
                    account.BalanceType = kind == AccountKind.GeneralLedger ? parent.BalanceType : AccountBalanceType.Debit;
                    account.Level = (parent.Level ?? 1) + 1;
                    parent.NumberOfSubAccounts = (parent.NumberOfSubAccounts ?? 0) + 1;
                }

                if (kind == AccountKind.Bank)
                {
                    account.AccountType = AccountType.Bank;
                }
                else if (kind == AccountKind.Safe)
                {
                    account.AccountType = AccountType.Box;
                }
                else
                {
                    account.AccountType = AccountType.Normal;
                }

                account.AccDate = DateTime.Today;
                account.AccCheckDate = DateTime.Today;
                account.CreatedAt = DateTime.Now;
                account.Adjust(openingBalance);

                if (kind == AccountKind.Asset)
                {
                    account.AccountDetails = new AccountDetails
                    {
                        CreatedAt = DateTime.Now,
                        Location = details?.Location,
                        BuyValue = details?.BuyValue,
                        BuyDate = details?.BuyDate,
                        StartUsingDate = details?.StartUsingDate,
                        ProductAge = details?.ProductAge,
                        DepreciationType = details?.DepreciationType ?? DepreciationType.FixedInstallment,
                        ScrapValue = details?.ScrapValue,
                        DepreciationAccountId = details?.DepreciationAccountId,
                        DepreciationExpensesAccountId = details?.DepreciationExpensesAccountId,
                        ParentAccountId = parent?.Id
                    };

                    // the main parent is the top account above the parent
                    var top = parent;
                    var guard = 0;
                    while (top?.ParentAccountId != null && guard++ < 50)
                    {
                        top = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == top.ParentAccountId.Value);
                    }

                    account.AccountDetails.MainParentAccountId = top?.Id;
                }

                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return new ResultMessage { Success = true };
            }

            var existing = await _context.Accounts
                .Include(a => a.AccountDetails)
                .FirstOrDefaultAsync(a => a.Id == account.Id);
            if (existing is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (ownedTypes.Contains(existing.AccountType))
            {
                return new ResultMessage { Success = false, Message = "AccountOwnedByAnotherScreen" };
            }

            if (account.ParentAccountId == existing.Id)
            {
                return new ResultMessage { Success = false, Message = "InvalidParent" };
            }

            // moving the account under another parent
            if (account.ParentAccountId != existing.ParentAccountId)
            {
                var newParent = account.ParentAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.ParentAccountId.Value)
                    : null;

                // the new parent can not be this account or one of its children
                var current = newParent?.Id;
                var steps = 0;
                while (current.HasValue && steps++ < 50)
                {
                    if (current.Value == existing.Id)
                    {
                        return new ResultMessage { Success = false, Message = "InvalidParent" };
                    }

                    current = await _context.Accounts.AsNoTracking()
                        .Where(a => a.Id == current.Value)
                        .Select(a => a.ParentAccountId)
                        .FirstOrDefaultAsync();
                }

                var oldParent = existing.ParentAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == existing.ParentAccountId.Value)
                    : null;
                if (oldParent is not null)
                {
                    oldParent.NumberOfSubAccounts = Math.Max(0, (oldParent.NumberOfSubAccounts ?? 1) - 1);
                }

                if (newParent is not null)
                {
                    newParent.NumberOfSubAccounts = (newParent.NumberOfSubAccounts ?? 0) + 1;
                }

                existing.ParentAccountId = newParent?.Id;
                existing.Level = (newParent?.Level ?? 0) + 1;
            }

            existing.ArName = account.ArName;
            existing.EnName = account.EnName;
            existing.CurrencyId = account.CurrencyId;
            existing.Remarks = account.Remarks;
            existing.MaxDebitAllowed = account.MaxDebitAllowed;
            existing.RequiresCostCenter = account.RequiresCostCenter;
            existing.UpdatedAt = DateTime.Now;

            if (existing.AccountDetails is not null)
            {
                var detailsParent = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == existing.ParentAccountId);

                existing.AccountDetails.Location = details?.Location;
                existing.AccountDetails.BuyValue = details?.BuyValue;
                existing.AccountDetails.BuyDate = details?.BuyDate;
                existing.AccountDetails.StartUsingDate = details?.StartUsingDate;
                existing.AccountDetails.ProductAge = details?.ProductAge;
                existing.AccountDetails.DepreciationType = details?.DepreciationType ?? DepreciationType.FixedInstallment;
                existing.AccountDetails.ScrapValue = details?.ScrapValue;
                existing.AccountDetails.DepreciationAccountId = details?.DepreciationAccountId;
                existing.AccountDetails.DepreciationExpensesAccountId = details?.DepreciationExpensesAccountId;
                existing.AccountDetails.ParentAccountId = detailsParent?.Id;

                var top = detailsParent;
                var guard = 0;
                while (top?.ParentAccountId != null && guard++ < 50)
                {
                    top = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == top.ParentAccountId.Value);
                }

                existing.AccountDetails.MainParentAccountId = top?.Id;
                existing.AccountDetails.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Accounts_Code"))
        {
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> DeleteAccountAsync(int id)
    {
        try
        {
            var ownedTypes = new List<AccountType>
            {
                AccountType.Customer, AccountType.Supplier, AccountType.CustomerAndSupplier, AccountType.Employee, AccountType.BoxAndBank
            };

            var account = await _context.Accounts
                .Include(a => a.AccountDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (account is null)
            {
                return new ResultMessage { Success = true };
            }

            if (ownedTypes.Contains(account.AccountType))
            {
                return new ResultMessage { Success = false, Message = "AccountOwnedByAnotherScreen" };
            }

            if (await _context.Accounts.AnyAsync(a => a.ParentAccountId == id)
                || await _context.EntryDetails.AnyAsync(a => a.AccountId == id)
                || await _context.EntryMasters.AnyAsync(a => a.AccountBoxOrBankId == id)
                || await _context.BillMasters.AnyAsync(a => a.CustomerOrSupplierAccountId == id || a.AccountPayId == id)
                || await _context.AccountDetails.AnyAsync(a => a.DepreciationAccountId == id || a.DepreciationExpensesAccountId == id)
                || await _context.AccountSettings.AnyAsync(a => a.ParentCustomerId == id || a.ParentSupplierId == id
                    || a.ParentExpensesId == id || a.ParentBankId == id || a.ParentBoxId == id
                    || a.ParentFixedAssetsId == id || a.ParentEmployeeId == id)
                || await _context.BillSettings.AnyAsync(a => a.AccountItemId == id || a.AccountPayId == id
                    || a.AccountDiscountId == id || a.AccountExtraId == id)
                || await _context.EntrySettings.AnyAsync(a => a.AccountId == id))
            {
                return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
            }

            var parent = account.ParentAccountId.HasValue
                ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.ParentAccountId.Value)
                : null;
            if (parent is not null)
            {
                parent.NumberOfSubAccounts = Math.Max(0, (parent.NumberOfSubAccounts ?? 1) - 1);
            }

            if (account.AccountDetails is not null)
            {
                _context.AccountDetails.Remove(account.AccountDetails);
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }
}
