using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class CustomerTierDiscount : BaseEntity
{
    public int? CustomerTierId { get; set; }
    //public int? ItemVarintPriceId { get; set; }
    public virtual CustomerTier CustomerTier { get; set; }
    //public virtual ItemVariantPrice ItemVariantPrice { get; set; }
    public double DiscountPercentage { get; set; }
}
