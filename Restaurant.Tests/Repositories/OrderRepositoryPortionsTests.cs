using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.Tests.Repositories;

// Tests for the "portions left" number shown on every item of the cashier screen.
public class OrderRepositoryPortionsTests
{
    private const int MainStore = 1;
    private const int OtherStore = 2;

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    // Puts "quantity" of a product into a store, the same way the app stores it (an item with a product code, in the base unit).
    private static async Task AddStockAsync(ApplicationDbContext context, int storeId, int productId, decimal quantity)
    {
        var code = BillSupportRepository.ItemCodePrefix + productId;

        var item = await context.Items.FirstOrDefaultAsync(a => a.Code == code);
        if (item == null)
        {
            item = new Item { Code = code, ArName = "p" + productId, EnName = "p" + productId };
            context.Items.Add(item);
        }

        var unit = await context.Units.FirstOrDefaultAsync(a => a.Code == StockRepository.BaseUnitCode);
        if (unit == null)
        {
            unit = new Unit { Code = StockRepository.BaseUnitCode, ArName = "unit", EnName = "unit" };
            context.Units.Add(unit);
        }

        context.StoreItemUnits.Add(new StoreItemUnit { StoreId = storeId, Item = item, Unit = unit, Quantity = quantity });
        await context.SaveChangesAsync();
    }

    // A menu item that needs the given amount of each product, as listed in "recipe" (productId, quantity).
    private static async Task<MenuItem> AddMenuItemAsync(ApplicationDbContext context, int storeId, params (int ProductId, decimal Quantity)[] recipe)
    {
        var menuItem = new MenuItem { ArName = "dish ar", EnName = "dish", MenuSectionId = 1, StoreId = storeId, Price = 50, Components = new List<MenuItemComponent>() };
        foreach (var line in recipe)
        {
            menuItem.Components.Add(new MenuItemComponent { ProductId = line.ProductId, Quantity = line.Quantity });
        }

        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        return menuItem;
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_SingleIngredient_StockDividedByRecipeQuantity()
    {
        // Arrange: 100 units in stock, one portion needs 10
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, productId: 1, quantity: 100);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(10, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_RoundsDown()
    {
        // Arrange: 25 in stock, 10 per portion: only 2 full portions
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 25);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        var portions = await repository.GetAvailablePortionsAsync();

        Assert.Equal(2, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_TheIngredientThatRunsOutFirstDecides()
    {
        // Arrange: product 1 would give 10 portions, product 2 only 3
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 100);
        await AddStockAsync(context, MainStore, 2, 9);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10), (2, 3));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(3, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_IngredientWithNoStockRow_MeansNoPortions()
    {
        // Arrange: product 2 was never stocked
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 100);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10), (2, 3));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(0, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_StockInAnotherStore_DoesNotCount()
    {
        // Arrange: the dish is made from the main store, the stock sits in the other one
        using var context = CreateContext();
        await AddStockAsync(context, OtherStore, 1, 100);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(0, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_SameProductInTwoStores_EachDishUsesItsOwnStore()
    {
        // Arrange
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 100);
        await AddStockAsync(context, OtherStore, 1, 20);
        var mainDish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var otherDish = await AddMenuItemAsync(context, OtherStore, (1, 10));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(10, portions[mainDish.Id]);
        Assert.Equal(2, portions[otherDish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_EmptyStock_IsZero()
    {
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 0);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        var portions = await repository.GetAvailablePortionsAsync();

        Assert.Equal(0, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_MenuItemWithoutRecipe_IsLeftOutSoNothingLimitsIt()
    {
        // Arrange: a bottle of water has no recipe
        using var context = CreateContext();
        var dish = await AddMenuItemAsync(context, MainStore);
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.False(portions.ContainsKey(dish.Id));
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_ZeroQuantityLineInRecipe_IsIgnored()
    {
        // Arrange: product 1 is listed with quantity 0 (it uses nothing), product 2 limits the dish
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 2, 20);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 0), (2, 5));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(4, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_RecipeThatUsesNothing_IsUnlimited()
    {
        using var context = CreateContext();
        var dish = await AddMenuItemAsync(context, MainStore, (1, 0));
        var repository = new OrderRepository(context);

        var portions = await repository.GetAvailablePortionsAsync();

        Assert.Equal(int.MaxValue, portions[dish.Id]);
    }

    [Fact]
    public async Task GetAvailablePortionsAsync_SeveralDishes_EachGetsItsOwnNumber()
    {
        // Arrange
        using var context = CreateContext();
        await AddStockAsync(context, MainStore, 1, 60);
        await AddStockAsync(context, MainStore, 2, 6);
        var burger = await AddMenuItemAsync(context, MainStore, (1, 1));
        var steak = await AddMenuItemAsync(context, MainStore, (2, 2));
        var repository = new OrderRepository(context);

        // Act
        var portions = await repository.GetAvailablePortionsAsync();

        // Assert
        Assert.Equal(60, portions[burger.Id]);
        Assert.Equal(3, portions[steak.Id]);
    }

    [Fact]
    public async Task GetCashierMenuAsync_FillsInThePortionsLeft()
    {
        using var context = CreateContext();
        context.MenuSections.Add(new MenuSection { Id = 1, ArName = "section", EnName = "section" });
        await context.SaveChangesAsync();
        await AddStockAsync(context, MainStore, 1, 100);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        var menu = (await repository.GetCashierMenuAsync()).ToList();

        Assert.Equal(10, menu.Single(a => a.Id == dish.Id).AvailablePortions);
        Assert.True(menu.Single(a => a.Id == dish.Id).IsAvailable);
    }

    [Fact]
    public async Task GetCashierMenuAsync_NoPortionsLeft_MarksTheDishNotAvailable()
    {
        using var context = CreateContext();
        context.MenuSections.Add(new MenuSection { Id = 1, ArName = "section", EnName = "section" });
        await context.SaveChangesAsync();
        await AddStockAsync(context, MainStore, 1, 5);
        var dish = await AddMenuItemAsync(context, MainStore, (1, 10));
        var repository = new OrderRepository(context);

        var menu = (await repository.GetCashierMenuAsync()).ToList();

        Assert.Equal(0, menu.Single(a => a.Id == dish.Id).AvailablePortions);
        Assert.False(menu.Single(a => a.Id == dish.Id).IsAvailable);
    }

    [Fact]
    public async Task GetCashierMenuAsync_DishWithoutRecipe_HasNoLimitAndStaysAvailable()
    {
        using var context = CreateContext();
        context.MenuSections.Add(new MenuSection { Id = 1, ArName = "section", EnName = "section" });
        await context.SaveChangesAsync();
        var dish = await AddMenuItemAsync(context, MainStore);
        var repository = new OrderRepository(context);

        var menu = (await repository.GetCashierMenuAsync()).ToList();

        Assert.Null(menu.Single(a => a.Id == dish.Id).AvailablePortions);
        Assert.True(menu.Single(a => a.Id == dish.Id).IsAvailable);
    }
}
