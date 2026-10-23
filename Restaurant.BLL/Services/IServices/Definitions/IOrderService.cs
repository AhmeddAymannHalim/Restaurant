using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IOrderService
{
    Task<PaginatedResult<OrderViewModel>> GetPagedOrdersAsync(PageRequest request, string userId);
    Task<OrderViewModel> GetOrderByIdAsync(int id);
    Task<OrderViewModel> GetPendingHallOrderByIdAsync(int id);
    Task<OrderViewModel> GetNewOrderAsync(int? tableId, int? reservationId);
    Task<IEnumerable<LookupItem>> GetPendingTablesAsync(int exceptTableId);

    Task<ResultMessage> CreateOrderAsync(OrderViewModel model, string userId);
    Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method);
    Task<ResultMessage> MoveItemAsync(OrderMoveViewModel model);
    Task<ResultMessage> MergeOrderAsync(OrderMoveViewModel model);
    Task<ResultMessage> DeleteOrderAsync(int id);
    Task<IEnumerable<OrderViewModel>> GetKitchenOrdersAsync();
    Task<ResultMessage> CompleteOrderAsync(int id);

}
