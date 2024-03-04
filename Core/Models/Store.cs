using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public int? StorekeeperId { get; set; }

        public virtual Employee Storekeeper { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<StoreProduct> StoreProducts { get; set; }

    }
}
