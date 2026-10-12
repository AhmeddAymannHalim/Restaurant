namespace Shared.ViewModels.Definitions;

public class DeliveryZoneViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Fee { get; set; }
}

public class SitePricingViewModel
{
    public decimal TaxPercent { get; set; }
    public decimal Service { get; set; }
    public List<DeliveryZoneViewModel> Zones { get; set; } = new();
}
