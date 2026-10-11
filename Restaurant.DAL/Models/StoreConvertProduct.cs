using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class StoreConvertProduct
    { 
        public int Id { get; set; }
        public int StoreConvertId { get; set; }
        public int StoreProductId { get; set; }
        public decimal Quantity { get; set; }


        public virtual StoreConvert StoreConvert { get; set; }
        public virtual StoreProduct StoreProduct { get; set; }
    }
}
