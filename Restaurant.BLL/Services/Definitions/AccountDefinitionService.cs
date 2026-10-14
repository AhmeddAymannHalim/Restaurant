using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class AccountDefinitionService : IAccountDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountDefinitionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly)
    {
        return await _unitOfWork.AccountDefinitions.GetPagedAccountsAsync(request, assetsOnly);
    }

    public async Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id)
    {
        return await _unitOfWork.AccountDefinitions.GetAccountByIdAsync(id);
    }

    public async Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind)
    {
        return await _unitOfWork.AccountDefinitions.NewAccountCodeAsync(parentId, kind);
    }

    public async Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? exceptId)
    {
        return await _unitOfWork.AccountDefinitions.GetParentOptionsAsync(exceptId);
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        return await _unitOfWork.AccountDefinitions.GetPostingAccountOptionsAsync();
    }

    public async Task<ResultMessage> UpsertAccountAsync(AccountDefinitionViewModel model)
    {
        var account = new Account
        {
            Id = model.Id,
            ArName = model.ArName?.Trim(),
            EnName = model.EnName?.Trim(),
            ParentAccountId = model.ParentAccountId,
            CurrencyId = model.CurrencyId,
            Remarks = model.Remarks,
            MaxDebitAllowed = model.MaxDebitAllowed,
            RequiresCostCenter = model.RequiresCostCenter
        };

        var details = model.Kind == AccountKind.Asset
            ? new AccountDetails
            {
                Location = model.Location,
                BuyValue = model.BuyValue,
                BuyDate = model.BuyDate,
                StartUsingDate = model.StartUsingDate,
                ProductAge = model.ProductAge,
                DepreciationType = model.DepreciationType,
                ScrapValue = model.ScrapValue,
                DepreciationAccountId = model.DepreciationAccountId,
                DepreciationExpensesAccountId = model.DepreciationExpensesAccountId
            }
            : null;

        return await _unitOfWork.AccountDefinitions.UpsertAccountAsync(account, details, model.Kind, model.OpeningBalance);
    }

    public async Task<ResultMessage> DeleteAccountAsync(int id)
    {
        return await _unitOfWork.AccountDefinitions.DeleteAccountAsync(id);
    }
}
