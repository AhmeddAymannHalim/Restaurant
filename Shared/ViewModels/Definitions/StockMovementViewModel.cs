using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class StockMovementViewModel
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityDelta { get; set; }
    public DateTime MovementDate { get; set; }
    public StockMovementSource Source { get; set; }
    public string ReferenceCode { get; set; }

    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }
    public string ProductArName { get; set; }
    public string ProductEnName { get; set; }
}

public class StockMovementIndexViewModel
{
    public PaginatedResult<StockMovementViewModel> Result { get; set; } = new();
    public int? StoreId { get; set; }
    public int? ProductId { get; set; }

    public IEnumerable<LookupItem> Stores { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Products { get; set; } = new List<LookupItem>();
}
