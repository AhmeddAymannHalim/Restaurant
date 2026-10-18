namespace Restaurant.DAL.Models
{
    public class PaymentInvoiceSupplier : PaymentInvoice
    {
        public int SupplierId { get; set; }
        public int? BillMasterId { get; set; }

        public virtual Supplier Supplier { get; set; }
        public virtual BillMaster BillMaster { get; set; }
    }
}
