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

public class PrinterSettingViewModel
{
    public int Id { get; set; }
    public string PrinterForAllSections { get; set; }
    public string PrinterForCashier { get; set; }
    public string PrinterForEachSection { get; set; }

    public IEnumerable<LookupItem_String> Printers { get; set; } = new List<LookupItem_String>();
}
