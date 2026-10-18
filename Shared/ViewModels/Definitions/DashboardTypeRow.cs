using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class DashboardTypeRow
{
    public OrderType Type { get; set; }
    public int InKitchen { get; set; }
    public int Unpaid { get; set; }
    public int Paid { get; set; }
}
