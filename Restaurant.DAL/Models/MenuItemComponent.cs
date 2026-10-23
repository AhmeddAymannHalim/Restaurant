namespace Restaurant.DAL.Models;

public class MenuItemComponent
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitPriceId { get; set; }
    // base units in one chosen unit, copied from the product when the recipe is saved
    public decimal TransRate { get; set; } = 1;

    public virtual MenuItem MenuItem { get; set; }
    public virtual Product Product { get; set; }
    public virtual UnitPrice UnitPrice { get; set; }

    public decimal QuantityInBase => Quantity * TransRate;
}
