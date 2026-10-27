using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// Units of a raw material (rate to the base unit and the unit price, which sets the cost),
// and the recipe line in a unit.
public class UnitsAndPriceTypesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TestHost _host = new();
    private readonly ProductRepository _products;
    private readonly MenuItemRepository _menu;
    private readonly StockRepository _stock;

    private static readonly MapperConfiguration MapperConfig =
        new(cfg => cfg.AddProfile<Restaurant.BLL.Helper.AutoMapper>(), NullLoggerFactory.Instance);

    public UnitsAndPriceTypesTests()
    {
        _products = new ProductRepository(_db.Context, _db.Http);
        _menu = new MenuItemRepository(_db.Context, _db.Http);
        _stock = new StockRepository(_db.Context);
    }

    public void Dispose()
    {
        _host.Dispose();
        _db.Dispose();
    }

    private async Task<(UnitPrice gram, UnitPrice kilo)> SeedUnits()
    {
        var gram = new UnitPrice { Code = "1", ArName = "Gram", EnName = "Gram" };
        var kilo = new UnitPrice { Code = "2", ArName = "Kilo", EnName = "Kilo" };
        _db.Context.AddRange(gram, kilo);
        await _db.Context.SaveChangesAsync();
        return (gram, kilo);
    }

    private static Product Flour(params ProductUnit[] units) => new() { ArName = "Flour", EnName = "Flour", Cost = 0.01m, Units = units.ToList() };



    // ---------------------------------------------------------------- unit prices of a raw material





    [Fact]
    public async Task Product_PriceIsKeptPerUnit_AndTheCostIsThePriceOfOneBaseUnit()
    {
        var (gram, kilo) = await SeedUnits();
        var product = Flour(
            new ProductUnit { UnitPriceId = gram.Id, Price = 0.012m },
            new ProductUnit { UnitPriceId = kilo.Id, TransRate = 1000, Price = 12 });
        await _products.UpsertProductAsync(product);

        _db.Context.ChangeTracker.Clear();
        var saved = await _products.GetProductByIdAsync(product.Id);
        Assert.Equal(0.012m, saved.Units.Single(u => u.UnitPriceId == gram.Id).Price);
        Assert.Equal(12, saved.Units.Single(u => u.UnitPriceId == kilo.Id).Price);
        Assert.Equal(0.012m, saved.Cost);
    }

    [Fact]
    public async Task Product_PricedOnlyInTheBigUnit_CostsThatPriceDividedByItsRate()
    {
        var (gram, kilo) = await SeedUnits();
        var product = Flour(new ProductUnit { UnitPriceId = gram.Id }, new ProductUnit { UnitPriceId = kilo.Id, TransRate = 1000, Price = 30 });

        await _products.UpsertProductAsync(product);

        _db.Context.ChangeTracker.Clear();
        Assert.Equal(0.03m, (await _products.GetProductByIdAsync(product.Id)).Cost);
    }

    [Fact]
    public async Task Product_EditWithoutAnyPrice_KeepsTheCostItHad()
    {
        var (gram, _) = await SeedUnits();
        var product = Flour(new ProductUnit { UnitPriceId = gram.Id, Price = 0.5m });
        await _products.UpsertProductAsync(product);
        _db.Context.ChangeTracker.Clear();

        var edit = Flour(new ProductUnit { UnitPriceId = gram.Id });
        edit.Id = product.Id;
        Assert.True((await _products.UpsertProductAsync(edit)).Success);

        _db.Context.ChangeTracker.Clear();
        Assert.Equal(0.5m, (await _products.GetProductByIdAsync(product.Id)).Cost);
    }



    [Fact]
    public async Task Product_SavingUnits_KeepsOnlyTheChosenDefault_EvenWithStockRows()
    {
        var a = new Store { Code = "1", ArName = "A", EnName = "A" };
        _db.Context.Add(a);
        var (gram, kilo) = await SeedUnits();
        var product = Flour();
        await _products.UpsertProductAsync(product);
        await _stock.MoveAsync(a.Id, product.Id, 5, StockMovementSource.OpeningBalance, product.Code);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var edit = Flour(new ProductUnit { UnitPriceId = kilo.Id, TransRate = 1000, IsDefault = true }, new ProductUnit { UnitPriceId = gram.Id });
        edit.Id = product.Id;
        await _products.UpsertProductAsync(edit);

        var defaults = await _db.Context.ItemUnits.AsNoTracking().Include(u => u.Unit).Where(u => u.IsDefault).ToListAsync();
        Assert.Equal("RST-U" + kilo.Id, defaults.Single().Unit.Code);
    }

    // ---------------------------------------------------------------- units next to the product in the cashier/invoice lookups

    [Fact]
    public async Task StoreProductLookup_CarriesTheUnitsOfEveryProduct_WithRateAndDefault()
    {
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        _db.Context.Add(store);
        var (gram, kilo) = await SeedUnits();
        var withUnits = Flour(new ProductUnit { UnitPriceId = gram.Id, IsDefault = true }, new ProductUnit { UnitPriceId = kilo.Id, TransRate = 1000 });
        var without = new Product { ArName = "Salt", EnName = "Salt", Cost = 1 };
        await _products.UpsertProductAsync(withUnits);
        await _products.UpsertProductAsync(without);
        _db.Context.ChangeTracker.Clear();

        var lookup = (await new LookupRepository(_db.Context).StoreProductLookupByStoreIdAsync(store.Id)).ToList();

        var flourUnits = lookup.Single(a => a.ProductId == withUnits.Id).Units;
        Assert.Equal(2, flourUnits.Count);
        Assert.Equal(1000, flourUnits.Single(u => u.UnitPriceId == kilo.Id).TransRate);
        Assert.True(flourUnits.Single(u => u.UnitPriceId == gram.Id).IsDefault);
        Assert.Empty(lookup.Single(a => a.ProductId == without.Id).Units);
    }

    // ---------------------------------------------------------------- recipe lines in a unit

    private async Task<(MenuSection section, Store store, Product flour, UnitPrice gram, UnitPrice kilo)> SeedRecipe()
    {
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        _db.Context.AddRange(section, store);
        var (gram, kilo) = await SeedUnits();
        var flour = Flour(new ProductUnit { UnitPriceId = gram.Id, Price = 0.01m }, new ProductUnit { UnitPriceId = kilo.Id, TransRate = 1000 });
        await _products.UpsertProductAsync(flour);
        _db.Context.ChangeTracker.Clear();
        return (section, store, flour, gram, kilo);
    }

    private static MenuItem Dish(MenuSection section, Store store, string name, params MenuItemComponent[] recipe) => new()
    {
        ArName = name, EnName = name, Price = 40, MenuSectionId = section.Id, StoreId = store.Id, Components = recipe.ToList()
    };

    [Fact]
    public async Task Recipe_TheSameProductInTwoUnits_IsTwoLines_TheSameUnitTwice_IsMerged()
    {
        var (section, store, flour, gram, kilo) = await SeedRecipe();
        var dish = Dish(section, store, "Bread",
            new MenuItemComponent { ProductId = flour.Id, UnitPriceId = gram.Id, Quantity = 100 },
            new MenuItemComponent { ProductId = flour.Id, UnitPriceId = gram.Id, Quantity = 50 },
            new MenuItemComponent { ProductId = flour.Id, UnitPriceId = kilo.Id, Quantity = 0.1m });

        var result = await _menu.UpsertMenuItemAsync(dish, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _menu.GetMenuItemByIdAsync(dish.Id);
        Assert.Equal(2, saved.Components.Count);
        Assert.Equal(150, saved.Components.Single(c => c.UnitPriceId == gram.Id).Quantity);
        Assert.Equal(1000, saved.Components.Single(c => c.UnitPriceId == kilo.Id).TransRate);
        // 150 g + 0.1 kg = 250 base units at 0.01 each
        Assert.Equal(2.5m, saved.TotalCost);
    }

    [Fact]
    public async Task Recipe_TheRateComesFromTheProduct_NotFromWhatTheBrowserSent()
    {
        var (section, store, flour, _, kilo) = await SeedRecipe();
        var dish = Dish(section, store, "Bread", new MenuItemComponent { ProductId = flour.Id, UnitPriceId = kilo.Id, Quantity = 1, TransRate = 1 });

        Assert.True((await _menu.UpsertMenuItemAsync(dish, null)).Success);

        _db.Context.ChangeTracker.Clear();
        Assert.Equal(1000, (await _menu.GetMenuItemByIdAsync(dish.Id)).Components.Single().TransRate);
    }

    [Fact]
    public async Task Recipe_WithoutAUnit_IsInTheBaseUnit()
    {
        var (section, store, flour, _, _) = await SeedRecipe();
        var dish = Dish(section, store, "Bread", new MenuItemComponent { ProductId = flour.Id, Quantity = 100 });

        Assert.True((await _menu.UpsertMenuItemAsync(dish, null)).Success);

        _db.Context.ChangeTracker.Clear();
        var line = (await _menu.GetMenuItemByIdAsync(dish.Id)).Components.Single();
        Assert.Null(line.UnitPriceId);
        Assert.Equal(1, line.TransRate);
        Assert.Equal(100, line.QuantityInBase);
    }

    [Fact]
    public async Task Recipe_EditingTheDish_ReplacesTheLinesWithTheirUnits()
    {
        var (section, store, flour, gram, kilo) = await SeedRecipe();
        var dish = Dish(section, store, "Bread", new MenuItemComponent { ProductId = flour.Id, UnitPriceId = gram.Id, Quantity = 100 });
        await _menu.UpsertMenuItemAsync(dish, null);
        _db.Context.ChangeTracker.Clear();

        var edit = Dish(section, store, "Bread", new MenuItemComponent { ProductId = flour.Id, UnitPriceId = kilo.Id, Quantity = 1 });
        edit.Id = dish.Id;
        Assert.True((await _menu.UpsertMenuItemAsync(edit, null)).Success);

        _db.Context.ChangeTracker.Clear();
        var line = (await _menu.GetMenuItemByIdAsync(dish.Id)).Components.Single();
        Assert.Equal(kilo.Id, line.UnitPriceId);
        Assert.Equal(1000, line.QuantityInBase);
    }

    // ---------------------------------------------------------------- models

    [Theory]
    [InlineData(100, 1, 100)]
    [InlineData(0.25, 1000, 250)]
    [InlineData(2, 0.001, 0.002)]
    public void MenuItemComponent_QuantityInBase_IsQuantityTimesRate(double quantity, double rate, double expected)
    {
        var line = new MenuItemComponent { Quantity = (decimal)quantity, TransRate = (decimal)rate };

        Assert.Equal((decimal)expected, line.QuantityInBase);
    }

    [Fact]
    public void MenuItemComponent_ANewLine_IsInTheBaseUnit()
    {
        Assert.Equal(1, new MenuItemComponent().TransRate);
    }

    [Fact]
    public void MenuItem_TotalCostAndBenefit_UseTheBaseQuantityOfEveryLine()
    {
        var flour = new Product { Cost = 0.01m };
        var dish = new MenuItem
        {
            Price = 5,
            Components = new List<MenuItemComponent>
            {
                new() { Product = flour, Quantity = 100, TransRate = 1 },
                new() { Product = flour, Quantity = 0.5m, TransRate = 1000 }
            }
        };

        Assert.Equal(1 + 5, dish.TotalCost);
        Assert.Equal(-1, dish.Benefit);
    }

    [Fact]
    public void ProductUnit_StartsInTheBaseUnit_WithNoPrice()
    {
        var unit = new ProductUnit();

        Assert.Equal(1, unit.TransRate);
        Assert.False(unit.IsDefault);
        Assert.Equal(0, unit.Price);
    }

    // ---------------------------------------------------------------- mapping

    [Fact]
    public void Map_RecipeLine_ShowsUnitNamesAndCostByTheRate()
    {
        var mapper = MapperConfig.CreateMapper();
        var line = new MenuItemComponent
        {
            Product = new Product { ArName = "دقيق", EnName = "Flour", Cost = 0.01m },
            UnitPrice = new UnitPrice { ArName = "كيلو", EnName = "Kilo" },
            Quantity = 0.5m,
            TransRate = 1000
        };

        var model = mapper.Map<MenuItemComponentViewModel>(line);

        Assert.Equal("Kilo", model.UnitEnName);
        Assert.Equal(10, model.UnitCost);
        Assert.Equal(5, model.Cost);
    }

    [Fact]
    public void Map_RecipeLineFromTheForm_DoesNotTrustAPostedRate()
    {
        var mapper = MapperConfig.CreateMapper();

        var line = mapper.Map<MenuItemComponent>(new MenuItemComponentViewModel { ProductId = 1, UnitPriceId = 2, Quantity = 3, TransRate = 999 });

        Assert.Equal(2, line.UnitPriceId);
        Assert.Equal(1, line.TransRate);
    }

    [Fact]
    public void Map_ProductUnitWithItsPrice_RoundTripsBothWays()
    {
        var mapper = MapperConfig.CreateMapper();
        var unit = new ProductUnit { UnitPriceId = 4, UnitPrice = new UnitPrice { ArName = "كيلو", EnName = "Kilo" }, TransRate = 1000, Price = 12 };

        var model = mapper.Map<ProductUnitViewModel>(unit);
        var back = mapper.Map<ProductUnit>(model);

        Assert.Equal("Kilo", model.UnitEnName);
        Assert.Equal(12, model.Price);
        Assert.Equal(1000, back.TransRate);
        Assert.Equal(12, back.Price);
    }

    // ---------------------------------------------------------------- validator

    private async Task<Dictionary<string, List<string>>> Errors(ProductViewModel model)
    {
        var validator = (IValidator<ProductViewModel>)_host.Get(typeof(IValidator<ProductViewModel>));
        var result = await validator.ValidateAsync(model);

        return result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }

    private static ProductViewModel Form(params ProductUnitViewModel[] units) => new()
    {
        ArName = "دقيق جديد", EnName = "New flour", ProductCategoryId = 1, Cost = 1, Units = units.ToList()
    };

    [Fact]
    public async Task ProductForm_UnitsAreOptional_AValidSetPasses()
    {
        Assert.DoesNotContain(await Errors(Form()), e => e.Key.StartsWith("Units"));
        Assert.DoesNotContain(await Errors(Form(
            new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1, IsDefault = true },
            new ProductUnitViewModel { UnitPriceId = 2, TransRate = 1000 })), e => e.Key.StartsWith("Units"));
    }

    [Fact]
    public async Task ProductForm_ARepeatedUnit_IsRefused()
    {
        var errors = await Errors(Form(new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1, IsDefault = true }, new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1 }));

        Assert.Contains("Val.UnitRepeated", errors["Units"]);
    }

    [Fact]
    public async Task ProductForm_WithoutABaseUnitOfRateOne_IsRefused()
    {
        var errors = await Errors(Form(new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1000, IsDefault = true }));

        Assert.Contains("Val.UnitNeedsBase", errors["Units"]);
    }


    [Fact]
    public async Task ProductForm_AUnitNeedsItsListEntryAndARateAboveZero()
    {
        var errors = await Errors(Form(new ProductUnitViewModel { UnitPriceId = 0, TransRate = 0, IsDefault = true }, new ProductUnitViewModel { UnitPriceId = 2, TransRate = -1 }));

        Assert.Contains("Val.RequiredField", errors["Units[0].UnitPriceId"]);
        Assert.Contains("Val.GreaterThanZero", errors["Units[0].TransRate"]);
        Assert.Contains("Val.GreaterThanZero", errors["Units[1].TransRate"]);
    }

    [Fact]
    public async Task ProductForm_APriceCannotBeNegative()
    {
        var errors = await Errors(Form(new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1, Price = -1 }));

        Assert.Contains("Val.NotNegative", errors["Units[0].Price"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task ProductForm_NeedsExactlyOneDefaultUnit(int defaults)
    {
        var errors = await Errors(Form(
            new ProductUnitViewModel { UnitPriceId = 1, TransRate = 1, IsDefault = defaults > 0 },
            new ProductUnitViewModel { UnitPriceId = 2, TransRate = 1000, IsDefault = defaults > 1 }));

        Assert.Contains("Val.UnitNeedsOneDefault", errors["Units"]);
    }



    // ---------------------------------------------------------------- precision (what the database column can hold)

    [Theory]
    [InlineData(typeof(BillDetail), nameof(BillDetail.Quantity))]
    [InlineData(typeof(BillDetail), nameof(BillDetail.UnitPrice))]
    [InlineData(typeof(BillDetail), nameof(BillDetail.TransRate))]
    [InlineData(typeof(Product), nameof(Product.Cost))]
    [InlineData(typeof(ItemUnit), nameof(ItemUnit.Whole))]
    [InlineData(typeof(ItemUnit), nameof(ItemUnit.HalfWhole))]
    [InlineData(typeof(ItemUnit), nameof(ItemUnit.LastPurchase))]
    [InlineData(typeof(MenuItemComponent), nameof(MenuItemComponent.TransRate))]
    public void FractionalUnitsNeedSixDecimals_SoARateOrPriceOfOneGramIsNotRoundedAway(Type entity, string property)
    {
        var column = _db.Context.Model.FindEntityType(entity).FindProperty(property);

        Assert.Equal(6, column.GetScale());
    }

    [Fact]
    public void TheCodeOnlyMapsThroughFluentConfiguration_NoColumnAttributesOnTheNewModels()
    {
        foreach (var type in new[] { typeof(ProductUnit), typeof(ItemUnit), typeof(MenuItemComponent) })
        {
            Assert.DoesNotContain(type.GetProperties(), p => p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), true).Any()
                || p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), true).Any());
        }
    }
}
