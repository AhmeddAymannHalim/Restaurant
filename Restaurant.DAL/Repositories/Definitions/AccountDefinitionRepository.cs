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

    private readonly ApplicationDbContext _context;
    private readonly IAccountCodeRepository _accountCodes;
    public AccountDefinitionRepository(ApplicationDbContext context)
    {
        _context = context;
        _accountCodes = new AccountCodeRepository();
    }
    private List<AccountType> GetOwnedTypes()
    {
        var types = new List<AccountType>();
        types.Add(AccountType.Customer);
        types.Add(AccountType.Supplier);
        types.Add(AccountType.CustomerAndSupplier);
        types.Add(AccountType.Employee);
        types.Add(AccountType.BoxAndBank);
        return types;
    }

    private bool IsOwnedType(AccountType type)
    {
        return GetOwnedTypes().Contains(type);
    }

    public async Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly)
    {
        var ownedTypes = GetOwnedTypes();
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
                    && !ownedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && a.Code.StartsWith(AccountCodeRepository.Expenses)),
                AccountKind.Owned => query.Where(a => ownedTypes.Contains(a.AccountType)),
                _ => query.Where(a => a.AccountDetails == null
                    && !ownedTypes.Contains(a.AccountType)
                    && a.AccountType != AccountType.Bank && a.AccountType != AccountType.Box
                    && !a.Code.StartsWith(AccountCodeRepository.Expenses))
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
        var rawRows = await query
            .OrderBy(a => a.Code)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.ArName,
                a.EnName,
                a.AccountType,
                a.ParentAccountId,
                ParentArName = a.ParentAccount.ArName,
                ParentEnName = a.ParentAccount.EnName,
                a.Level,
                a.BalanceType,
                a.DebitBalance,
                a.CreditBalance,
                HasAccountDetails = a.AccountDetails != null,
                BuyValue = a.AccountDetails.BuyValue,
                BuyDate = a.AccountDetails.BuyDate,
                Location = a.AccountDetails.Location,
                AccountDetailsDepreciationType = a.AccountDetails == null ? (DepreciationType?)null : a.AccountDetails.DepreciationType,
                ProductAge = a.AccountDetails.ProductAge
            })
            .ToListAsync();

        var data = new List<AccountDefinitionViewModel>();
        foreach (var row in rawRows)
        {
            decimal balance;
            if (row.BalanceType == AccountBalanceType.Debit)
            {
                balance = row.DebitBalance - row.CreditBalance;
            }
            else
            {
                balance = row.CreditBalance - row.DebitBalance;
            }

            DepreciationType depreciationType;
            if (row.AccountDetailsDepreciationType.HasValue)
            {
                depreciationType = row.AccountDetailsDepreciationType.Value;
            }
            else
            {
                depreciationType = DepreciationType.FixedInstallment;
            }

            var isOwned = IsOwnedType(row.AccountType);
            var isSystem = _accountCodes.IsSystemCode(row.Code);

            AccountKind kind;
            if (row.HasAccountDetails)
            {
                kind = AccountKind.Asset;
            }
            else if (isOwned)
            {
                kind = AccountKind.Owned;
            }
            else if (row.AccountType == AccountType.Bank)
            {
                kind = AccountKind.Bank;
            }
            else if (row.AccountType == AccountType.Box)
            {
                kind = AccountKind.Safe;
            }
            else if (row.Code.StartsWith(AccountCodeRepository.Expenses))
            {
                kind = AccountKind.Expense;
            }
            else
            {
                kind = AccountKind.GeneralLedger;
            }

            data.Add(new AccountDefinitionViewModel
            {
                Id = row.Id,
                Code = row.Code,
                ArName = row.ArName,
                EnName = row.EnName,
                AccountType = row.AccountType,
                ParentAccountId = row.ParentAccountId,
                ParentArName = row.ParentArName,
                ParentEnName = row.ParentEnName,
                Level = row.Level,
                BalanceType = row.BalanceType,
                Balance = balance,
                BuyValue = row.BuyValue,
                BuyDate = row.BuyDate,
                Location = row.Location,
                DepreciationType = depreciationType,
                ProductAge = row.ProductAge,
                Kind = kind,
                IsOwned = isOwned,
                IsSystem = isSystem
            });
        }

        return PaginatedResult<AccountDefinitionViewModel>.Create(data, total, request);
    }

    public async Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id)
    {
        var row = await _context.Accounts.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.ArName,
                a.EnName,
                a.AccountType,
                a.ParentAccountId,
                a.CurrencyId,
                a.Remarks,
                a.MaxDebitAllowed,
                a.RequiresCostCenter,
                a.Level,
                a.BalanceType,
                a.DebitBalance,
                a.CreditBalance,
                HasAccountDetails = a.AccountDetails != null,
                Location = a.AccountDetails.Location,
                BuyValue = a.AccountDetails.BuyValue,
                BuyDate = a.AccountDetails.BuyDate,
                StartUsingDate = a.AccountDetails.StartUsingDate,
                ProductAge = a.AccountDetails.ProductAge,
                AccountDetailsDepreciationType = a.AccountDetails == null ? (DepreciationType?)null : a.AccountDetails.DepreciationType,
                ScrapValue = a.AccountDetails.ScrapValue,
                DepreciationAccountId = a.AccountDetails.DepreciationAccountId,
                DepreciationExpensesAccountId = a.AccountDetails.DepreciationExpensesAccountId
            })
            .FirstOrDefaultAsync();

        if (row is null)
        {
            return null;
        }

        decimal balance;
        if (row.BalanceType == AccountBalanceType.Debit)
        {
            balance = row.DebitBalance - row.CreditBalance;
        }
        else
        {
            balance = row.CreditBalance - row.DebitBalance;
        }

        DepreciationType depreciationType;
        if (row.AccountDetailsDepreciationType.HasValue)
        {
            depreciationType = row.AccountDetailsDepreciationType.Value;
        }
        else
        {
            depreciationType = DepreciationType.FixedInstallment;
        }

        var isOwned = IsOwnedType(row.AccountType);
        var isSystem = _accountCodes.IsSystemCode(row.Code);

        AccountKind kind;
        if (row.HasAccountDetails)
        {
            kind = AccountKind.Asset;
        }
        else if (isOwned)
        {
            kind = AccountKind.Owned;
        }
        else if (row.AccountType == AccountType.Bank)
        {
            kind = AccountKind.Bank;
        }
        else if (row.AccountType == AccountType.Box)
        {
            kind = AccountKind.Safe;
        }
        else if (row.Code.StartsWith(AccountCodeRepository.Expenses))
        {
            kind = AccountKind.Expense;
        }
        else
        {
            kind = AccountKind.GeneralLedger;
        }

        return new AccountDefinitionViewModel
        {
            Id = row.Id,
            Code = row.Code,
            ArName = row.ArName,
            EnName = row.EnName,
            AccountType = row.AccountType,
            ParentAccountId = row.ParentAccountId,
            CurrencyId = row.CurrencyId,
            Remarks = row.Remarks,
            MaxDebitAllowed = row.MaxDebitAllowed,
            RequiresCostCenter = row.RequiresCostCenter,
            Level = row.Level,
            BalanceType = row.BalanceType,
            Balance = balance,
            Location = row.Location,
            BuyValue = row.BuyValue,
            BuyDate = row.BuyDate,
            StartUsingDate = row.StartUsingDate,
            ProductAge = row.ProductAge,
            DepreciationType = depreciationType,
            ScrapValue = row.ScrapValue,
            DepreciationAccountId = row.DepreciationAccountId,
            DepreciationExpensesAccountId = row.DepreciationExpensesAccountId,
            Kind = kind,
            IsOwned = isOwned,
            IsSystem = isSystem
        };
    }

    public async Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind)
    {
        Account parent = null;
        if (parentId.HasValue)
        {
            parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == parentId.Value);
        }

        if (parent is null)
        {
            var rootCode = kind switch
            {
                AccountKind.Asset => AccountCodeRepository.Assets,
                AccountKind.Expense => AccountCodeRepository.Expenses,
                AccountKind.Bank or AccountKind.Safe => AccountCodeRepository.Assets,
                _ => AccountCodeRepository.Assets
            };

            parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == rootCode);
            if (parent is null)
            {
                await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();
                parent = await _context.Accounts.FirstAsync(a => a.Code == rootCode);
            }
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
        var ownedTypes = GetOwnedTypes();
        var accounts = await _context.Accounts.AsNoTracking()
            .Where(a => !ownedTypes.Contains(a.AccountType)
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
        var ownedTypes = GetOwnedTypes();
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
            if (account.Id == 0)
            {
                Account parent = null;
                if (account.ParentAccountId.HasValue)
                {
                    parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.ParentAccountId.Value);
                }

                if (parent is null)
                {
                    var rootCode = kind switch
                    {
                        AccountKind.Asset => AccountCodeRepository.Assets,
                        AccountKind.Expense => AccountCodeRepository.Expenses,
                        AccountKind.Bank or AccountKind.Safe => AccountCodeRepository.Assets,
                        _ => AccountCodeRepository.Assets
                    };

                    parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == rootCode);
                    if (parent is null)
                    {
                        await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();
                        parent = await _context.Accounts.FirstAsync(a => a.Code == rootCode);
                    }
                }

                account.ParentAccountId = parent.Id;

                var childCodes = await _context.Accounts.AsNoTracking()
                    .Where(a => a.ParentAccountId == parent.Id)
                    .Select(a => a.Code)
                    .ToListAsync();

                var lastNumber = childCodes
                    .Where(code => code.Length > parent.Code.Length && code.StartsWith(parent.Code))
                    .Select(code => int.TryParse(code.Substring(parent.Code.Length), out var number) ? number : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                account.Code = parent.Code + (lastNumber + 1).ToString().PadLeft(CodeSuffixLength, '0');
                account.AccountType = kind switch
                {
                    AccountKind.Bank => AccountType.Bank,
                    AccountKind.Safe => AccountType.Box,
                    _ => AccountType.Normal
                };
                account.BalanceType = kind == AccountKind.GeneralLedger ? parent.BalanceType : AccountBalanceType.Debit;
                account.Level = (parent.Level ?? 1) + 1;
                account.AccDate = DateTime.Today;
                account.AccCheckDate = DateTime.Today;
                account.CreatedAt = DateTime.Now;
                account.Adjust(openingBalance);
                parent.NumberOfSubAccounts = (parent.NumberOfSubAccounts ?? 0) + 1;

                if (kind == AccountKind.Asset)
                {
                    var newDetails = new AccountDetails { CreatedAt = DateTime.Now };
                    newDetails.Location = details?.Location;
                    newDetails.BuyValue = details?.BuyValue;
                    newDetails.BuyDate = details?.BuyDate;
                    newDetails.StartUsingDate = details?.StartUsingDate;
                    newDetails.ProductAge = details?.ProductAge;
                    newDetails.DepreciationType = details?.DepreciationType ?? DepreciationType.FixedInstallment;
                    newDetails.ScrapValue = details?.ScrapValue;
                    newDetails.DepreciationAccountId = details?.DepreciationAccountId;
                    newDetails.DepreciationExpensesAccountId = details?.DepreciationExpensesAccountId;
                    newDetails.ParentAccountId = parent.Id;

                    var detailsRoot = parent;
                    var detailsRootGuard = 0;
                    while (detailsRoot?.ParentAccountId != null && detailsRootGuard++ < 50)
                    {
                        detailsRoot = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == detailsRoot.ParentAccountId.Value);
                    }

                    newDetails.MainParentAccountId = detailsRoot?.Id;
                    newDetails.UpdatedAt = null;

                    account.AccountDetails = newDetails;
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

            if (IsOwnedType(existing.AccountType))
            {
                return new ResultMessage { Success = false, Message = "AccountOwnedByAnotherScreen" };
            }

            if (account.ParentAccountId == existing.Id)
            {
                return new ResultMessage { Success = false, Message = "InvalidParent" };
            }

            if (account.ParentAccountId != existing.ParentAccountId)
            {
                Account newParent = null;
                if (account.ParentAccountId.HasValue)
                {
                    newParent = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.ParentAccountId.Value);
                }

                if (newParent is null)
                {
                    var rootCode = kind switch
                    {
                        AccountKind.Asset => AccountCodeRepository.Assets,
                        AccountKind.Expense => AccountCodeRepository.Expenses,
                        AccountKind.Bank or AccountKind.Safe => AccountCodeRepository.Assets,
                        _ => AccountCodeRepository.Assets
                    };

                    newParent = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == rootCode);
                    if (newParent is null)
                    {
                        await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();
                        newParent = await _context.Accounts.FirstAsync(a => a.Code == rootCode);
                    }
                }

                var isDescendant = false;
                var descendantCurrent = (int?)newParent.Id;
                var descendantGuard = 0;
                while (descendantCurrent.HasValue && descendantGuard++ < 50)
                {
                    if (descendantCurrent.Value == existing.Id)
                    {
                        isDescendant = true;
                        break;
                    }

                    descendantCurrent = await _context.Accounts.AsNoTracking()
                        .Where(a => a.Id == descendantCurrent.Value)
                        .Select(a => a.ParentAccountId)
                        .FirstOrDefaultAsync();
                }

                if (isDescendant)
                {
                    return new ResultMessage { Success = false, Message = "InvalidParent" };
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

                var updated = existing.AccountDetails;
                updated.Location = details?.Location;
                updated.BuyValue = details?.BuyValue;
                updated.BuyDate = details?.BuyDate;
                updated.StartUsingDate = details?.StartUsingDate;
                updated.ProductAge = details?.ProductAge;
                updated.DepreciationType = details?.DepreciationType ?? DepreciationType.FixedInstallment;
                updated.ScrapValue = details?.ScrapValue;
                updated.DepreciationAccountId = details?.DepreciationAccountId;
                updated.DepreciationExpensesAccountId = details?.DepreciationExpensesAccountId;
                updated.ParentAccountId = parentForDetails?.Id;

                var updatedRoot = parentForDetails;
                var updatedRootGuard = 0;
                while (updatedRoot?.ParentAccountId != null && updatedRootGuard++ < 50)
                {
                    updatedRoot = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == updatedRoot.ParentAccountId.Value);
                }

                updated.MainParentAccountId = updatedRoot?.Id;
                updated.UpdatedAt = DateTime.Now;

                existing.AccountDetails = updated;
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
            var account = await _context.Accounts
                .Include(a => a.AccountDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (account is null)
            {
                return new ResultMessage { Success = true };
            }

            if (_accountCodes.IsSystemCode(account.Code))
            {
                return new ResultMessage { Success = false, Message = "SystemAccountCannotBeDeleted" };
            }

            if (IsOwnedType(account.AccountType))
            {
                return new ResultMessage { Success = false, Message = "AccountOwnedByAnotherScreen" };
            }

            if (await _context.Accounts.AnyAsync(a => a.ParentAccountId == id)
                || await _context.EntryDetails.AnyAsync(a => a.AccountId == id)
                || await _context.EntryMasters.AnyAsync(a => a.AccountBoxOrBankId == id)
                || await _context.BillMasters.AnyAsync(a => a.CustomerOrSupplierAccountId == id || a.AccountPayId == id)
                || await _context.AccountDetails.AnyAsync(a => a.DepreciationAccountId == id || a.DepreciationExpensesAccountId == id))
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
