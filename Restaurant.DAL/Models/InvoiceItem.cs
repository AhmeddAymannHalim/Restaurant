namespace Restaurant.DAL.Models;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public int UnitPriceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }

    public virtual Invoice Invoice { get; set; }
    public virtual Product Product { get; set; }
    public virtual UnitPrice UnitPrice { get; set; }

    public decimal Total => Quantity * Price;
}
