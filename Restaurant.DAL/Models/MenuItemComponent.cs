namespace Restaurant.DAL.Models;

public class MenuItemComponent
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }

    public virtual MenuItem MenuItem { get; set; }
    public virtual Product Product { get; set; }
}
