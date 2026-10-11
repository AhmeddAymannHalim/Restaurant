using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
    
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        public virtual ICollection<StoreProduct> StoreProducts { get; set; }


    }
}
