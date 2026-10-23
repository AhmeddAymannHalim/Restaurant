namespace Restaurant.DAL.Models;

// One unit a product can be measured in. TransRate is how many base units (rate 1) the unit holds, e.g. kilogram = 1000 when gram is the base.
public class ProductUnit
{
    public int UnitPriceId { get; set; }
    public UnitPrice UnitPrice { get; set; }
    public decimal TransRate { get; set; } = 1;
    public bool IsDefault { get; set; }
    public string Barcode { get; set; }

    public decimal LastPurchase { get; set; }

    // the price of this unit for every price type that was filled in
    public List<ProductUnitPrice> Prices { get; set; } = new();
}

public class ProductUnitPrice
{
    public int SellTypeId { get; set; }
    public decimal Price { get; set; }
}
