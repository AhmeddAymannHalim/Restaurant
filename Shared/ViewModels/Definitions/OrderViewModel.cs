using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class OrderViewModel
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderType Type { get; set; }
    public bool Cash { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public bool OnAccount { get; set; }
    public bool Complete { get; set; }
    public bool FinishShift { get; set; }

    public decimal Service { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DeliveryFee { get; set; }
    public int? DeliveryZoneId { get; set; }
    public string DeliveryZoneArName { get; set; }
    public string DeliveryZoneEnName { get; set; }
    public decimal SubTotal => Items.Sum(a => a.Price);

    public string CustomerName { get; set; }
    public int? CustomerId { get; set; }
    public int? TableId { get; set; }
    public int? EmployeeId { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string MobileNumber { get; set; }

    public int? OnlineOrderId { get; set; }

    public string SellerName { get; set; }
    public string ReceiverName { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public string EmployeeName { get; set; }
    public int ItemsCount { get; set; }

    public List<OrderItemViewModel> Items { get; set; } = new();

    public decimal TaxPercent { get; set; }
    public decimal DefaultService { get; set; }
    public IEnumerable<LookupItem> Customers { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Tables { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> DeliveryDrivers { get; set; } = new List<LookupItem>();
    public IEnumerable<DeliveryZoneViewModel> DeliveryZones { get; set; } = new List<DeliveryZoneViewModel>();
    public List<PosMenuItemViewModel> Menu { get; set; } = new();
}

public class OrderItemViewModel
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemArName { get; set; }
    public string MenuItemEnName { get; set; }
    public decimal Quantity { get; set; }
    public decimal PriceForUnit { get; set; }
    public decimal Price { get; set; }
}

public class PosMenuItemViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public int SectionId { get; set; }
    public string SectionArName { get; set; }
    public string SectionEnName { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class OrderMoveViewModel
{
    public int Id { get; set; }
    public int OrderMenuItemId { get; set; }
    public int TargetTableId { get; set; }
}

public class ShiftDashboardViewModel
{
    public List<ShiftRowViewModel> Collected { get; set; } = new();
    public List<ShiftRowViewModel> PendingMine { get; set; } = new();
    public List<ShiftRowViewModel> PendingOthers { get; set; } = new();
    public ShiftRowViewModel CollectedFromOthers { get; set; } = new();
    public ShiftRowViewModel OnAccount { get; set; } = new();
    public ShiftRowViewModel Electronic { get; set; } = new();

    public decimal CollectedTotal => Collected.Sum(a => a.Amount);
    public int CollectedCount => Collected.Sum(a => a.Count);
    public decimal PendingMineTotal => PendingMine.Sum(a => a.Amount);
    public int PendingMineCount => PendingMine.Sum(a => a.Count);
    public decimal PendingOthersTotal => PendingOthers.Sum(a => a.Amount);
    public int PendingOthersCount => PendingOthers.Sum(a => a.Count);
    public decimal PendingTotal => PendingMineTotal + PendingOthersTotal;
    public int PendingCount => PendingMineCount + PendingOthersCount;
    public decimal MoneyInDrawer => CollectedTotal + CollectedFromOthers.Amount - Electronic.Amount;
    public int MoneyInDrawerCount => CollectedCount + CollectedFromOthers.Count - Electronic.Count;

    public ShiftCloseReportViewModel LastShiftReport { get; set; }
}

public class ShiftCloseReportViewModel
{
    public DateTime ClosedAt { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal CountedCash { get; set; }
    public decimal Variance { get; set; }
    public bool HadIncompleteOrders { get; set; }
}

public class IncompleteCashOrderViewModel
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public decimal TotalPrice { get; set; }
}

public class ShiftCloseConfirmViewModel
{
    public string Password { get; set; }
    public decimal CountedCash { get; set; }
    public List<IncompleteCashOrderViewModel> IncompleteOrders { get; set; } = new();
}

public class ShiftRowViewModel
{
    public OrderType Type { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class PendingOnlineOrderViewModel
{
    public int Id { get; set; }
    public bool IsPending { get; set; }
    public OnlineOrderStatus Status { get; set; }
    public string RejectionReason { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public int? TableId { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Tax { get; set; }
    public decimal Service { get; set; }
    public decimal DeliveryFee { get; set; }
    public string DeliveryZoneArName { get; set; }
    public string DeliveryZoneEnName { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(a => a.Price);
}
