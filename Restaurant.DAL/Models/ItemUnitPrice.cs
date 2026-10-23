namespace Restaurant.DAL.Models;

// The price of one unit of an item for one price type (SellType). Price types are a definition, so there is one row per type the user sets.
public class ItemUnitPrice
{
    public int Id { get; set; }
    public int ItemUnitId { get; set; }
    public int SellTypeId { get; set; }
    public decimal Price { get; set; }

    public virtual ItemUnit ItemUnit { get; set; }
    public virtual SellType SellType { get; set; }
}
