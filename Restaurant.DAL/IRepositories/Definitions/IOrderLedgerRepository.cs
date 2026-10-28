using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IOrderLedgerRepository
{
    Task<BillSetting> GetSettingAsync();
    Task<int> GetItemIdAsync(MenuItem dish);
    Task<bool> PostEntryAsync(Order order);
    Task RemoveEntryAsync(int billId);
    void RecalculateBill(Order order);
    Task<string> SetPaymentsAsync(Order order, List<Shared.ViewModels.Definitions.OrderPaymentViewModel> payments);
    Task RemoveOrderAsync(Order order);
}
