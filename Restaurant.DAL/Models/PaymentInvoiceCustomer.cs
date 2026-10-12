namespace Restaurant.DAL.Models
{
    public class PaymentInvoiceCustomer : PaymentInvoice
    {
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
