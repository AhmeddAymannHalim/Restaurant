namespace Shared.ViewModels.Definitions;

// Used by the public booking page.
public class ReservationCreateViewModel
{
    public int TableId { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public byte GuestCount { get; set; } = 2;
    public DateTime? ReservationDate { get; set; }
    public string Notes { get; set; }
}
