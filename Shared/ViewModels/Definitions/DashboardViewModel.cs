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
    public int WaitingReservations { get; set; }

    public List<DashboardTypeRow> ByType { get; set; } = new();
    public List<DashboardOnlineItem> OnlineRequests { get; set; } = new();
    public List<DashboardKitchenItem> Kitchen { get; set; } = new();
    public List<DashboardUnpaidItem> Unpaid { get; set; } = new();
    public List<DashboardTable> Tables { get; set; } = new();
    public List<DashboardReservation> Reservations { get; set; } = new();
}
