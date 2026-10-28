using Shared.Enums;

namespace Shared.ViewModels.Definitions;

// One table card on the dashboard floor.
public class DashboardTable
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public byte Seats { get; set; }
    public DashboardTableState State { get; set; }

    // Filled only when State is Occupied: the order that is open on this table.
    public int? OrderId { get; set; }

    // Filled only when State is Reserved: the booking that holds this table.
    public int? ReservationId { get; set; }
    public ReservationStatus? BookingStatus { get; set; }
    public string GuestName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? ReservedAt { get; set; }
    public byte Guests { get; set; }
}
