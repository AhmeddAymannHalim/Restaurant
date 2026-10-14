using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class OnlineOrder
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsPending { get; set; }
        public OnlineOrderStatus Status { get; set; } = OnlineOrderStatus.Pending;
        public string RejectionReason { get; set; }
        public int? TableId { get; set; }
        public int? DeliveryZoneId { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Service { get; set; }

        public virtual ApplicationUser Customer { get; set; }
        public virtual DeliveryZone DeliveryZone { get; set; }
        public virtual Table Table { get; set; }
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
