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
    public List<CashierMenuItemViewModel> Menu { get; set; } = new();

    // shows the cashier again after a refused save: a fresh code and menu, and the cart priced from the current menu
    public void RefreshFrom(OrderViewModel fresh)
    {
        OrderCode = fresh?.OrderCode;
        TaxPercent = fresh?.TaxPercent ?? 0;
        DefaultService = fresh?.DefaultService ?? 0;
        Menu = fresh?.Menu ?? new();
        foreach (var item in Items.ToList())
        {
            var dish = Menu.FirstOrDefault(m => m.Id == item.MenuItemId);
            if (dish is null)
            {
                Items.Remove(item);
                continue;
            }

            item.MenuItemArName = dish.ArName;
            item.MenuItemEnName = dish.EnName;
            item.PriceForUnit = dish.Price;
            item.Price = dish.Price * item.Quantity;
        }
    }
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

public class CashierMenuItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public int SectionId { get; set; }
    public string SectionArName { get; set; }
    public string SectionEnName { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string ArIngredients { get; set; }
    public string EnIngredients { get; set; }

    // How many portions the current stock can make. Null when the item has no recipe, so nothing limits it.
    public int? Available { get; set; }
}

public class OrderMoveViewModel
{
    public int Id { get; set; }
    public int OrderMenuItemId { get; set; }
    public int TargetTableId { get; set; }
}
