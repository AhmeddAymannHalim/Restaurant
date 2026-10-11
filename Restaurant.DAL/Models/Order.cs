using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public enum OrderType
    {
        Hall, TakeAway, Delivery, Delivery_Online
    }
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
        public bool Complete { get; set; } = false;
        public bool FinishShift { get; set; }
        public string CustomerName { get; set; } = "عميل نقدي";


        public int? TableId { get; set; }   // لو الحجز صالة
        public int? CustomerId { get; set; }   //  لو الطلب خاص بعميل لدينا
        public int? EmployeeId { get; set; }    // لو الطلب دليفري هنختار طيار
        public string Address { get; set; }     // لو الطلب دليفري
        public string PhoneNumber { get; set; }    // لو الطلب دليفري
        public string MobileNumber { get; set; }    // لو الطلب دليفري




        public virtual ApplicationUser Seller { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        public virtual Table Table { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ICollection<OrderMenuItem> OrderMenuItems { get; set; }



    }
}
