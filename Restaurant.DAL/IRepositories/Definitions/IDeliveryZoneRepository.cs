using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IDeliveryZoneRepository
{
    Task<PaginatedResult<DeliveryZone>> GetPagedDeliveryZonesAsync(PageRequest request);
    Task<List<DeliveryZone>> GetAllDeliveryZonesAsync();
    Task<DeliveryZone> GetDeliveryZoneByIdAsync(int id);
    Task<ResultMessage> UpsertDeliveryZoneAsync(DeliveryZone entity);
    Task<string> NewDeliveryZoneCodeAsync();
    Task<ResultMessage> DeleteDeliveryZoneAsync(int id);
}
