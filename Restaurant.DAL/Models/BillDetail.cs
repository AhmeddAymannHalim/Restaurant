using Shared.Enums;

namespace Restaurant.DAL.Models;

public class BillDetail : BaseEntity
{
    public byte? RowNumber { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TransRate { get; set; }
    public string DetailRemarks { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? DiscountAmountValue { get; set; }
    public decimal? DiscountAmountPercentage { get; set; }
    public decimal? ExtraAmountValue { get; set; }
    public decimal? ExtraAmountPercentage { get; set; }
    public decimal? TotalAmount { get; set; }

    public decimal? OwnDiscountPercentage { get; set; }

    public decimal? OwnDiscountValue { get; set; }

    public decimal? UnitCostAtPosting { get; set; }

    public decimal? DiscountFromBillLevel { get; set; }

    public int? BillMasterId { get; set; }
    public int? UnitId { get; set; }
    public int? ItemId { get; set; }
    public int? StoreId { get; set; }

    public virtual BillMaster BillMaster { get; set; }
    public virtual Unit Unit { get; set; }
    public virtual Item Item { get; set; }
    public virtual Store Store { get; set; }
}
