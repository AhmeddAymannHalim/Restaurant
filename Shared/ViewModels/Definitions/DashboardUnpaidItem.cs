using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class DashboardUnpaidItem
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public OrderType Type { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
}
