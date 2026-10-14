using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class ItemLocation : BaseEntity
{
    public string Location { get; set; }
    public int? StoreId { get; set; }
    public int? ItemId { get; set; }

    public virtual Store Store { get; set; }
    public virtual Item Item { get; set; }
}
