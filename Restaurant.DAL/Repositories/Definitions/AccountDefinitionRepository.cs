using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;
public class AccountDefinitionRepository : IAccountDefinitionRepository
{
    private const int CodeSuffixLength = 4;

    private static readonly AccountType[] OwnedTypes =
    {
        AccountType.Customer,
        AccountType.Supplier,
        AccountType.CustomerAndSupplier,
        AccountType.Employee,
        AccountType.BoxAndBank
    };

    private static readonly string[] SystemCodes = AccountCodes.Default.Select(a => a.Code).ToArray();

    private readonly ApplicationDbContext _context;

    public AccountDefinitionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly)
    {
        var query = _context.Accounts.AsNoTracking().AsQueryable();

        if (assetsOnly)
        {
            query = query.Where(a => a.AccountDetails != null);
        }
        else if (Enum.TryParse<AccountKind>(request.Type, out var kind))
        {
            query = kind switch
            {
                AccountKind.Bank => query.Where(a => a.AccountType == AccountType.Bank),
                AccountKind.Safe => query.Where(a => a.AccountType == AccountType.Box),
                AccountKind.Asset => query.Where(a => a.AccountDetails != null),
                AccountKind.Expense => query.Where(a => a.AccountDetails == null
                    && !OwnedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && a.Code.StartsWith(AccountCodes.Expenses)),
                AccountKind.Owned => query.Where(a => OwnedTypes.Contains(a.AccountType)),
                _ => query.Where(a => a.AccountDetails == null
                    && !OwnedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && !a.Code.StartsWith(AccountCodes.Expenses))
            };
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Code.ToLower().Contains(search)
                || a.ArName.ToLower().Contains(search)
                || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Code)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(a => new AccountDefinitionViewModel
            {
                Id = a.Id,
                Code = a.Code,
                ArName = a.ArName,
                EnName = a.EnName,
                AccountType = a.AccountType,
                ParentAccountId = a.ParentAccountId,
                ParentArName = a.ParentAccount.ArName,
                ParentEnName = a.ParentAccount.EnName,
                Level = a.Level,
                BalanceType = a.BalanceType,
                Balance = a.BalanceType == AccountBalanceType.Debit ? a.DebitBalance - a.CreditBalance : a.CreditBalance - a.DebitBalance,
                BuyValue = a.AccountDetails.BuyValue,
                BuyDate = a.AccountDetails.BuyDate,
                Location = a.AccountDetails.Location,
                DepreciationType = a.AccountDetails == null ? DepreciationType.FixedInstallment : a.AccountDetails.DepreciationType,
                ProductAge = a.AccountDetails.ProductAge,
                Kind = a.AccountDetails != null ? AccountKind.Asset
                    : OwnedTypes.Contains(a.AccountType) ? AccountKind.Owned
                    : a.AccountType == AccountType.Bank ? AccountKind.Bank
                    : a.AccountType == AccountType.Box ? AccountKind.Safe
                    : a.Code.StartsWith(AccountCodes.Expenses) ? AccountKind.Expense
                    : AccountKind.GeneralLedger,
                IsOwned = OwnedTypes.Contains(a.AccountType),
                IsSystem = SystemCodes.Contains(a.Code)
            })
            .ToListAsync();

        return PaginatedResult<AccountDefinitionViewModel>.Create(data, total, request);
    }

    public async Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id)
    {
        return await _context.Accounts.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AccountDefinitionViewModel
            {
                Id = a.Id,
                Code = a.Code,
                ArName = a.ArName,
                EnName = a.EnName,
                AccountType = a.AccountType,
                ParentAccountId = a.ParentAccountId,
                CurrencyId = a.CurrencyId,
                Remarks = a.Remarks,
                MaxDebitAllowed = a.MaxDebitAllowed,
                RequiresCostCenter = a.RequiresCostCenter,
                Level = a.Level,
                BalanceType = a.BalanceType,
                Balance = a.BalanceType == AccountBalanceType.Debit ? a.DebitBalance - a.CreditBalance : a.CreditBalance - a.DebitBalance,
                Location = a.AccountDetails.Location,
                BuyValue = a.AccountDetails.BuyValue,
                BuyDate = a.AccountDetails.BuyDate,
                StartUsingDate = a.AccountDetails.StartUsingDate,
                ProductAge = a.AccountDetails.ProductAge,
                DepreciationType = a.AccountDetails == null ? DepreciationType.FixedInstallment : a.AccountDetails.DepreciationType,
                ScrapValue = a.AccountDetails.ScrapValue,
                DepreciationAccountId = a.AccountDetails.DepreciationAccountId,
                DepreciationExpensesAccountId = a.AccountDetails.DepreciationExpensesAccountId,
                Kind = a.AccountDetails != null ? AccountKind.Asset
                    : OwnedTypes.Contains(a.AccountType) ? AccountKind.Owned
                    : a.AccountType == AccountType.Bank ? AccountKind.Bank
                    : a.AccountType == AccountType.Box ? AccountKind.Safe
                    : a.Code.StartsWith(AccountCodes.Expenses) ? AccountKind.Expense
                    : AccountKind.GeneralLedger,
                IsOwned = OwnedTypes.Contains(a.AccountType),
                IsSystem = SystemCodes.Contains(a.Code)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind)
    {
        var parent = await ResolveParentAsync(parentId, kind);

        return await NextCodeAsync(parent);
    }

    public async Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? exceptId)
    {
        var accounts = await _context.Accounts.AsNoTracking()
            .Where(a => !OwnedTypes.Contains(a.AccountType)
                && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                && a.AccountDetails == null
                && a.Id != exceptId)
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();

        return accounts;
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        return await _context.Accounts.AsNoTracking()
            .Where(a => !OwnedTypes.Contains(a.AccountType) && a.AccountType != AccountType.Collective)
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();
    }

    public async Task<ResultMessage> UpsertAccountAsync(Account account, AccountDetails details, AccountKind kind, decimal openingBalance)
    {
        try
        {
            if (account.Id == 0)
            {
                var parent = await ResolveParentAsync(account.ParentAccountId, kind);
                account.ParentAccountId = parent.Id;
                account.Code = await NextCodeAsync(parent);
                account.AccountType = AccountTypeOf(kind);
                account.BalanceType = kind == AccountKind.GeneralLedger ? parent.BalanceType : AccountBalanceType.Debit;
                account.Level = (parent.Level ?? 1) + 1;
                account.AccDate = DateTime.Today;
                account.AccCheckDate = DateTime.Today;
                account.CreatedAt = DateTime.Now;
                account.Adjust(openingBalance);
                parent.NumberOfSubAccounts = (parent.NumberOfSubAccounts ?? 0) + 1;

                if (kind == AccountKind.Asset)
                {
                    account.AccountDetails = await BuildDetailsAsync(details, parent, null);
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
                return Fail("RecordNotFound");
            }

            if (OwnedTypes.Contains(existing.AccountType))
            {
                return Fail("AccountOwnedByAnotherScreen");
            }

            if (account.ParentAccountId == existing.Id)
            {
                return Fail("InvalidParent");
            }

            if (account.ParentAccountId != existing.ParentAccountId)
            {
                var newParent = await ResolveParentAsync(account.ParentAccountId, kind);
                if (await IsDescendantAsync(newParent.Id, existing.Id))
                {
                    return Fail("InvalidParent");
                }

                var oldParent = existing.ParentAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == existing.ParentAccountId.Value)
                    : null;
                if (oldParent is not null)
                {
                    oldParent.NumberOfSubAccounts = Math.Max(0, (oldParent.NumberOfSubAccounts ?? 1) - 1);
                }

                newParent.NumberOfSubAccounts = (newParent.NumberOfSubAccounts ?? 0) + 1;
                existing.ParentAccountId = newParent.Id;
                existing.Level = (newParent.Level ?? 1) + 1;
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
                var parentForDetails = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == existing.ParentAccountId);
                var updated = await BuildDetailsAsync(details, parentForDetails, existing.AccountDetails);
                existing.AccountDetails = updated;
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Accounts_Code"))
        {
            return Fail("ConcurrentUpdate");
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    public async Task<ResultMessage> DeleteAccountAsync(int id)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.AccountDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (account is null)
            {
                return new ResultMessage { Success = true };
            }

            if (SystemCodes.Contains(account.Code))
            {
                return Fail("SystemAccountCannotBeDeleted");
            }

            if (OwnedTypes.Contains(account.AccountType))
            {
                return Fail("AccountOwnedByAnotherScreen");
            }

            if (await _context.Accounts.AnyAsync(a => a.ParentAccountId == id)
                || await _context.EntryDetails.AnyAsync(a => a.AccountId == id)
                || await _context.EntryMasters.AnyAsync(a => a.AccountBoxOrBankId == id)
                || await _context.BillMasters.AnyAsync(a => a.CustomerOrSupplierAccountId == id || a.AccountPayId == id)
                || await _context.AccountDetails.AnyAsync(a => a.DepreciationAccountId == id || a.DepreciationExpensesAccountId == id))
            {
                return Fail("CannotDeleteLinkedRecord");
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
            return Fail("CannotDeleteLinkedRecord");
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    private async Task<Account> ResolveParentAsync(int? parentId, AccountKind kind)
    {
        if (parentId.HasValue)
        {
            var chosen = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == parentId.Value);
            if (chosen is not null)
            {
                return chosen;
            }
        }

        var rootCode = kind switch
        {
            AccountKind.Asset => AccountCodes.Assets,
            AccountKind.Expense => AccountCodes.Expenses,
            AccountKind.Bank or AccountKind.Safe => AccountCodes.Assets,
            _ => AccountCodes.Assets
        };

        var root = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == rootCode);
        if (root is null)
        {
            await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();
            root = await _context.Accounts.FirstAsync(a => a.Code == rootCode);
        }

        return root;
    }

    private async Task<string> NextCodeAsync(Account parent)
    {
        var childCodes = await _context.Accounts.AsNoTracking()
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

    private async Task<bool> IsDescendantAsync(int candidateId, int ancestorId)
    {
        var current = (int?)candidateId;
        var guard = 0;
        while (current.HasValue && guard++ < 50)
        {
            if (current.Value == ancestorId)
            {
                return true;
            }

            current = await _context.Accounts.AsNoTracking()
                .Where(a => a.Id == current.Value)
                .Select(a => a.ParentAccountId)
                .FirstOrDefaultAsync();
        }

        return false;
    }

    private async Task<AccountDetails> BuildDetailsAsync(AccountDetails source, Account parent, AccountDetails target)
    {
        var details = target ?? new AccountDetails { CreatedAt = DateTime.Now };
        details.Location = source?.Location;
        details.BuyValue = source?.BuyValue;
        details.BuyDate = source?.BuyDate;
        details.StartUsingDate = source?.StartUsingDate;
        details.ProductAge = source?.ProductAge;
        details.DepreciationType = source?.DepreciationType ?? DepreciationType.FixedInstallment;
        details.ScrapValue = source?.ScrapValue;
        details.DepreciationAccountId = source?.DepreciationAccountId;
        details.DepreciationExpensesAccountId = source?.DepreciationExpensesAccountId;
        details.ParentAccountId = parent?.Id;
        details.MainParentAccountId = await RootOfAsync(parent);
        details.UpdatedAt = target is null ? null : DateTime.Now;

        return details;
    }

    private async Task<int?> RootOfAsync(Account account)
    {
        var current = account;
        var guard = 0;
        while (current?.ParentAccountId != null && guard++ < 50)
        {
            current = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == current.ParentAccountId.Value);
        }

        return current?.Id;
    }

    private static AccountType AccountTypeOf(AccountKind kind)
    {
        return kind switch
        {
            AccountKind.Bank => AccountType.Bank,
            AccountKind.Safe => AccountType.Box,
            _ => AccountType.Normal
        };
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
