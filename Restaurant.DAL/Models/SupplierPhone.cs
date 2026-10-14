namespace Restaurant.DAL.Models;

public class SupplierPhone : BaseEntity, IPhoneRow
{
    public string PhoneNumber { get; set; }
    public int SupplierId { get; set; }

    public virtual Supplier Supplier { get; set; }
}
