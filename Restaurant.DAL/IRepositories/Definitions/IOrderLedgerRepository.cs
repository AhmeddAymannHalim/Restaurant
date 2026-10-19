using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderLedgerRepository
{
    Task<BillSetting> GetSettingAsync();
    Task<int> GetItemIdAsync(MenuItem dish);
    Task PostEntryAsync(Order order);
    Task RemoveEntryAsync(int billId);
}
