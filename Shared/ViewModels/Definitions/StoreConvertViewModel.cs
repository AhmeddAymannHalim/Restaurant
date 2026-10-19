using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class StoreConvertViewModel
{
    public int Id { get; set; }
    public string ConvertCode { get; set; }
    public DateTime ConvertDate { get; set; } = DateTime.Now;
    public int? FromStoreId { get; set; }
    public int? ToStoreId { get; set; }

    public string FromStoreArName { get; set; }
    public string FromStoreEnName { get; set; }
    public string ToStoreArName { get; set; }
    public string ToStoreEnName { get; set; }
    public int ProductsCount { get; set; }

    public List<StoreConvertItemViewModel> Items { get; set; } = new();

    public IEnumerable<LookupItem> Stores { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Categories { get; set; } = new List<LookupItem>();
}

public class StoreConvertItemViewModel
{
    public int ProductId { get; set; }
    public string ProductArName { get; set; }
    public string ProductEnName { get; set; }
    public decimal Quantity { get; set; }
}
