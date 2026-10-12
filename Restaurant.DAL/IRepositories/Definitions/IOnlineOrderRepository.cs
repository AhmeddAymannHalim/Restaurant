using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOnlineOrderRepository
{
    Task<IEnumerable<MenuItem>> GetMenuAsync();
    Task<ResultMessage> CreateOrderAsync(OnlineOrder order, string customerId);
    Task<IEnumerable<OnlineOrder>> GetCustomerOrdersAsync(string customerId);
    Task<OnlineOrder> GetOrderAsync(int id);
    Task<ResultMessage> CancelOrderAsync(int id, string userId, bool isStaff);
}
