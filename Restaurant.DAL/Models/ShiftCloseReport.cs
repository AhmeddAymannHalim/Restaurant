using System;

namespace Restaurant.DAL.Models
{
    public class ShiftCloseReport
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime ClosedAt { get; set; } = DateTime.Now;

        public decimal ExpectedCash { get; set; }

        public decimal CountedCash { get; set; }

        public decimal Variance { get; set; }

        public bool HadIncompleteOrders { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
