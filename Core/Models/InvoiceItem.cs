using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int StoreProductId { get; set; }
        public int UnitPriceId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }


        public virtual Invoice Invoice { get; set; }
        public virtual StoreProduct StoreProduct { get; set; }
        public virtual UnitPrice UnitPrice { get; set; }
    }
}
