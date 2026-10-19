using AutoMapper;
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
    private readonly IMapper _mapper;

    public AccountDefinitionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly)
    {
        var result = await _unitOfWork.AccountDefinitions.GetPagedAccountsAsync(request, assetsOnly);

        return PaginatedResult<AccountDefinitionViewModel>.Create(_mapper.Map<List<AccountDefinitionViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id)
    {
        return _mapper.Map<AccountDefinitionViewModel>(await _unitOfWork.AccountDefinitions.GetAccountByIdAsync(id));
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
        var account = _mapper.Map<Account>(model);
        var details = model.Kind == AccountKind.Asset ? _mapper.Map<AccountDetails>(model) : null;

        return await _unitOfWork.AccountDefinitions.UpsertAccountAsync(account, details, model.Kind, model.OpeningBalance);
    }

    public async Task<ResultMessage> DeleteAccountAsync(int id)
    {
        return await _unitOfWork.AccountDefinitions.DeleteAccountAsync(id);
    }
}
