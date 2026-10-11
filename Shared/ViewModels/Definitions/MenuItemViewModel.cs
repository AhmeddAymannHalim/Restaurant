using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class MenuItemViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int MenuSectionId { get; set; }
    public string MenuSectionArName { get; set; }
    public string MenuSectionEnName { get; set; }
    public Size? Size { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public IFormFile ImageFile { get; set; }
    public int StoreId { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Price { get; set; }
    public decimal Benefit { get; set; }

    public List<MenuItemComponentViewModel> Components { get; set; } = new();

    // Lookups for the Upsert form
    public SelectList MenuSections { get; set; }
    public SelectList Stores { get; set; }
}

public class MenuItemComponentViewModel
{
    public int Id { get; set; }
    public int StoreProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
}

// Returned by the AJAX endpoints that feed the component picker (and the invoice screens).
public class StoreProductLookupItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public string ProductName { get; set; }
    public string StoreName { get; set; }
    public decimal Quantity { get; set; }
    public decimal PriceForUnit { get; set; }
}
