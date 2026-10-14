using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class DashboardViewModel
{
    public string UserName { get; set; }
    public string UserImage { get; set; }

    public int ShiftOrders { get; set; }
    public decimal ShiftSales { get; set; }
    public int WaitingOnline { get; set; }
    public int InKitchen { get; set; }
    public int AwaitingPayment { get; set; }
    public decimal AwaitingPaymentAmount { get; set; }

    public List<DashboardTypeRow> ByType { get; set; } = new();
    public List<DashboardOnlineItem> OnlineRequests { get; set; } = new();
    public List<DashboardKitchenItem> Kitchen { get; set; } = new();
    public List<DashboardUnpaidItem> Unpaid { get; set; } = new();
}

public class DashboardTypeRow
{
    public OrderType Type { get; set; }
    public int InKitchen { get; set; }
    public int Unpaid { get; set; }
    public int Paid { get; set; }
}

public class DashboardLine
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Quantity { get; set; }
}

public class DashboardOnlineItem
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public int? TableId { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public List<DashboardLine> Items { get; set; } = new();
}

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
