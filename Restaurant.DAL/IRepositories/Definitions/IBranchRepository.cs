using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IBranchRepository
{
    Task<PaginatedResult<Branche>> GetPagedBranchesAsync(PageRequest request);
    Task<Branche> GetBranchByIdAsync(int id);
    Task<ResultMessage> UpsertBranchAsync(Branche entity);
    Task<ResultMessage> DeleteBranchAsync(int id);
    Task<string> NewBranchCodeAsync();
}
