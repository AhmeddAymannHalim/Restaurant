using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class TableReservation
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string GuestName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public byte GuestCount { get; set; }
        public DateTime ReservationDate { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public string Notes { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Table Table { get; set; }
    }
}
