namespace Restaurant.DAL.Models;

public class StoreConvertProduct
{
    public int Id { get; set; }
    public int StoreConvertId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }

    public virtual StoreConvert StoreConvert { get; set; }
    public virtual Product Product { get; set; }
}
