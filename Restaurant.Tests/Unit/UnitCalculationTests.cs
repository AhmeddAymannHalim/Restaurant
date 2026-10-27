using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The unit-of-measure maths in the repositories: a kilo holds 1000 base units (grams), stock and cost are always kept per base unit,
// while prices and quantities on documents stay in the unit the user picked.
public class UnitCalculationTests : IDisposable
{
    private readonly TestDb _db = new(invoiceAccounts: true);
    private readonly InvoiceRepository _invoices;
    private readonly OrderRepository _orders;
    private readonly StockRepository _stock;

    private Store _store;
    private Product _sugar;
    private UnitPrice _gram;
    private UnitPrice _kilo;
    private UnitPrice _box;
    private Supplier _supplier;
    private Customer _customer;
    private MenuItem _dish;
    private string _user;

    public UnitCalculationTests()
    {
        _invoices = new InvoiceRepository(_db.Context, new JournalEntryRepository(_db.Context));
        _orders = new OrderRepository(_db.Context);
        _stock = new StockRepository(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    // Sugar costs 0.005 per gram, is sold in grams and kilos (1000), a box has no rate on the product.
    // The dish needs 0.25 kilo = 250 grams of sugar and sells for 50. 10% tax, 5 service.
    private async Task Seed(decimal sugarInStock = 1000)
    {
        var seller = new ApplicationUser { UserName = "cashier", Email = "c@test.com" };
        _store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        _sugar = new Product { Code = "1", ArName = "Sugar", EnName = "Sugar", Cost = 0.005m };
        _gram = new UnitPrice { Code = "1", ArName = "Gram", EnName = "Gram" };
        _kilo = new UnitPrice { Code = "2", ArName = "Kilo", EnName = "Kilo" };
        _box = new UnitPrice { Code = "3", ArName = "Box", EnName = "Box" };
        var section = new MenuSection { Code = "1", ArName = "Section", EnName = "Section" };
        _supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        _customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(seller, _store, _sugar, _gram, _kilo, _box, section, _supplier, _customer);
        _db.Context.ApplicationSettings.Add(new ApplicationSetting { Password = "x", OrderTax = 10, OrderService = 5 });
        await _db.Context.SaveChangesAsync();

        var products = new ProductRepository(_db.Context, _db.Http);
        var edit = new Product { Id = _sugar.Id, ArName = "Sugar", EnName = "Sugar", Cost = 0.005m };
        edit.Units = new List<ProductUnit> { new() { UnitPriceId = _gram.Id, TransRate = 1, IsDefault = true }, new() { UnitPriceId = _kilo.Id, TransRate = 1000 } };
        _db.Context.ChangeTracker.Clear();
        Assert.True((await products.UpsertProductAsync(edit)).Success);

        _dish = new MenuItem
        {
            Code = "1", ArName = "Cake", EnName = "Cake", Price = 50, StoreId = _store.Id, MenuSectionId = section.Id,
            Components = new List<MenuItemComponent> { new() { ProductId = _sugar.Id, Quantity = 0.25m, UnitPriceId = _kilo.Id, TransRate = 1000 } }
        };
        _db.Context.MenuItems.Add(_dish);
        await _db.Context.SaveChangesAsync();
        await _stock.AdjustAsync(_store.Id, _sugar.Id, sugarInStock);
        _user = seller.Id;
        _db.Context.ChangeTracker.Clear();
    }

    private async Task<decimal> Sugar()
    {
        _db.Context.ChangeTracker.Clear();
        return (await _stock.FindAsync(_store.Id, _sugar.Id))?.Quantity ?? 0;
    }

    private async Task<decimal> Cost()
    {
        _db.Context.ChangeTracker.Clear();
        return (await _db.Context.Products.AsNoTracking().SingleAsync(a => a.Id == _sugar.Id)).Cost;
    }

    private InvoiceViewModel Invoice(InvoiceType type, UnitPrice unit, decimal quantity, decimal price, decimal paid = 0, bool post = true)
    {
        var model = new InvoiceViewModel
        {
            InvoiceType = type,
            StoreId = _store.Id,
            Paid = paid,
            PayType = Shared.Enums.PayType.Later,
            AutoStock = post,
            AutoPost = post,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = _sugar.Id, UnitPriceId = unit.Id, Quantity = quantity, Price = price } }
        };
        if (model.IsSupplierInvoice) model.SupplierId = _supplier.Id;
        else model.CustomerId = _customer.Id;
        return model;
    }

