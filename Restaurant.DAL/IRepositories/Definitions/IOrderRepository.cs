using PaymentMethod = Shared.Enums.PaymentMethod;
using Shared.Enums;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderRepository
{
    Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId);
    Task<PaginatedResult<PosBill>> GetPagedOrdersAsync(PageRequest request, string userId);
    Task<PosBill> GetOrderByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<MenuItem>> GetPosMenuAsync();
    Task<ApplicationSetting> GetSettingsAsync();
    Task<IEnumerable<Table>> GetPendingTablesAsync(int exceptTableId);

    Task<ResultMessage> CreateOrderAsync(PosBill order, int? onlineOrderId, string userId);
    Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method);
    Task<ResultMessage> MoveItemAsync(int orderMenuItemId, int targetTableId);
    Task<ResultMessage> MergeOrderAsync(int id, int targetTableId);
    Task<ResultMessage> DeleteOrderAsync(int id);
    Task<IEnumerable<PosBill>> GetIncompleteCashOrdersAsync(string userId);
    Task<ResultMessage> FinishShiftAsync(string userId, decimal countedCash);
    Task<bool> VerifyShiftPasswordAsync(string password);

    Task<IEnumerable<PosBill>> GetKitchenOrdersAsync();
    Task<ResultMessage> CompleteOrderAsync(int id);

    Task<IEnumerable<OnlineOrder>> GetPendingOnlineOrdersAsync();
    Task<OnlineOrder> GetOnlineOrderAsync(int id);
    Task<ResultMessage> CancelOnlineOrderAsync(int id);
    Task<ResultMessage> RejectOnlineOrderAsync(int id, string reason);
}
