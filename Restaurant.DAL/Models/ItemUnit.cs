using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class ItemUnit : BaseEntity
{
    public double TransRate { get; set; }
    public bool IsDefault { get; set; }
    public string Barcode { get; set; }

    public decimal? Whole { get; set; }
    public decimal? HalfWhole { get; set; }
    public decimal? Retail { get; set; }
    public decimal? EndUsers { get; set; }
    public decimal? WeightedAverage { get; set; }
    public decimal? Export { get; set; }
    public decimal? LastPurchase { get; set; }

    public int? ItemId { get; set; }
    public int? UnitId { get; set; }
    public virtual Item Item { get; set; }
    public virtual Unit Unit { get; set; }

}
