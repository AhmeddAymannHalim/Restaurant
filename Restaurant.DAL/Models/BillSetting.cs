
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class BillSetting : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArAbrvation { get; set; }
    public string EnAbrvation { get; set; }
    public BillType BillType { get; set; }
    public EntryGenerate EntryGenerate { get; set; }
    public bool EntryAutoPosted { get; set; }
    public bool PriceCostEffect { get; set; }
    public bool LastPayPrice { get; set; }
    public StorageEffect StorageEffect { get; set; }
    public double CurrencyRate { get; set; }
    public bool NegtiveQtyAllowed { get; set; }
    public bool FirstExpiredIsOut { get; set; }
    public bool MinQtyAllowed { get; set; }
    public bool OnlyDefaultUnit { get; set; }
    public PayType DefaultPayType { get; set; }
    public bool DisablePayType { get; set; }
    public bool ShowLastBalanceOfCustomer { get; set; }
    public bool CanRepeatSameItem { get; set; }
    public bool ShowCurrentItemQty { get; set; }
    public bool BillEntryWithMainCurrencyRate { get; set; }
    public double AdditionalPaymentCardPercentage { get; set; }
    public string PrintArName { get; set; }
    public string PrintEnName { get; set; }


    public int? AccountItemId { get; set; }
    public int? AccountDiscountId { get; set; }
    public int? AccountExtraId { get; set; }
    public int? AccountPayId { get; set; }
    public int? StoreId { get; set; }
    public int? ToStoreId { get; set; }
    public int? EntrySettingId { get; set; }
    public int? CurrencyId { get; set; }
    public int SellTypeId { get; set; }
    public int? AccountPaymentCardId { get; set; }
    public int? AccountPaymentCardCostId { get; set; }
    public int? CostCenterId { get; set; }
    
    public virtual Account AccountItem { get; set; }
    public virtual Account AccountDiscount { get; set; }
    public virtual Account AccountExtra { get; set; }
    public virtual Account AccountPay { get; set; }
    public virtual Store Store { get; set; }
    public virtual Store ToStore { get; set; }
    public virtual EntrySetting EntrySetting { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual SellType SellType { get; set; }
    public virtual Account AccountPaymentCard { get; set; }
    public virtual Account AccountPaymentCardCost { get; set; }
    public virtual CostCenter CostCenter { get; set; }
  
}
