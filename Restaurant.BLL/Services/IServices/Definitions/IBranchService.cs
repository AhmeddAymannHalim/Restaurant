using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IBranchService
{
    Task<PaginatedResult<BranchViewModel>> GetPagedBranchesAsync(PageRequest request);
    Task<BranchViewModel> GetBranchByIdAsync(int id);
    Task<ResultMessage> UpsertBranchAsync(BranchViewModel model);
    Task<ResultMessage> DeleteBranchAsync(int id);
    Task<string> NewBranchCodeAsync();
}
