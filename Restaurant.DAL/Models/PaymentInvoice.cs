namespace Restaurant.DAL.Models
{
    public class PaymentInvoice
    {
        public int Id { get; set; }
        public string ProcessCode { get; set; }
        public DateTime Date { get; set; }
        public decimal Paid { get; set; }
        public string Description { get; set; }

        // filled only when a payment is read for its refund form
        public decimal RemainingRefundable { get; set; }
        public List<PaymentRefund> Refunds { get; set; } = new();
    }
}
