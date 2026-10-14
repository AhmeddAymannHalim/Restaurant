using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class ItemCostingError : BaseEntity
{
    public string Message { get; set; }
    public int? ItemId { get; set; }

    public virtual Item Item { get; set; }
}
