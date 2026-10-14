namespace Restaurant.DAL.Models;

public class CustomerPhone : BaseEntity, IPhoneRow
{
    public string PhoneNumber { get; set; }
    public int CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
