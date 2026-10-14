using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Commons;

public interface IJournalEntryRepository
{
    Task<string> GetNextEntryNumberAsync();
    Task EnsureDefaultAccountsAsync();
    Task<ResultMessage> PostSourceEntryAsync(JournalSourceType sourceType,
        int sourceId,
        DateTime date,
        string description,
        IEnumerable<JournalLineRequest> lines,
        bool saveChanges = true);
    Task RemoveSourceEntriesAsync(JournalSourceType sourceType, int sourceId);
}
