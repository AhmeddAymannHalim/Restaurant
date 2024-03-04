using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        public virtual ICollection<StoreProduct> StoreProducts { get; set; }


    }
}
