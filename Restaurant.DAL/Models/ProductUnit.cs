namespace Restaurant.DAL.Models;

public class ProductUnit
{
    public int UnitPriceId { get; set; }
    public UnitPrice UnitPrice { get; set; }
    public decimal TransRate { get; set; } = 1;
    public bool IsDefault { get; set; }
    public decimal Price { get; set; }
}
