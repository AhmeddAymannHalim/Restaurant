namespace Restaurant.DAL.Models;

public class PosBillIngredient
{
    public int Id { get; set; }
    public int PosBillLineId { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }

    public virtual PosBillLine PosBillLine { get; set; }
}
