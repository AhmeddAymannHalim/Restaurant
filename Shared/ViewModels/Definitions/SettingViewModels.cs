using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class ApplicationSettingViewModel
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

public class RestaurantSettingViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string ArLocation { get; set; }
    public string EnLocation { get; set; }
    public string StartDay { get; set; }
    public string EndDay { get; set; }
    public string StartHour { get; set; }
    public string EndHour { get; set; }
    public string ImageUrl { get; set; }
    public string VideoUrl { get; set; }
    public IFormFile Image { get; set; }
    public IFormFile Video { get; set; }
}

public class PrinterSettingViewModel
{
    public int Id { get; set; }
    public string PrinterForAllSections { get; set; }
    public string PrinterForCashier { get; set; }
    public string PrinterForEachSection { get; set; }

    public IEnumerable<LookupItem_String> Printers { get; set; } = new List<LookupItem_String>();
}
