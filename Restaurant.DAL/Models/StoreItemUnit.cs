
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class StoreItemUnit : BaseEntity
{
    public decimal? Quantity { get; set; }
    public decimal? MinQuantityOrder { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public int? StoreId { get; set; }
    public int? UnitId { get; set; }
    public int? ItemId { get; set; }

    public virtual Store Store { get; set; }
    public virtual Unit Unit { get; set; }
    public virtual Item Item { get; set; }
}
