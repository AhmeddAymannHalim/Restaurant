using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public decimal Service { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderType Type { get; set; }
        public DateTime OrderDate { get; set; }
        public string SellerId { get; set; }
        public string ReceiverId { get; set; }
        public bool Cash { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public bool OnAccount { get; set; }
        public bool Complete { get; set; } = false;
        public bool FinishShift { get; set; }
        public string CustomerName { get; set; } = "عميل نقدي";

        public int? TableId { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int? DeliveryZoneId { get; set; }
        public decimal DeliveryFee { get; set; }

        public virtual ApplicationUser Seller { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        public virtual Table Table { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual DeliveryZone DeliveryZone { get; set; }
        public virtual ICollection<OrderMenuItem> OrderMenuItems { get; set; }
    }
}
