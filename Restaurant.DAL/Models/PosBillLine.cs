namespace Restaurant.DAL.Models;

public class PosBillLine
{
    public int Id { get; set; }
    public int PosBillId { get; set; }
    public int MenuItemId { get; set; }

    public virtual BillDetail BillDetail { get; set; }
    public virtual PosBill PosBill { get; set; }
    public virtual MenuItem MenuItem { get; set; }
    public virtual ICollection<PosBillIngredient> Ingredients { get; set; } = new List<PosBillIngredient>();

    public decimal Quantity => BillDetail?.Quantity ?? 0;
    public decimal PriceForUnit => BillDetail?.UnitPrice ?? 0;
    public decimal Price => BillDetail?.TotalAmount ?? 0;
    public decimal UnitCost => BillDetail?.UnitCostAtPosting ?? 0;
}
