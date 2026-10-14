using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class BillMaster : BaseEntity
{
    public string Code { get; set; }
    public string CodeWithoutSuffix { get; set; }
    public DateTime Date { get; set; }
    public decimal? CurrencyRate { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? BillCost { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? BillProfit { get; set; }
    public string Remarks { get; set; }

    public decimal TotalAmountBeforeDiscountOrExtra { get; set; }
    public decimal TotalItemsDiscountValue { get; set; }
    public decimal TotalItemsExtraValue { get; set; }
    public decimal TotalItemsAmountAfterDiscountAndExtra { get; set; }

    public decimal BillDiscountPercentage { get; set; }
    public decimal BillDiscountValue { get; set; }
    public decimal BillExtraValue { get; set; }
    public decimal BillExtraPercentage { get; set; }

    public decimal TotalDiscountValueOnBillAndItems { get; set; }
    public decimal TotalExtraValueOnBillAndItems { get; set; }
    public decimal TotalBillAmountAfterDiscountAndExtra { get; set; }

    public PayType? PayType { get; set; }
    public bool IsStorageEffected { get; set; }
    public bool IsEntryPosted { get; set; }
    public bool IsCostingApplied { get; set; }
    public int BillDailyNo { get; set; }
    public decimal AmountPaided { get; set; }
    public decimal CasherCurrentPaidedAmount { get; set; }
    public decimal CasherDifferenceAmount { get; set; }
    public decimal CasherRemainingAmount { get; set; }
    public string CustomerOrSupplierPhoneNumber { get; set; }

    #region Zatca
    public SupportedInvoiceTypes ZatcaInvType { get; set; }
    public bool IsExport { get; set; } = false;
    public string OriginalBillNumber { get; set; }
    public string ZatcaReason { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? LastDeliveryDate { get; set; }
    public string VatCategoryCode { get; set; }
    public string VatCategorySubTypeCode { get; set; }
    public bool IsSentToZatca { get; set; } = false;
    #endregion

    #region ETA
    public string EtaUuid { get; set; }
    public string EtaSubmissionId { get; set; }
    public bool IsSentToEta { get; set; }
    #endregion

    #region Offline
    public string OfflineLocalId { get; set; }
    #endregion

    public int? EntryId { get; set; }

    public int? BillSettingId { get; set; }
    public int? CostCenterId { get; set; }
    public int? EmployeeId { get; set; }
    public int? SellTypeId { get; set; }
    public int? CurrencyId { get; set; }
    public int? StoreId { get; set; }
    public int? ToStoreId { get; set; }
    public int? CustomerOrSupplierAccountId { get; set; }
    public int? AccountItemId { get; set; }
    public int? AccountPayId { get; set; }
    public int? AccountDiscountId { get; set; }
    public int? AccountExtraId { get; set; }
    public int? CompanyBranchId { get; set; }

    public virtual BillSetting BillSetting { get; set; }
    public virtual CostCenter CostCenter { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual CompanyBranch CompanyBranch { get; set; }
    public virtual SellType SellType { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual Store Store { get; set; }
    public virtual Store ToStore { get; set; }
    public virtual Account CustomerOrSupplierAccount { get; set; }
    public virtual Account AccountPay { get; set; }
    public virtual Account AccountDiscount { get; set; }
    public virtual Account AccountExtra { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}
