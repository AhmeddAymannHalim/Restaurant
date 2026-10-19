using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class CustomerTier : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal? MinimumTotalAmount { get; set; }
    public virtual ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
    public virtual ICollection<CustomerTierDiscount> CustomerTierDiscounts { get; set; } = new HashSet<CustomerTierDiscount>();

}
