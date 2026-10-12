namespace Restaurant.DAL.Models;

public class StoreProduct
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public byte[] RowVersion { get; set; }

    public virtual Store Store { get; set; }
    public virtual Product Product { get; set; }
    public virtual ICollection<StoreConvertProduct> StoreConvertProducts { get; set; }
}
