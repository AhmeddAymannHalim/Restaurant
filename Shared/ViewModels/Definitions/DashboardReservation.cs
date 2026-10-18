using Shared.Enums;

namespace Shared.ViewModels.Definitions;

// One row in the dashboard's "upcoming reservations" list.
public class DashboardReservation
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public byte GuestCount { get; set; }
    public DateTime ReservationDate { get; set; }
    public string Notes { get; set; }
    public ReservationStatus Status { get; set; }
}
