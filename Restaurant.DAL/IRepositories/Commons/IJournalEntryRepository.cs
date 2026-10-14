using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Commons;

public interface IJournalEntryRepository
{
    Task<string> GetNextEntryNumberAsync();
    Task EnsureDefaultAccountsAsync();
    Task<ResultMessage> PostEntryAsync(JournalEntry entry, bool saveChanges = true);
    Task<ResultMessage> PostSourceEntryAsync(JournalSourceType sourceType,
        int sourceId,
        DateTime date,
        string description,
        IEnumerable<JournalLineRequest> lines,
        bool saveChanges = true);
    Task RemoveSourceEntriesAsync(JournalSourceType sourceType, int sourceId);
    Task<IEnumerable<TrialBalanceLine>> GetTrialBalanceAsync(DateTime? from = null, DateTime? to = null);
}