    private async Task<int> Save(InvoiceViewModel model)
    {
        _db.Context.ChangeTracker.Clear();
        var result = await _invoices.UpsertInvoiceAsync(model);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        return await _db.Context.BillMasters.AsNoTracking().OrderByDescending(a => a.Id).Select(a => a.Id).FirstAsync();
    }

    private Order NewOrder(int quantity) => new()
    {
        Type = OrderType.TakeAway,
        Cash = true,
        CustomerId = _customer.Id,
        Lines = new List<OrderMenuItem> { new() { MenuItemId = _dish.Id, BillDetail = new BillDetail { Quantity = quantity } } }
    };

    // ---------------------------------------------------------------- orders: a recipe written in kilos

    [Fact]
    public async Task Order_RecipeInKilo_TakesTheBaseUnitsAndRecordsThemOnTheLine()
    {
        await Seed();

        var result = await _orders.CreateOrderAsync(NewOrder(2), _user);

        Assert.True(result.Success, result.Message);
        Assert.Equal(500, await Sugar()); // 2 cakes * 0.25 kilo * 1000
        var ingredient = await _db.Context.OrderIngredients.AsNoTracking().SingleAsync();
        Assert.Equal(500, ingredient.Quantity);
        var movement = await _db.Context.StoreProductMovements.AsNoTracking().SingleAsync(a => a.Source == StockMovementSource.Order);
        Assert.Equal(-500, movement.QuantityDelta);
    }

    [Fact]
    public async Task Order_RecipeInKilo_CostsTheDishByTheRate_NotByTheKiloQuantity()
    {
        await Seed();

        var id = int.Parse((await _orders.CreateOrderAsync(NewOrder(2), _user)).Data);

        _db.Context.ChangeTracker.Clear();
        var order = await _db.Context.Orders.AsNoTracking().Include(a => a.Bill).Include(a => a.Lines).ThenInclude(a => a.BillDetail).SingleAsync(a => a.Id == id);
        Assert.Equal(1.25m, order.Lines.Single().BillDetail.UnitCostAtPosting); // 250 grams * 0.005
        Assert.Equal(2.5m, order.Bill.BillCost);
        Assert.Equal(100, order.Bill.TotalAmountBeforeDiscountOrExtra);
    }

    [Fact]
    public async Task Order_RecipeInKilo_DeleteGivesBackExactlyWhatWasTaken()
    {
        await Seed();
        var id = int.Parse((await _orders.CreateOrderAsync(NewOrder(3), _user)).Data);
        Assert.Equal(250, await Sugar());

        _db.Context.ChangeTracker.Clear();
        Assert.True((await _orders.DeleteOrderAsync(id)).Success);

        Assert.Equal(1000, await Sugar());
        var back = await _db.Context.StoreProductMovements.AsNoTracking().SingleAsync(a => a.Source == StockMovementSource.OrderCancellation);
        Assert.Equal(750, back.QuantityDelta);
    }

