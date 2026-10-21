using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

// Used by the admin screens (list and edit).
public class TableReservationViewModel
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public byte GuestCount { get; set; } = 2;
    public DateTime ReservationDate { get; set; } = DateTime.Today.AddDays(1).AddHours(20);
    public int DurationMinutes { get; set; } = 60;
    public string Notes { get; set; }
    public ReservationStatus Status { get; set; }
    public string RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<LookupItem> Tables { get; set; } = new();
}
