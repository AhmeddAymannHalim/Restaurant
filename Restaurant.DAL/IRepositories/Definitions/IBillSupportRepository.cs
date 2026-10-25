using Restaurant.DAL.Helpers;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IBillSupportRepository
{
    List<InvoiceTypeInfo> GetAllInvoiceTypes();
    InvoiceTypeInfo GetInvoiceType(InvoiceType type);
    List<BillType> GetInvoiceBillTypes();
    InvoiceType TypeOf(BillType billType);
    bool IsInvoiceBillType(BillType billType);
    BillType? BillTypeOfKind(string kind);
    Task<bool> IsInvoiceAsync(BillMaster bill);
    IQueryable<BillMaster> OfKind(IQueryable<BillMaster> query, params string[] kinds);
    Task<BillSetting> GetBillSettingAsync(BillType billType, string arName, string enName);
    Task<string> NextBillCodeAsync(int billSettingId);
    Task<EntrySetting> GetEntrySettingAsync(EntryType type, string arName, string enName);
    Task<int> GetItemIdAsync(int productId);
    Task<int> GetUnitIdAsync(int unitPriceId);
    int ProductIdOf(string itemCode);
    int UnitPriceIdOf(string unitCode);
    decimal? PriceOf(ItemUnit row, int sellTypeId);
    void SetPriceOf(ItemUnit row, int sellTypeId, decimal price);
    Task EnsureItemUnitAsync(int itemId, int unitId);
    Task LinkEntryAsync(BillMaster bill);
    Task RecalculateInvoicePaymentAsync(int billMasterId);
    Task<StoreItemUnit> GetStockRowAsync(int storeId, int itemId, int unitId);
}
