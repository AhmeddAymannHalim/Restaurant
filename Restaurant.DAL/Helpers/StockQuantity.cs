namespace Restaurant.DAL.Helpers;

// How much of one product is in one store.
public class StockQuantity
{
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}
