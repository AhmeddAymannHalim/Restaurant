using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public enum PaymentType
    {
        Pay, 
        Recieve
    }

    public class PaymentInvoiceCustomer  : PaymentInvoice
    {
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

    }
}
