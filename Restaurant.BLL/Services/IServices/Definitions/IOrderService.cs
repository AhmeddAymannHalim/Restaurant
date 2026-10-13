using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IOrderService
{
    Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId);
    Task<PaginatedResult<OrderViewModel>> GetPagedOrdersAsync(PageRequest request, string userId);
    Task<OrderViewModel> GetOrderByIdAsync(int id);
    Task<OrderViewModel> GetNewOrderAsync();
    Task<OrderViewModel> GetNewOrderFromOnlineAsync(int onlineOrderId);
    Task<IEnumerable<LookupItem>> GetPendingTablesAsync(int exceptTableId);

    Task<ResultMessage> CreateOrderAsync(OrderViewModel model, string userId);
    Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method);
    Task<ResultMessage> MoveItemAsync(OrderMoveViewModel model);
    Task<ResultMessage> MergeOrderAsync(OrderMoveViewModel model);
    Task<ResultMessage> DeleteOrderAsync(int id);
    Task<IEnumerable<IncompleteCashOrderViewModel>> GetIncompleteCashOrdersAsync(string userId);
    Task<ResultMessage> FinishShiftAsync(string userId, decimal countedCash);
    Task<bool> VerifyShiftPasswordAsync(string password);
    Task<IEnumerable<OrderViewModel>> GetKitchenOrdersAsync();
    Task<ResultMessage> CompleteOrderAsync(int id);

    Task<IEnumerable<PendingOnlineOrderViewModel>> GetPendingOnlineOrdersAsync();
    Task<ResultMessage> CancelOnlineOrderAsync(int id);
    Task<ResultMessage> RejectOnlineOrderAsync(int id, string reason);
}
