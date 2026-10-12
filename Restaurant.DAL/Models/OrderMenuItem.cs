using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class OrderMenuItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceForUnit { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Price { get; set; }

        public virtual Order Order { get; set; }
        public virtual MenuItem MenuItem { get; set; }
        public virtual ICollection<OrderIngredient> Ingredients { get; set; } = new List<OrderIngredient>();
    }
}
