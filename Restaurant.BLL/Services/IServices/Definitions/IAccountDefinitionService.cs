using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IAccountDefinitionService
{
    Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly);
    Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id);
    Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind);
    Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? exceptId);
    Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync();
    Task<ResultMessage> UpsertAccountAsync(AccountDefinitionViewModel model);
    Task<ResultMessage> DeleteAccountAsync(int id);
}
