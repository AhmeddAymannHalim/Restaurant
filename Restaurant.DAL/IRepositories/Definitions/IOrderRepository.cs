using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderRepository
{
    Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId);
    Task<PaginatedResult<Order>> GetPagedOrdersAsync(PageRequest request, string userId);
    Task<Order> GetOrderByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<MenuItem>> GetPosMenuAsync();
    Task<ApplicationSetting> GetSettingsAsync();
    Task<IEnumerable<Table>> GetPendingTablesAsync(int exceptTableId);

    Task<ResultMessage> CreateOrderAsync(Order order, int? onlineOrderId, string userId);
    Task<ResultMessage> PayOrderAsync(int id, string userId);
    Task<ResultMessage> MoveItemAsync(int orderMenuItemId, int targetTableId);
    Task<ResultMessage> MergeOrderAsync(int id, int targetTableId);
    Task<ResultMessage> DeleteOrderAsync(int id);
    Task<ResultMessage> FinishShiftAsync(string userId);
    Task<bool> VerifyShiftPasswordAsync(string password);

    Task<IEnumerable<Order>> GetKitchenOrdersAsync();
    Task<ResultMessage> CompleteOrderAsync(int id);

    Task<IEnumerable<OnlineOrder>> GetPendingOnlineOrdersAsync();
    Task<OnlineOrder> GetOnlineOrderAsync(int id);
    Task<ResultMessage> CancelOnlineOrderAsync(int id);
}
