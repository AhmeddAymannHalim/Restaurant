namespace Restaurant.DAL.Models;

public class OrderMenuItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }

    public virtual BillDetail BillDetail { get; set; }
    public virtual Order Order { get; set; }
    public virtual MenuItem MenuItem { get; set; }
    public virtual ICollection<OrderIngredient> Ingredients { get; set; } = new List<OrderIngredient>();

    public decimal Quantity => BillDetail?.Quantity ?? 0;
    public decimal PriceForUnit => BillDetail?.UnitPrice ?? 0;
    public decimal Price => BillDetail?.TotalAmount ?? 0;
    public decimal UnitCost => BillDetail?.UnitCostAtPosting ?? 0;
}
