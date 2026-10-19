using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IBranchRepository
{
    Task<PaginatedResult<Branch>> GetPagedBranchesAsync(PageRequest request);
    Task<Branch> GetBranchByIdAsync(int id);
    Task<ResultMessage> UpsertBranchAsync(Branch entity);
    Task<ResultMessage> DeleteBranchAsync(int id);
    Task<string> NewBranchCodeAsync();
}
