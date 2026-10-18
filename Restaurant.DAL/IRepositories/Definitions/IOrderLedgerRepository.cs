using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderLedgerRepository
{
    Task<BillSetting> GetSettingAsync();
    Task<Dictionary<int, int>> GetItemIdsAsync(IEnumerable<MenuItem> dishes);
    Task PostEntryAsync(Order order);
    Task RemoveEntryAsync(int billId);
}
