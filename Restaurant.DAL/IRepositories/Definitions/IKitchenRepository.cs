using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IKitchenRepository
{
    Task<PaginatedResult<Kitchen>> GetPagedKitchensAsync(PageRequest request);
    Task<Kitchen> GetKitchenByIdAsync(int id);
    Task<ResultMessage> UpsertKitchenAsync(Kitchen entity);
    Task<ResultMessage> DeleteKitchenAsync(int id);
}
