using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

public class OrderViewModelTests
{
    private static OrderViewModel Fresh() => new()
    {
        OrderCode = "9",
        TaxPercent = 14,
        DefaultService = 12,
        Menu = new List<CashierMenuItemViewModel>
        {
            new() { Id = 1, ArName = "Pizza", EnName = "Pizza", Price = 50 },
            new() { Id = 2, ArName = "Cola", EnName = "Cola", Price = 10 }
        }
    };

    [Fact]
    public void RefreshFrom_TakesTheFreshCodePercentsAndMenu()
    {
        var model = new OrderViewModel { OrderCode = "1", TaxPercent = 0 };

        model.RefreshFrom(Fresh());

        Assert.Equal("9", model.OrderCode);
        Assert.Equal(14, model.TaxPercent);
        Assert.Equal(12, model.DefaultService);
        Assert.Equal(2, model.Menu.Count);
    }

    [Fact]
    public void RefreshFrom_PricesTheCartFromTheCurrentMenu_AndDropsADishThatLeftTheMenu()
    {
        var model = new OrderViewModel
        {
            Items =
            {
                new OrderItemViewModel { MenuItemId = 1, Quantity = 3, Price = 1, PriceForUnit = 1 },
                new OrderItemViewModel { MenuItemId = 99, Quantity = 2 }
            }
        };

        model.RefreshFrom(Fresh());

        var item = Assert.Single(model.Items);
        Assert.Equal("Pizza", item.MenuItemEnName);
        Assert.Equal(50, item.PriceForUnit);
        Assert.Equal(150, item.Price);
        Assert.Equal(150, model.SubTotal);
    }

    [Fact]
    public void RefreshFrom_WithoutAFreshOrder_ClearsThePercentsAndTheCart()
    {
        var model = new OrderViewModel { TaxPercent = 14, Items = { new OrderItemViewModel { MenuItemId = 1, Quantity = 1 } } };

        model.RefreshFrom(null);

        Assert.Null(model.OrderCode);
        Assert.Equal(0, model.TaxPercent);
        Assert.Empty(model.Menu);
        Assert.Empty(model.Items);
    }
}
