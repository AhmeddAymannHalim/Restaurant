using Shared.ViewModels.Commons;
using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class MenuItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int MenuSectionId { get; set; }
    public string MenuSectionArName { get; set; }
    public string MenuSectionEnName { get; set; }
    public Size? Size { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string ImageUrl { get; set; }
    public IFormFile ImageFile { get; set; }
    public int StoreId { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public decimal Price { get; set; }

    public decimal TotalCost { get; set; }
    public string ArIngredients { get; set; }
    public string EnIngredients { get; set; }
    public decimal Benefit { get; set; }

    public List<MenuItemComponentViewModel> Components { get; set; } = new();

    public IEnumerable<LookupItem> MenuSections { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Stores { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Products { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Categories { get; set; } = new List<LookupItem>();
}

public class MenuItemComponentViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductArName { get; set; }
    public string ProductEnName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Cost { get; set; }
}

public class StoreProductLookupItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public int? ProductCategoryId { get; set; }
    public string ProductArName { get; set; }
    public string ProductEnName { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
}
