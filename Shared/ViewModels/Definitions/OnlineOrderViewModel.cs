using Shared.ViewModels.Commons;
using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class OnlineOrderCreateViewModel
{
    public string CustomerName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public int? TableId { get; set; }
    public int? DeliveryZoneId { get; set; }
    public IEnumerable<LookupItem> Tables { get; set; } = new List<LookupItem>();
    public IEnumerable<DeliveryZoneViewModel> Zones { get; set; } = new List<DeliveryZoneViewModel>();
    public List<OnlineOrderLineViewModel> OnlineOrderMenuItems { get; set; } = new();
}

public class OnlineOrderLineViewModel
{
    public int MenuItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class OnlineMenuItemViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public string ArIngredients { get; set; }
    public string EnIngredients { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public Size? Size { get; set; }
    public string SectionArName { get; set; }
    public string SectionEnName { get; set; }
}
