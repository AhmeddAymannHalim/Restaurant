using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICostCenterService
{
    Task<PaginatedResult<CostCenterViewModel>> GetPagedCostCentersAsync(PageRequest request);
    Task<CostCenterViewModel> GetCostCenterByIdAsync(int id);
    Task<ResultMessage> UpsertCostCenterAsync(CostCenterViewModel model);
    Task<ResultMessage> DeleteCostCenterAsync(int id);
    Task<string> NewCostCenterCodeAsync();
}
