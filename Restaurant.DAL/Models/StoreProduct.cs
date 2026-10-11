using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class StoreProduct
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceForUnit { get; set; }


        public virtual Store Store { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<StoreConvertProduct> StoreConvertProducts { get; set; }
    }
}
