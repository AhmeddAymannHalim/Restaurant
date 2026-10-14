namespace Restaurant.DAL.Models;

public class StorePhone : BaseEntity, IPhoneRow
{
    public string PhoneNumber { get; set; }
    public int StoreId { get; set; }

    public virtual Store Store { get; set; }
}