    [Fact]
    public async Task Order_RecipeInKilo_NeedsTheBaseUnitsInStock_NotTheKiloQuantity()
    {
        await Seed(sugarInStock: 400);

        var result = await _orders.CreateOrderAsync(NewOrder(2), _user); // needs 500 grams, only 400 there

        Assert.False(result.Success);
        Assert.Equal("InsufficientStock", result.Message);
        Assert.Equal(400, await Sugar());
        Assert.Empty(await _db.Context.Orders.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Order_RecipeInKilo_ExactlyEnoughBaseUnits_IsAllowedAndLeavesZero()
    {
        await Seed(sugarInStock: 500);

        var result = await _orders.CreateOrderAsync(NewOrder(2), _user);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, await Sugar());
    }

    [Theory]
    [InlineData(1000, 4)]
    [InlineData(999, 3)] // 3.996 portions, only 3 whole ones
    [InlineData(250, 1)]
    [InlineData(249, 0)]
    [InlineData(0, 0)]
    public async Task CashierMenu_PortionsAreTheBaseStockDividedByTheBaseQuantityOfTheRecipe(int sugarInStock, int portions)
    {
        await Seed(sugarInStock);

        var menu = (await _orders.GetCashierMenuAsync()).ToList();

        if (portions == 0)
        {
            Assert.DoesNotContain(menu, a => a.IsAvailable);
            return;
        }

        Assert.Equal(portions, menu.Single().AvailablePortions);
    }

    [Fact]
    public async Task Order_TheSameProductInGramsAndKilos_TakesBothLinesInBaseUnits()
    {
        await Seed();
        _db.Context.MenuItemComponents.Add(new MenuItemComponent { MenuItemId = _dish.Id, ProductId = _sugar.Id, Quantity = 100, UnitPriceId = _gram.Id, TransRate = 1 });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _orders.CreateOrderAsync(NewOrder(2), _user); // (250 + 100) * 2

        Assert.True(result.Success, result.Message);
        Assert.Equal(300, await Sugar());
        Assert.Equal(700, (await _db.Context.OrderIngredients.AsNoTracking().ToListAsync()).Sum(a => a.Quantity));
        Assert.Equal(2, (await _db.Context.OrderIngredients.AsNoTracking().ToListAsync()).Count);
    }

    // ---------------------------------------------------------------- invoices: lines in a kilo

    [Fact]
    public async Task Purchase_TwoLinesInDifferentUnits_AddsBothAsBaseUnits_AndTotalsThePricesAsTyped()
    {
        await Seed(sugarInStock: 0);
        var model = Invoice(InvoiceType.Purchases, _kilo, 1, 7, paid: 12);
        model.Items.Add(new InvoiceItemViewModel { ProductId = _sugar.Id, UnitPriceId = _gram.Id, Quantity = 500, Price = 0.01m });

        var id = await Save(model);

        Assert.Equal(1500, await Sugar());
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync(a => a.Id == id);
        Assert.Equal(12, bill.TotalBillAmountAfterDiscountAndExtra); // 1 * 7 + 500 * 0.01, never multiplied by the rate
        Assert.Equal(new decimal[] { 500, 1000 }, (await _db.Context.StoreProductMovements.AsNoTracking().ToListAsync()).Select(a => a.QuantityDelta).OrderBy(a => a).ToArray());
    }

    [Fact]
    public async Task Purchase_UnitWithoutARateOnTheProduct_CountsAsOneBaseUnit()
    {
        await Seed(sugarInStock: 0);

        var id = await Save(Invoice(InvoiceType.Purchases, _box, 5, 7, paid: 35));

        Assert.Equal(5, await Sugar());
        Assert.Equal(1, (await _db.Context.BillDetails.AsNoTracking().SingleAsync(a => a.BillMasterId == id)).TransRate);
        Assert.Equal(7, await Cost());
    }

    [Theory]
    [InlineData(7, 1000, 0.007)]
    [InlineData(10, 4, 2.5)]
    [InlineData(10, 3, 3.333333)]
    public async Task Purchase_TheCostIsThePricePerBaseUnit(double price, int rate, double expectedCost)
    {
        await Seed(sugarInStock: 0);
        var unit = new UnitPrice { Code = "9", ArName = "Pack", EnName = "Pack" };
        _db.Context.Add(unit);
        await _db.Context.SaveChangesAsync();
        var products = new ProductRepository(_db.Context, _db.Http);
        var edit = new Product { Id = _sugar.Id, ArName = "Sugar", EnName = "Sugar", Cost = 0.005m };
        edit.Units = new List<ProductUnit> { new() { UnitPriceId = _gram.Id, TransRate = 1, IsDefault = true }, new() { UnitPriceId = _kilo.Id, TransRate = 1000 }, new() { UnitPriceId = unit.Id, TransRate = rate } };
        _db.Context.ChangeTracker.Clear();
        Assert.True((await products.UpsertProductAsync(edit)).Success);

        await Save(Invoice(InvoiceType.Purchases, unit, 2, (decimal)price, paid: 2 * (decimal)price));

        Assert.InRange((double)await Cost(), expectedCost - 0.000001, expectedCost + 0.000001);
        Assert.Equal(2 * rate, await Sugar());
    }

    [Fact]
    public async Task SalesReturn_InAKilo_PutsTheBaseUnitsBack_AndDeleteTakesThemOut()
    {
        await Seed(sugarInStock: 0);

        var id = await Save(Invoice(InvoiceType.SalesReturn, _kilo, 2, 20));
        Assert.Equal(2000, await Sugar());
        Assert.Equal(0.005m, await Cost()); // only a purchase changes the cost

        _db.Context.ChangeTracker.Clear();
        Assert.True((await _invoices.DeleteInvoiceAsync(id)).Success);
        Assert.Equal(0, await Sugar());
    }

    [Fact]
    public async Task PurchaseReturn_InAKilo_TakesTheBaseUnitsOut_AndLeavesTheCostAlone()
    {
        await Seed(sugarInStock: 5000);

        await Save(Invoice(InvoiceType.Returns, _kilo, 2, 7));

        Assert.Equal(3000, await Sugar());
        Assert.Equal(0.005m, await Cost());
    }

    [Fact]
    public async Task Sale_InAKilo_DeleteGivesTheBaseUnitsBack()
    {
        await Seed(sugarInStock: 5000);
        var id = await Save(Invoice(InvoiceType.Sales, _kilo, 2, 20));
        Assert.Equal(3000, await Sugar());

        _db.Context.ChangeTracker.Clear();
        Assert.True((await _invoices.DeleteInvoiceAsync(id)).Success);

        Assert.Equal(5000, await Sugar());
    }

    [Fact]
    public async Task Invoice_EditedFromKilosToGrams_TakesBackTheKilosAndAppliesTheGrams()
    {
        await Seed(sugarInStock: 0);
        var id = await Save(Invoice(InvoiceType.Purchases, _kilo, 3, 7, paid: 21));
        Assert.Equal(3000, await Sugar());
        Assert.Equal(0.007m, await Cost());

        var edit = Invoice(InvoiceType.Purchases, _gram, 200, 0.01m, paid: 2);
        edit.Id = id;
        await Save(edit);

        Assert.Equal(200, await Sugar());
        Assert.Equal(0.01m, await Cost());
        Assert.Equal(1, (await _db.Context.BillDetails.AsNoTracking().SingleAsync(a => a.BillMasterId == id)).TransRate);
    }

    [Fact]
    public async Task Invoice_EditedFromGramsToKilos_TakesBackTheGramsAndAppliesTheKilos()
    {
        await Seed(sugarInStock: 0);
        var id = await Save(Invoice(InvoiceType.Purchases, _gram, 200, 0.01m, paid: 2));
        Assert.Equal(200, await Sugar());

        var edit = Invoice(InvoiceType.Purchases, _kilo, 1, 7, paid: 7);
        edit.Id = id;
        await Save(edit);

        Assert.Equal(1000, await Sugar());
        Assert.Equal(0.007m, await Cost());
    }

    [Fact]
    public async Task Sale_InAKilo_RecordsTheCostOfAKilo_AndTheProfitFollowsIt()
    {
        await Seed(sugarInStock: 5000);

        var id = await Save(Invoice(InvoiceType.Sales, _kilo, 2, 20));

        var detail = await _db.Context.BillDetails.AsNoTracking().SingleAsync(a => a.BillMasterId == id);
        Assert.Equal(5, detail.UnitCostAtPosting); // 1000 grams * 0.005
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync(a => a.Id == id);
        Assert.Equal(10, bill.BillCost);
        Assert.Equal(30, bill.BillProfit);
    }

    // ---------------------------------------------------------------- posting an invoice that was saved without stock

    [Fact]
    public async Task Post_PurchaseSavedLater_InAKilo_AddsTheBaseUnitsAndSetsTheCostPerBaseUnit()
    {
        await Seed(sugarInStock: 0);
        var id = await Save(Invoice(InvoiceType.Purchases, _kilo, 3, 7, paid: 21, post: false));
        Assert.Equal(0, await Sugar());
        Assert.Equal(0.005m, await Cost());

        var result = await _invoices.PostInvoiceAsync(id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(3000, await Sugar());
        Assert.Equal(0.007m, await Cost());
        Assert.Equal(3000, (await _db.Context.StoreProductMovements.AsNoTracking().SingleAsync()).QuantityDelta);
    }

    [Fact]
    public async Task Post_SaleSavedLater_InAKilo_TakesTheBaseUnits_AndKeepsTheCost()
    {
        await Seed(sugarInStock: 5000);
        var id = await Save(Invoice(InvoiceType.Sales, _kilo, 2, 20, post: false));
        Assert.Equal(5000, await Sugar());

        var result = await _invoices.PostInvoiceAsync(id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(3000, await Sugar());
        Assert.Equal(0.005m, await Cost());
    }

    // ---------------------------------------------------------------- reading it back

    [Fact]
    public async Task GetById_AKiloInvoice_ShowsTheKiloQuantityAndPrice_NotTheBaseUnits()
    {
        await Seed(sugarInStock: 0);
        var id = await Save(Invoice(InvoiceType.Purchases, _kilo, 3, 7, paid: 21));

        var model = await _invoices.GetInvoiceByIdAsync(id);

        var line = model.Items.Single();
        Assert.Equal(3, line.Quantity);
        Assert.Equal(7, line.Price);
        Assert.Equal(_kilo.Id, line.UnitPriceId);
        Assert.Equal(21, model.SubTotal);
    }
}
