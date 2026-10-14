using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IErpEntryRepository
{
    Task<PaginatedResult<ErpEntryViewModel>> GetPagedEntriesAsync(PageRequest request);
    Task<ErpEntryViewModel> GetEntryByIdAsync(int id);
    Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync();
    Task<ResultMessage> CreateManualEntryAsync(ErpEntryViewModel model);
    Task<ResultMessage> DeleteManualEntryAsync(int id);
}
