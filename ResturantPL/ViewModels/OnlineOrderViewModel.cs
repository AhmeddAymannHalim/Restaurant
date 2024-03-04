namespace ResturantPL.ViewModels
{
    public class OnlineOrderViewModel
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OnlineOrderMenuItemViewModel> OnlineOrderMenuItems { get; set; } = new List<OnlineOrderMenuItemViewModel>();
    }
   
    public class OnlineOrderMenuItemViewModel
    {
        public int MenuItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}
