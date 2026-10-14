namespace Restaurant.DAL.Models;

public class ProductStock
{
    public int StoreId { get; set; }
    public Store Store { get; set; }
    public decimal Quantity { get; set; }
}
