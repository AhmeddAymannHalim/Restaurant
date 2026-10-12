using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class ProductViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Cost { get; set; }
    public int ProductCategoryId { get; set; }

    public string ProductCategoryArName { get; set; }
    public string ProductCategoryEnName { get; set; }

    public decimal TotalStock { get; set; }

    public List<ProductStockViewModel> Stocks { get; set; } = new();

    public IEnumerable<LookupItem> Categories { get; set; } = new List<LookupItem>();
}

public class ProductStockViewModel
{
    public int StoreId { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public decimal Quantity { get; set; }
}
