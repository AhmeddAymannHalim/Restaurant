using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IOnlineOrderService
{
    Task<IEnumerable<OnlineMenuItemViewModel>> GetMenuAsync();
    Task<ResultMessage> CreateOrderAsync(OnlineOrderCreateViewModel model, string customerId);
    Task<IEnumerable<PendingOnlineOrderViewModel>> GetCustomerOrdersAsync(string customerId);
    Task<PendingOnlineOrderViewModel> GetOrderAsync(int id);
    Task<ResultMessage> CancelOrderAsync(int id, string userId, bool isStaff);
}
