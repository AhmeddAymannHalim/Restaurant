using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class ItemDiscountWithQuantity : BaseEntity
{
    public decimal MinQuantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int? ItemId { get; set; }

    public virtual Item Item { get; set; }
}
