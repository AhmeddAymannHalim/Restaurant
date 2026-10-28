using PaymentMethod = Shared.Enums.PaymentMethod;
using Shared.Enums;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderRepository
{
    Task<PaginatedResult<Order>> GetPagedOrdersAsync(PageRequest request);
    Task<Order> GetOrderByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<MenuItem>> GetCashierMenuAsync();

    Task<ApplicationSetting> GetSettingsAsync();
    Task<decimal> GetTaxPercentAsync(int? storeId);
    Task<Order> GetNewOrderAsync(int? tableId, int? reservationId);
    Task<Order> GetPendingHallOrderByIdAsync(int id);
    Task<IEnumerable<Table>> GetPendingTablesAsync(int exceptTableId);

    Task<ResultMessage> CreateOrderAsync(Order order, string userId);
    Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method, List<OrderPaymentViewModel> payments);
    Task<ResultMessage> MoveItemAsync(int orderMenuItemId, int targetTableId);
    Task<ResultMessage> MergeOrderAsync(int id, int targetTableId);
    Task<ResultMessage> DeleteOrderAsync(int id);

    Task<IEnumerable<Order>> GetKitchenOrdersAsync();
    Task<ResultMessage> CompleteOrderAsync(int id);

}
