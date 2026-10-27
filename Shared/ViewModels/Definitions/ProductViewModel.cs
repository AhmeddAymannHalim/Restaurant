using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Cost { get; set; }
    public int? ProductCategoryId { get; set; }

    public string ProductCategoryArName { get; set; }
    public string ProductCategoryEnName { get; set; }

    public decimal TotalStock { get; set; }

    public List<ProductStockViewModel> Stocks { get; set; } = new();
    public List<ProductUnitViewModel> Units { get; set; } = new();

    public IEnumerable<LookupItem> Categories { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> UnitList { get; set; } = new List<LookupItem>();
}

public class ProductUnitViewModel
{
    public int UnitPriceId { get; set; }
    public string UnitArName { get; set; }
    public string UnitEnName { get; set; }
    public decimal TransRate { get; set; } = 1;
    public bool IsDefault { get; set; }
    public decimal Price { get; set; }
}

public class ProductStockViewModel
{
    public int StoreId { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public decimal Quantity { get; set; }
    public decimal MinQuantity { get; set; }
}
