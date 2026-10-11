using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class PaymentInvoiceSupplier : PaymentInvoice
    {
        public int SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
