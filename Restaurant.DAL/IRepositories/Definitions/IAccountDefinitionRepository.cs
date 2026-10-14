using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountDefinitionRepository
{
    Task<PaginatedResult<AccountDefinitionViewModel>> GetPagedAccountsAsync(PageRequest request, bool assetsOnly);
    Task<AccountDefinitionViewModel> GetAccountByIdAsync(int id);
    Task<string> NewAccountCodeAsync(int? parentId, AccountKind kind);
    Task<IEnumerable<LookupItem>> GetParentOptionsAsync(int? exceptId);
    Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync();
    Task<ResultMessage> UpsertAccountAsync(Account account, AccountDetails details, AccountKind kind, decimal openingBalance);
    Task<ResultMessage> DeleteAccountAsync(int id);
}
