namespace Restaurant.DAL.Models;

public class OrderIngredient
{
    public int Id { get; set; }
    public int OrderMenuItemId { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }

    public virtual OrderMenuItem OrderMenuItem { get; set; }
}
