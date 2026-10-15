using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IEntryRepository
{
    Task<PaginatedResult<EntryViewModel>> GetPagedEntriesAsync(PageRequest request);
    Task<EntryViewModel> GetEntryByIdAsync(int id);
    Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync();
    Task<ResultMessage> CreateManualEntryAsync(EntryViewModel model);
    Task<ResultMessage> DeleteManualEntryAsync(int id);
}
