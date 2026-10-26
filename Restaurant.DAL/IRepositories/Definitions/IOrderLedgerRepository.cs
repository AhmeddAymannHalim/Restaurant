using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderLedgerRepository
{
    Task<BillSetting> GetSettingAsync();
    Task<int> GetItemIdAsync(MenuItem dish);
    Task<bool> PostEntryAsync(Order order);
    Task RemoveEntryAsync(int billId);
    void RecalculateBill(Order order, decimal taxPercent);
    Task RemoveOrderAsync(Order order);
}
