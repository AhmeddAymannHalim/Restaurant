using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class StoreItemUnit : BaseEntity
{
    public decimal Quantity { get; set; }
    public int? StoreId { get; set; }
    public int? ItemId { get; set; }
    public int? UnitId { get; set; }

    public virtual Store Store { get; set; }
    public virtual Item Item { get; set; }
    public virtual Unit Unit { get; set; }
}
