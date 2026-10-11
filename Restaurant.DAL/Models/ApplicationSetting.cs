
namespace Restaurant.Data.Models
{
    public class ApplicationSetting
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public decimal OrderTax { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal OrderService { get; set; }
        public bool PrintAfterOrder { get; set; }
        public bool PrintForTakeawayOrder { get; set; }
        public bool PrintForKitchen { get; set; }
        public bool ShowTotalShift { get; set; }
        public byte NumberOfCustomerPrint { get; set; }
        public byte NumberOfKitchenPrint { get; set; }
    }
}
