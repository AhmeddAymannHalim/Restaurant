using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IDeliveryZoneService
{
    Task<PaginatedResult<DeliveryZoneViewModel>> GetPagedDeliveryZonesAsync(PageRequest request);
    Task<List<DeliveryZoneViewModel>> GetAllDeliveryZonesAsync();
    Task<DeliveryZoneViewModel> GetDeliveryZoneByIdAsync(int id);
    Task<ResultMessage> UpsertDeliveryZoneAsync(DeliveryZoneViewModel model);
    Task<ResultMessage> DeleteDeliveryZoneAsync(int id);
}
