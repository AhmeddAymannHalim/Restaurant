using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class OnlineOrder
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsPending { get; set; }


        public virtual ApplicationUser Customer { get; set; }
        public virtual ICollection<OnlineOrderMenuItem> OnlineOrderMenuItems { get; set; }
    }

    public class OnlineOrderMenuItem
    {
        public int Id { get; set; }
        public int OnlineOrderId { get; set; }
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceForUnit { get; set; }
        public decimal Price { get; set; }



        public virtual OnlineOrder OnlineOrder { get; set; }
        public virtual MenuItem MenuItem { get; set; }
    }
}
