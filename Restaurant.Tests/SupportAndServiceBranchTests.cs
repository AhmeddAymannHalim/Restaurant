using Microsoft.EntityFrameworkCore;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The bill-type helpers every invoice, bill list and report leans on, a transfer that loses a race, and two service reads
// (a payment's refund request, an employee's login) that had no test with real data behind them.
public class SupportAndServiceBranchTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private IBillSupportRepository Support => _host.Get<IBillSupportRepository>();

    // ---------------------------------------------------------------- bill types

    [Fact]
    public void InvoiceTypes_AreFound_AnUnknownOneIsNot_AndOnlyInvoiceBillTypesMapBack()
    {
        foreach (var info in Support.GetAllInvoiceTypes())
        {
            Assert.Equal(info.BillType, Support.GetInvoiceType(info.Type).BillType);
            Assert.Equal(info.Type, Support.TypeOf(info.BillType));
            Assert.True(Support.IsInvoiceBillType(info.BillType));
        }

        Assert.Null(Support.GetInvoiceType((InvoiceType)999));
        Assert.False(Support.IsInvoiceBillType(BillType.StockTransfers));
        Assert.Throws<InvalidOperationException>(() => Support.TypeOf(BillType.StockTransfers));
        Assert.Equal(Support.GetAllInvoiceTypes().Count, Support.GetInvoiceBillTypes().Count);
    }

    [Theory]
    [InlineData("ORD", BillType.Sales)]
    [InlineData("TRF", BillType.StockTransfers)]
    [InlineData("PUR", BillType.Purchases)]
    [InlineData("PRET", BillType.PurchaseReturn)]
    [InlineData("SHRT", BillType.InventoryDeficit)]
    [InlineData("OPEN", BillType.OpeningBalance)]
    public void BillTypeOfKind_ReadsTheKindsTheListFiltersBy(string kind, BillType expected)
    {
        Assert.Equal(expected, Support.BillTypeOfKind(kind));
    }

    [Fact]
    public void BillTypeOfKind_AnUnknownKindIsNothing()
    {
        Assert.Null(Support.BillTypeOfKind("nope"));
        Assert.Null(Support.BillTypeOfKind(null));
    }

    private async Task SeedBills()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        var product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _host.Context.AddRange(store, product, unit, supplier, customer, user, section);
        await _host.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _host.Context.MenuItems.Add(dish);
        await _host.Context.SaveChangesAsync();

        var invoices = _host.Get<IInvoiceRepository>();
        InvoiceViewModel Make(InvoiceType type) => new()
        {
            InvoiceType = type, StoreId = store.Id, AutoPost = true, AutoStock = true,
            SupplierId = type == InvoiceType.Purchases ? supplier.Id : null,
            CustomerId = type == InvoiceType.Sales ? customer.Id : null,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = product.Id, UnitPriceId = unit.Id, Quantity = 1, Price = 10 } }
        };

        Assert.True((await invoices.UpsertInvoiceAsync(Make(InvoiceType.Purchases))).Success);
        await _host.Get<IStockRepository>().AdjustAsync(store.Id, product.Id, 10, allowNegative: true);
        Assert.True((await invoices.UpsertInvoiceAsync(Make(InvoiceType.Sales))).Success);
        var order = await new OrderRepository(_host.Context).CreateOrderAsync(new Order { Type = OrderType.TakeAway, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, user.Id);
        Assert.True(order.Success, order.Message);
        _host.Context.ChangeTracker.Clear();
    }

    private async Task<List<string>> KindsOf(params string[] kinds)
    {
        var query = Support.OfKind(_host.Context.BillMasters.AsNoTracking().Include(a => a.BillSetting), kinds);

        return (await query.ToListAsync()).Select(a => a.BillSetting.BillType.ToString()).OrderBy(a => a).ToList();
    }

    [Fact]
    public async Task OfKind_FiltersBillsByTheirKind_OrdersAreSalesButInvoicesAreNotOrders()
    {
        await SeedBills();

        Assert.Equal(new[] { "Purchases" }, await KindsOf("PUR"));
        // a cashier order and a sales invoice are both sales; the order kind alone is only the cashier order
        Assert.Equal(new[] { "Sales", "Sales" }, await KindsOf("SAL"));
        Assert.Equal(new[] { "Sales" }, await KindsOf("ORD"));
        Assert.Equal(new[] { "Purchases", "Sales", "Sales" }, await KindsOf("PUR", "SAL"));
        Assert.Empty(await KindsOf("nope"));
        Assert.Empty(await KindsOf());
    }

    [Fact]
    public async Task RecalculateInvoicePayment_OfAnUnknownBill_DoesNothing()
    {
        await Support.RecalculateInvoicePaymentAsync(404);

        Assert.Empty(await _host.Context.BillMasters.AsNoTracking().ToListAsync());
    }

    // ---------------------------------------------------------------- a store transfer that loses a race

    [Fact]
    public async Task StoreConvert_CreateAndDelete_WhenTheRowWasChangedByAnotherUser_AnswerConcurrentUpdate()
    {
        var from = new Store { Code = "1", ArName = "A", EnName = "A" };
        var to = new Store { Code = "2", ArName = "B", EnName = "B" };
        var product = new Product { Code = "1", ArName = "P", EnName = "P", Cost = 1 };
        _host.Context.AddRange(from, to, product);
        await _host.Context.SaveChangesAsync();
        await _host.Get<IStockRepository>().AdjustAsync(from.Id, product.Id, 10, allowNegative: true);
        _host.Context.ChangeTracker.Clear();
        var converts = new StoreConvertRepository(_host.Context);
        var convert = new StoreConvert
        {
            FromStoreId = from.Id, ToStoreId = to.Id,
            StoreConvertProducts = new List<StoreConvertProduct> { new() { ProductId = product.Id, Quantity = 3 } }
        };
        Assert.True((await converts.CreateConvertAsync(convert)).Success);
        _host.Context.ChangeTracker.Clear();
        _host.Db.SaveFailure.Failure = new DbUpdateConcurrencyException("forced");

        var created = await converts.CreateConvertAsync(new StoreConvert
        {
            FromStoreId = from.Id, ToStoreId = to.Id,
            StoreConvertProducts = new List<StoreConvertProduct> { new() { ProductId = product.Id, Quantity = 1 } }
        });
        var deleted = await converts.DeleteConvertAsync(1);

        Assert.Equal("ConcurrentUpdate", created.Message);
        Assert.Equal("ConcurrentUpdate", deleted.Message);
    }

    // ---------------------------------------------------------------- service reads with data behind them

    [Fact]
    public async Task SupplierPayment_RefundRequest_ShowsWhatIsStillRefundable_AndNullForAnUnknownPayment()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        var product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        _host.Context.AddRange(store, product, unit, supplier);
        await _host.Context.SaveChangesAsync();
        var invoices = _host.Get<IInvoiceRepository>();
        Assert.True((await invoices.UpsertInvoiceAsync(new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Purchases, StoreId = store.Id, SupplierId = supplier.Id, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = product.Id, UnitPriceId = unit.Id, Quantity = 10, Price = 10 } }
        })).Success);
        var bill = await _host.Context.BillMasters.AsNoTracking().SingleAsync();
        var payments = new SupplierPaymentRepository(_host.Context, _host.Get<Restaurant.DAL.IRepositories.Commons.IJournalEntryRepository>());
        Assert.True((await payments.UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 40 })).Success);
        var payment = await _host.Context.PaymentInvoiceSupplier.AsNoTracking().SingleAsync();
        _host.Context.ChangeTracker.Clear();
        var service = _host.Get<ISupplierPaymentService>();

        var request = await service.GetRefundRequestAsync(payment.Id);

        Assert.Equal(40, request.OriginalPaymentAmount);
        Assert.Equal(40, request.RemainingRefundable);
        Assert.Empty(request.Refunds);
        Assert.Null(await service.GetRefundRequestAsync(404));
    }

    [Fact]
    public async Task EmployeeAccount_OfAnEmployeeWithALogin_ShowsThatLogin_WithoutOneItShowsTheEmployee()
    {
        var job = new Job { Code = "1", ArName = "Cashier", EnName = "Cashier" };
        _host.Context.Add(job);
        await _host.Context.SaveChangesAsync();
        var employees = new EmployeeRepository(_host.Context, _host.Db.Http);
        var withLogin = new Employee { ArName = "Ali", EnName = "Ali", JobId = job.Id, PhoneNumber = "01000000001", BasicSalary = 1, WorkDays = 6, DailyHours = 8 };
        var without = new Employee { ArName = "Omar", EnName = "Omar", JobId = job.Id, PhoneNumber = "01000000002", BasicSalary = 1, WorkDays = 6, DailyHours = 8 };
        await employees.UpsertEmployeeAsync(withLogin, null, null);
        await employees.UpsertEmployeeAsync(without, null, null);
        _host.Context.ChangeTracker.Clear();
        var accounts = _host.Get<IEmployeeAccountRepository>();
        Assert.True((await accounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "ali", Email = "ali@test.com" }, withLogin.Id, "Pass123")).Success);
        _host.Context.ChangeTracker.Clear();
        var service = _host.Get<IEmployeeAccountService>();

        var login = await service.GetAccountByEmployeeIdAsync(withLogin.Id);
        var noLogin = await service.GetAccountByEmployeeIdAsync(without.Id);

        Assert.Equal("ali", login.UserName);
        Assert.NotEmpty(login.Id);
        Assert.True(string.IsNullOrEmpty(noLogin.Id));
        Assert.Equal(without.Id, noLogin.EmployeeId);
        Assert.Null(await service.GetAccountByEmployeeIdAsync(404));
    }
}
