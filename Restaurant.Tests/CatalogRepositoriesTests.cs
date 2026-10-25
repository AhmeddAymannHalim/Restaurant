using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Products (with the stock they hold in every store), menu items (dishes with a recipe) and the shift password / settings.
public class CatalogRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly ProductRepository _products;
    private readonly MenuItemRepository _menu;
    private readonly StockRepository _stock;

    public CatalogRepositoriesTests()
    {
        _products = new ProductRepository(_db.Context, _db.Http);
        _menu = new MenuItemRepository(_db.Context, _db.Http);
        _stock = new StockRepository(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(Store a, Store b)> SeedStores()
    {
        var a = new Store { Code = "1", ArName = "A", EnName = "A" };
        var b = new Store { Code = "2", ArName = "B", EnName = "B" };
        _db.Context.AddRange(a, b);
        await _db.Context.SaveChangesAsync();
        return (a, b);
    }

    private static Product NewProduct(string name = "Flour") => new() { ArName = name, EnName = name, Cost = 3 };

    // ---------------------------------------------------------------- products

    [Fact]
    public async Task Product_Create_GetsCodeAndTheStockPerStore()
    {
        var (a, b) = await SeedStores();
        var product = NewProduct();

        var result = await _products.UpsertProductAsync(product, new[] { new ProductStock { StoreId = a.Id, Quantity = 10 }, new ProductStock { StoreId = b.Id, Quantity = 4 } });

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _products.GetProductByIdAsync(product.Id);
        Assert.Equal("1", saved.Code);
        Assert.Equal(10, saved.Stocks.Single(s => s.StoreId == a.Id).Quantity);
        Assert.Equal(4, saved.Stocks.Single(s => s.StoreId == b.Id).Quantity);
    }

    [Fact]
    public async Task Product_Edit_ChangesStockAndZeroesAStoreTakenOffTheList()
    {
        var (a, b) = await SeedStores();
        var product = NewProduct();
        await _products.UpsertProductAsync(product, new[] { new ProductStock { StoreId = a.Id, Quantity = 10 }, new ProductStock { StoreId = b.Id, Quantity = 4 } });
        _db.Context.ChangeTracker.Clear();

        var edit = NewProduct("Flour 2");
        edit.Id = product.Id;
        var result = await _products.UpsertProductAsync(edit, new[] { new ProductStock { StoreId = a.Id, Quantity = 7 } });

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _products.GetProductByIdAsync(product.Id);
        Assert.Equal("Flour 2", saved.EnName);
        Assert.Equal("1", saved.Code);
        Assert.Equal(7, saved.Stocks.Single(s => s.StoreId == a.Id).Quantity);
        Assert.Equal(0, saved.Stocks.Single(s => s.StoreId == b.Id).Quantity);
    }

    [Fact]
    public async Task Product_Edit_Unknown_ReturnsRecordNotFound()
    {
        var edit = NewProduct();
        edit.Id = 55;

        Assert.Equal("RecordNotFound", (await _products.UpsertProductAsync(edit, new List<ProductStock>())).Message);
    }

    [Fact]
    public async Task Product_Delete_BlockedWhileItHoldsStock_AllowedAtZero_AndLinkedToADishIsBlocked()
    {
        var (a, _) = await SeedStores();
        var withStock = NewProduct("Stocked");
        await _products.UpsertProductAsync(withStock, new[] { new ProductStock { StoreId = a.Id, Quantity = 5 } });
        var empty = NewProduct("Empty");
        await _products.UpsertProductAsync(empty, new List<ProductStock>());
        _db.Context.ChangeTracker.Clear();

        Assert.Equal("CannotDeleteHasStock", (await _products.DeleteProductAsync(withStock.Id)).Message);
        Assert.True((await _products.DeleteProductAsync(empty.Id)).Success);
        Assert.True((await _products.DeleteProductAsync(404)).Success);
        Assert.Single(_db.Context.Products);
    }

    [Fact]
    public async Task Product_UsedInARecipe_CannotBeDeleted()
    {
        var (a, _) = await SeedStores();
        var product = NewProduct();
        await _products.UpsertProductAsync(product, new List<ProductStock>());
        var section = new MenuSection { Code = "1", ArName = "S", EnName = "S" };
        _db.Context.MenuItems.Add(new MenuItem
        {
            Code = "1", ArName = "Dish", EnName = "Dish", Price = 10, StoreId = a.Id, MenuSection = section,
            Components = new List<MenuItemComponent> { new() { ProductId = product.Id, Quantity = 1 } }
        });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Equal("CannotDeleteLinkedRecord", (await _products.DeleteProductAsync(product.Id)).Message);
    }

    [Fact]
    public async Task Product_PagingShowsStockOfEveryStore_AndSearches()
    {
        var (a, b) = await SeedStores();
        await _products.UpsertProductAsync(NewProduct("Flour"), new[] { new ProductStock { StoreId = a.Id, Quantity = 3 } });
        await _products.UpsertProductAsync(NewProduct("Sugar"), new List<ProductStock>());

        var all = await _products.GetPagedProductsAsync(new PageRequest());
        var search = await _products.GetPagedProductsAsync(new PageRequest { SearchString = "sug" });

        Assert.Equal(2, all.TotalRecords);
        Assert.Equal(2, all.Data[0].Stocks.Count);
        Assert.Equal(3, all.Data[0].Stocks.Single(s => s.StoreId == a.Id).Quantity);
        Assert.Equal(0, all.Data[1].Stocks.Single(s => s.StoreId == b.Id).Quantity);
        Assert.Single(search.Data);
        Assert.Equal("3", await _products.NewProductCodeAsync());
    }

    // ---------------------------------------------------------------- product units

    private async Task<(UnitPrice gram, UnitPrice kilo)> SeedUnits()
    {
        var gram = new UnitPrice { Code = "1", ArName = "جرام", EnName = "Gram" };
        var kilo = new UnitPrice { Code = "2", ArName = "كيلو", EnName = "Kilo" };
        _db.Context.AddRange(gram, kilo);
        await _db.Context.SaveChangesAsync();
        return (gram, kilo);
    }

    [Fact]
    public async Task Product_Units_AreSavedWithRateBarcodeAndDefault_AndAStoreKeepsItsReorderLimit()
    {
        var (a, _) = await SeedStores();
        var (gram, kilo) = await SeedUnits();
        var priceTypes = Enumerable.Range(1, 6).Select(n => new SellType { ArName = "T" + n, EnName = "T" + n, Show = true }).ToList();
        _db.Context.AddRange(priceTypes);
        await _db.Context.SaveChangesAsync();
        var product = NewProduct();
        product.Units = new List<ProductUnit>
        {
            new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true, Barcode = "111" },
            new() { UnitPriceId = kilo.Id, TransRate = 1000, Barcode = "222", LastPurchase = 400, Prices = priceTypes.Select((t, n) => new ProductUnitPrice { SellTypeId = t.Id, Price = 10 * (n + 1) + 0.5m }).ToList() }
        };

        var result = await _products.UpsertProductAsync(product, new[] { new ProductStock { StoreId = a.Id, Quantity = 5, MinQuantity = 2 } });

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _products.GetProductByIdAsync(product.Id);
        Assert.Equal(2, saved.Units.Count);
        Assert.Equal(1000, saved.Units.Single(u => u.UnitPriceId == kilo.Id).TransRate);
        Assert.Equal("222", saved.Units.Single(u => u.UnitPriceId == kilo.Id).Barcode);
        var kiloPrices = saved.Units.Single(u => u.UnitPriceId == kilo.Id);
        Assert.Equal(400, kiloPrices.LastPurchase);
        // price types are a definition, so there is no fixed number of them (here 8)
        Assert.Equal(6, kiloPrices.Prices.Count);
        Assert.Equal(new[] { 10.5m, 20.5m, 30.5m, 40.5m, 50.5m, 60.5m }, kiloPrices.Prices.OrderBy(a => a.SellTypeId).Select(a => a.Price).ToArray());
        Assert.True(saved.Units.Single(u => u.UnitPriceId == gram.Id).IsDefault);
        Assert.Equal(2, saved.Stocks.Single(s => s.StoreId == a.Id).MinQuantity);
        Assert.Equal(5, saved.Stocks.Single(s => s.StoreId == a.Id).Quantity);
        // the stock unit that is created with the first stock row must not be a second default
        Assert.Single(await _db.Context.ItemUnits.AsNoTracking().Where(u => u.IsDefault).ToListAsync());
    }

    [Fact]
    public async Task Product_Units_AUnitTakenOffTheListIsRemoved()
    {
        var (gram, kilo) = await SeedUnits();
        var product = NewProduct();
        product.Units = new List<ProductUnit> { new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true }, new() { UnitPriceId = kilo.Id, TransRate = 1000 } };
        await _products.UpsertProductAsync(product, new List<ProductStock>());
        _db.Context.ChangeTracker.Clear();

        var edit = NewProduct();
        edit.Id = product.Id;
        edit.Units = new List<ProductUnit> { new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true } };
        Assert.True((await _products.UpsertProductAsync(edit, new List<ProductStock>())).Success);

        _db.Context.ChangeTracker.Clear();
        var saved = await _products.GetProductByIdAsync(product.Id);
        Assert.Equal(gram.Id, saved.Units.Single().UnitPriceId);
    }

    [Fact]
    public async Task Menu_Recipe_InAKiloOfAGramProduct_CostsByTheRate()
    {
        var (section, store, flour, _) = await SeedMenu();
        var (gram, kilo) = await SeedUnits();
        flour.Cost = 0.01m; // per gram
        flour.Units = new List<ProductUnit> { new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true }, new() { UnitPriceId = kilo.Id, TransRate = 1000 } };
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        await _products.UpsertProductAsync(new Product { Id = flour.Id, ArName = "Flour", EnName = "Flour", Cost = 0.01m, Units = flour.Units }, new List<ProductStock>());
        _db.Context.ChangeTracker.Clear();

        var dish = Dish(section, store, "Bread", (flour.Id, 0.25m));
        dish.Components.Single().UnitPriceId = kilo.Id;
        var result = await _menu.UpsertMenuItemAsync(dish, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _menu.GetMenuItemByIdAsync(dish.Id);
        Assert.Equal(1000, saved.Components.Single().TransRate);
        Assert.Equal(250, saved.Components.Single().QuantityInBase);
        Assert.Equal(2.5m, saved.TotalCost);
    }

    [Fact]
    public async Task Order_OfADishWithAKiloRecipe_TakesBaseUnitsOutOfTheStock()
    {
        var (section, store, flour, _) = await SeedMenu();
        var (_, kilo) = await SeedUnits();
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        _db.Context.Add(user);
        var dish = Dish(section, store, "Bread", (flour.Id, 0.1m));
        dish.Components.Single().UnitPriceId = kilo.Id;
        dish.Components.Single().TransRate = 1000;
        _db.Context.MenuItems.Add(dish);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        await _stock.AdjustAsync(store.Id, flour.Id, 5000, allowNegative: true);
        _db.Context.ChangeTracker.Clear();
        var orders = new OrderRepository(_db.Context);

        var menuBefore = (await orders.GetCashierMenuAsync()).Single();
        var result = await orders.CreateOrderAsync(new Order { Type = OrderType.TakeAway, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 2 } } } }, user.Id);

        Assert.Equal(50, menuBefore.AvailablePortions);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(4800, (await _stock.FindAsync(store.Id, flour.Id)).Quantity);
    }

    [Fact]
    public async Task Menu_Recipe_AUnitTheProductDoesNotHaveIsRejected()
    {
        var (section, store, flour, _) = await SeedMenu();
        var (_, kilo) = await SeedUnits();

        var dish = Dish(section, store, "Bread", (flour.Id, 1));
        dish.Components.Single().UnitPriceId = kilo.Id;

        Assert.Equal("RecordNotFound", (await _menu.UpsertMenuItemAsync(dish, null)).Message);
    }

    // ---------------------------------------------------------------- menu items

    private async Task<(MenuSection section, Store store, Product flour, Product cheese)> SeedMenu()
    {
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var flour = new Product { Code = "1", ArName = "Flour", EnName = "Flour", Cost = 2 };
        var cheese = new Product { Code = "2", ArName = "Cheese", EnName = "Cheese", Cost = 5 };
        _db.Context.AddRange(section, store, flour, cheese);
        await _db.Context.SaveChangesAsync();
        return (section, store, flour, cheese);
    }

    private static MenuItem Dish(MenuSection section, Store store, string name, params (int product, decimal quantity)[] recipe) => new()
    {
        ArName = name, EnName = name, Price = 40, MenuSectionId = section.Id, StoreId = store.Id,
        Components = recipe.Select(a => new MenuItemComponent { ProductId = a.product, Quantity = a.quantity }).ToList()
    };

    [Fact]
    public async Task Menu_Create_MergesRepeatedIngredientsAndComputesCost()
    {
        var (section, store, flour, cheese) = await SeedMenu();
        var dish = Dish(section, store, "Pizza", (flour.Id, 2), (flour.Id, 1), (cheese.Id, 1), (cheese.Id, 0));

        var result = await _menu.UpsertMenuItemAsync(dish, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _menu.GetMenuItemByIdAsync(dish.Id);
        Assert.Equal("1", saved.Code);
        Assert.Equal(2, saved.Components.Count);
        Assert.Equal(3, saved.Components.Single(a => a.ProductId == flour.Id).Quantity);
        Assert.Equal(3 * 2 + 1 * 5, saved.TotalCost);
        Assert.Equal(40 - 11, saved.Benefit);
    }

    [Fact]
    public async Task Menu_Create_Rules()
    {
        var (section, store, flour, _) = await SeedMenu();

        Assert.Equal("MenuItemNeedsComponents", (await _menu.UpsertMenuItemAsync(Dish(section, store, "Empty"), null)).Message);
        Assert.Equal("MenuItemNeedsComponents", (await _menu.UpsertMenuItemAsync(Dish(section, store, "Zero", (flour.Id, 0)), null)).Message);
        Assert.Equal("RecordNotFound", (await _menu.UpsertMenuItemAsync(Dish(section, store, "Ghost", (999, 1)), null)).Message);

        Assert.True((await _menu.UpsertMenuItemAsync(Dish(section, store, "Pizza", (flour.Id, 1)), null)).Success);
        Assert.Equal("MenuItemSizeExists", (await _menu.UpsertMenuItemAsync(Dish(section, store, "Pizza", (flour.Id, 1)), null)).Message);

        var sameEnglishNameOnly = Dish(section, store, "Other", (flour.Id, 1));
        sameEnglishNameOnly.EnName = "Pizza";
        Assert.Equal("MenuItemSizeExists", (await _menu.UpsertMenuItemAsync(sameEnglishNameOnly, null)).Message);

        var large = Dish(section, store, "Pizza", (flour.Id, 1));
        large.Size = Size.Large;
        Assert.True((await _menu.UpsertMenuItemAsync(large, null)).Success);
    }

    [Fact]
    public async Task Menu_Edit_ReplacesTheRecipe_AndKeepsCode()
    {
        var (section, store, flour, cheese) = await SeedMenu();
        var dish = Dish(section, store, "Pizza", (flour.Id, 2));
        await _menu.UpsertMenuItemAsync(dish, null);
        _db.Context.ChangeTracker.Clear();

        var edit = Dish(section, store, "Pizza", (cheese.Id, 4));
        edit.Id = dish.Id;
        edit.Price = 55;
        var result = await _menu.UpsertMenuItemAsync(edit, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _menu.GetMenuItemByIdAsync(dish.Id);
        Assert.Equal("1", saved.Code);
        Assert.Equal(55, saved.Price);
        var component = Assert.Single(saved.Components);
        Assert.Equal(cheese.Id, component.ProductId);
        Assert.Single(_db.Context.MenuItemComponents);
    }

    [Fact]
    public async Task Menu_EditUnknown_ReturnsRecordNotFound_DeleteWorksAndPagingSearches()
    {
        var (section, store, flour, _) = await SeedMenu();
        var edit = Dish(section, store, "Ghost", (flour.Id, 1));
        edit.Id = 77;
        Assert.Equal("RecordNotFound", (await _menu.UpsertMenuItemAsync(edit, null)).Message);

        var dish = Dish(section, store, "Burger", (flour.Id, 1));
        await _menu.UpsertMenuItemAsync(dish, null);
        Assert.Single((await _menu.GetPagedMenuItemsAsync(new PageRequest { SearchString = "BURG" })).Data);
        Assert.Equal("2", await _menu.NewMenuItemCodeAsync());

        Assert.True((await _menu.DeleteMenuItemAsync(dish.Id)).Success);
        Assert.True((await _menu.DeleteMenuItemAsync(404)).Success);
        Assert.Empty(_db.Context.MenuItems);
    }

    // ---------------------------------------------------------------- shift password and settings

    [Fact]
    public void ShiftPassword_HashesAndVerifies_PlainOldValuesAreUpgraded()
    {
        var sut = new ShiftPasswordRepository();
        var hash = sut.Hash("1234");

        Assert.NotEqual("1234", hash);
        Assert.True(sut.Verify(hash, "1234", out var rehash));
        Assert.False(rehash);
        Assert.False(sut.Verify(hash, "9999", out _));
        Assert.False(sut.Verify(null, "1234", out _));
        Assert.False(sut.Verify(hash, "", out _));

        Assert.True(sut.Verify("1234", "1234", out var upgrade));
        Assert.True(upgrade);
        Assert.False(sut.Verify("1234", "0000", out upgrade));
        Assert.False(upgrade);
    }

    [Fact]
    public async Task Settings_BlankPasswordKeepsTheOldOne_NewPasswordIsHashed()
    {
        var sut = new SettingRepository(_db.Context);
        var hasher = new ShiftPasswordRepository();
        _db.Context.ApplicationSettings.Add(new ApplicationSetting { Password = hasher.Hash("1234"), OrderTax = 5 });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        var oldHash = (await sut.GetApplicationSettingAsync()).Password;

        var keep = await sut.GetApplicationSettingAsync();
        keep.Password = "";
        keep.OrderTax = 14;
        await sut.UpdateApplicationSettingAsync(keep);
        _db.Context.ChangeTracker.Clear();
        var afterKeep = await sut.GetApplicationSettingAsync();
        Assert.Equal(oldHash, afterKeep.Password);
        Assert.Equal(14, afterKeep.OrderTax);

        afterKeep.Password = "5678";
        await sut.UpdateApplicationSettingAsync(afterKeep);
        _db.Context.ChangeTracker.Clear();
        var changed = await sut.GetApplicationSettingAsync();
        Assert.NotEqual(oldHash, changed.Password);
        Assert.True(hasher.Verify(changed.Password, "5678", out _));
    }

    [Fact]
    public async Task PrinterSetting_SavesAndLoads()
    {
        var sut = new SettingRepository(_db.Context);
        _db.Context.PrinterSettings.Add(new PrinterSetting());
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var printer = await sut.GetPrinterSettingAsync();
        var result = await sut.UpdatePrinterSettingAsync(printer);

        Assert.True(result.Success);
        Assert.NotNull(await sut.GetPrinterSettingAsync());
    }
}
