using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Balance { get; set; } = 0;


        public List<PaymentInvoiceCustomer> PaymentInvoices { get; set; } = new List<PaymentInvoiceCustomer>();
    }
}
