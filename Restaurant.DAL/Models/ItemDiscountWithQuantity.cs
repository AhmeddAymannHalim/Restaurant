using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class ItemDiscountWithQuantity : BaseEntity
{
    public int? ItemId { get; set; }
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public virtual Item Item { get; set; }
}
