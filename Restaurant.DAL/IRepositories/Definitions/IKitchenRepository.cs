using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IKitchenRepository
{
    Task<PaginatedResult<kitchen>> GetPagedKitchensAsync(PageRequest request);
    Task<kitchen> GetKitchenByIdAsync(int id);
    Task<ResultMessage> UpsertKitchenAsync(kitchen entity);
    Task<ResultMessage> DeleteKitchenAsync(int id);
}
