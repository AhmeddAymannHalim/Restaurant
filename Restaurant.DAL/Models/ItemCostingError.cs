using Shared.Enums;

namespace Restaurant.DAL.Models;

public class ItemCostingError : BaseEntity
{
    public string Message { get; set; }
    public int? ItemId { get; set; }

    public virtual Item Item { get; set; }
}
