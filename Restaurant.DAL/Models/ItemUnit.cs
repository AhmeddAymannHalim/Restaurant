using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class ItemUnit : BaseEntity
{
    public decimal Factor { get; set; } = 1;
    public int? ItemId { get; set; }
    public int? UnitId { get; set; }

    public virtual Item Item { get; set; }
    public virtual Unit Unit { get; set; }
}
