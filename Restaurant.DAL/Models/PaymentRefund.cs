using System;
using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class PaymentRefund
    {
        public int Id { get; set; }

        public int OriginalPaymentId { get; set; }

        public decimal Amount { get; set; }

        public string Reason { get; set; }

        public string RequestedByUserId { get; set; }

        public string ApprovedByUserId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? ApprovedAt { get; set; }

        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        public virtual PaymentInvoiceCustomer OriginalPayment { get; set; }

        public virtual ApplicationUser RequestedByUser { get; set; }

        public virtual ApplicationUser ApprovedByUser { get; set; }
    }
}
