using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{
    public class OrderViewModel
    {
        public string OrderCode { get; set; }
        public decimal Service { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderType Type { get; set; }
        public bool Cash { get; set; }
        public string CustomerName { get; set; } = "عميل نقدي";


        public int? TableId { get; set; }   // لو الحجز صالة
        public int? CustomerId { get; set; }   //  لو الطلب خاص بعميل لدينا
        public int? EmployeeId { get; set; }    // لو الطلب دليفري هنختار طيار
        public string Address { get; set; }     // لو الطلب دليفري
        public string PhoneNumber { get; set; }    // لو الطلب دليفري
        public string MobileNumber { get; set; }    // لو الطلب دليفري

        public List<OrderMenuItemViewModel> OrderMenuItems { get; set; } = new List<OrderMenuItemViewModel>();
    }


    public class OrderMenuItemViewModel
    {
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceForUnit { get; set; }
        public decimal Price { get; set; }


    }



    
}
