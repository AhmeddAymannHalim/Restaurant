using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Balance { get; set; } = 0;
        public int? CityId { get; set; }

        public virtual City City { get; set; }
        public virtual ICollection<PaymentInvoiceCustomer> PaymentInvoices { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
