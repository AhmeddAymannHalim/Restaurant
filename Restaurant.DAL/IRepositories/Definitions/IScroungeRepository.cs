using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IScroungeRepository
{
    Task<PaginatedResult<Scrounge>> GetPagedScroungesAsync(PageRequest request);
    Task<Scrounge> GetScroungeByIdAsync(int id);
    Task<ResultMessage> UpsertScroungeAsync(Scrounge entity, string userId);
    Task<ResultMessage> DeleteScroungeAsync(int id);
}
