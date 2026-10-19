using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IKitchenService
{
    Task<PaginatedResult<KitchenViewModel>> GetPagedKitchensAsync(PageRequest request);
    Task<KitchenViewModel> GetKitchenByIdAsync(int id);
    Task<ResultMessage> UpsertKitchenAsync(KitchenViewModel model);
    Task<ResultMessage> DeleteKitchenAsync(int id);
    Task<string> NewKitchenCodeAsync();
}
