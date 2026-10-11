using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal? LastBalance { get; set; }


        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<PaymentInvoiceSupplier> PaymentInvoices { get; set; }

    }
}
