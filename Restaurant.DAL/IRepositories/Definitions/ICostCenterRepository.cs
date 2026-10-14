using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICostCenterRepository
{
    Task<PaginatedResult<CostCenter>> GetPagedCostCentersAsync(PageRequest request);
    Task<CostCenter> GetCostCenterByIdAsync(int id);
    Task<string> NewCostCenterCodeAsync();
    Task<ResultMessage> UpsertCostCenterAsync(CostCenter entity);
    Task<ResultMessage> DeleteCostCenterAsync(int id);
}
