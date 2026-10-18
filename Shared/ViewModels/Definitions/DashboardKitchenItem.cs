using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class DashboardKitchenItem
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public OrderType Type { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public DateTime OrderDate { get; set; }
    public List<DashboardLine> Items { get; set; } = new();
}
