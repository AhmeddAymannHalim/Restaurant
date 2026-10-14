using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IBranchRepository
{
    Task<PaginatedResult<CompanyBranch>> GetPagedBranchesAsync(PageRequest request);
    Task<CompanyBranch> GetBranchByIdAsync(int id);
    Task<ResultMessage> UpsertBranchAsync(CompanyBranch entity);
    Task<ResultMessage> DeleteBranchAsync(int id);
    Task<string> NewBranchCodeAsync();
}
