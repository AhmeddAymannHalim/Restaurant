using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class MenuItemComponent
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public int StoreProductId { get; set; }

        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }

        public virtual StoreProduct StoreProduct { get; set; }
        public virtual MenuItem MenuItem { get; set; }

    }
}
