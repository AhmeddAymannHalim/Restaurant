namespace Shared.ViewModels.Definitions;

public class BillLineViewModel
{
    public int Id { get; set; }
    public int Row { get; set; }
    public string ItemCode { get; set; }
    public string ItemArName { get; set; }
    public string ItemEnName { get; set; }
    public string UnitArName { get; set; }
    public string UnitEnName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
