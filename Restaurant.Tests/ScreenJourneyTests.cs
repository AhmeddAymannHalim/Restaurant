using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ResturantPL.Controllers;
using ResturantPL.Controllers.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The business screens, driven through the real controller, service, repository and database.
public class ScreenJourneyTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private Restaurant.DAL.ApplicationDbContext Db => _host.Context;

    private async Task<(int store, int product, int unit, int supplier, int customer, int employee, int expense)> Seed()
    {
        _host.Db.SeedInvoiceAccounts();
        var user = new ApplicationUser { Id = "user-1", UserName = "halim", Email = "h@test.com" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var product = new Product { Code = "1", ArName = "P", EnName = "P", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "U", EnName = "U" };
        var expense = new Expense { Code = "1", ArName = "E", EnName = "E", Price = 10 };
        var job = new Job { Code = "1", ArName = "J", EnName = "J" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        var employee = new Employee { Code = "1", ArName = "Emp", EnName = "Emp", Job = job, BasicSalary = 3000 };
        Db.AddRange(user, store, section, product, unit, expense, supplier, customer, employee);
        Db.ApplicationSettings.Add(new ApplicationSetting { Password = "1234", OrderTax = 10, OrderService = 5 });
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        return (store.Id, product.Id, unit.Id, supplier.Id, customer.Id, employee.Id, expense.Id);
    }

    private static T Redirects<T>(IActionResult result) where T : class => Assert.IsAssignableFrom<T>(result);

    private static object Json(IActionResult result, string name) => ((JsonResult)result).Value.GetType().GetProperty(name).GetValue(((JsonResult)result).Value);

    // ---------------------------------------------------------------- money records

    [Fact]
    public async Task Deposit_Save_Edit_Delete()
    {
        await Seed();
        var controller = _host.Controller<DepositController>();

        Redirects<RedirectToActionResult>(await controller.Upsert(new DepositViewModel { DeposerName = "Ali", Amount = 500, Description = "capital" }));
        var id = (await Db.Deposits.AsNoTracking().SingleAsync()).Id;
        Assert.Equal("Ali", ((DepositViewModel)((ViewResult)await controller.Upsert(id)).Model).DeposerName);
        Db.ChangeTracker.Clear();
        Redirects<RedirectToActionResult>(await controller.Upsert(new DepositViewModel { Id = id, DeposerName = "Omar", Amount = 700 }));
        Assert.Equal(700, (await Db.Deposits.AsNoTracking().SingleAsync()).Amount);
        Assert.IsType<RedirectToActionResult>(await controller.Upsert(99999));
        Assert.Equal(true, Json(await controller.Delete(id), "success"));
        Assert.Empty(Db.Deposits);
    }

    [Fact]
    public async Task ExitExpense_Scrounge_Salary_SaveAndTheEmployeeSummaryAdds()
    {
        var seed = await Seed();
        Redirects<RedirectToActionResult>(await _host.Controller<ExitExpenseController>().Upsert(new ExitExpenseViewModel { ExpenseId = seed.expense, Price = 80 }));
        Redirects<RedirectToActionResult>(await _host.Controller<ScroungeController>().Upsert(new ScroungeViewModel { EmployeeId = seed.employee, Amount = 200, State = MoneyState.Dept, Info = "advance" }));
        Redirects<RedirectToActionResult>(await _host.Controller<ScroungeController>().Upsert(new ScroungeViewModel { EmployeeId = seed.employee, Amount = 100, State = MoneyState.Incentive }));
        var salary = _host.Controller<SalaryController>();
        Redirects<RedirectToActionResult>(await salary.Upsert(new SalaryViewModel { EmployeeId = seed.employee, FromDate = new DateTime(2030, 1, 1), ToDate = new DateTime(2030, 1, 31), SalaryAmount = 2500 }));

        var summary = Assert.IsType<JsonResult>(await salary.EmployeeExpenses(seed.employee, new DateTime(2000, 1, 1), new DateTime(2100, 1, 1)));
        Assert.Equal(2900m, summary.Value.GetType().GetProperty("TotalSalary", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(summary.Value));
        Assert.Single(Db.ExitExpenses);
        Assert.Equal(2, Db.Scrounges.Count());
        Assert.Single(Db.Salaries);

        foreach (var (controller, ids) in new (Controller, int)[] { (_host.Controller<ExitExpenseController>(), Db.ExitExpenses.Select(a => a.Id).First()), (_host.Controller<SalaryController>(), Db.Salaries.Select(a => a.Id).First()), (_host.Controller<ScroungeController>(), Db.Scrounges.Select(a => a.Id).First()) })
        {
            var delete = controller.GetType().GetMethod("Delete");
            var result = await (Task<IActionResult>)delete.Invoke(controller, new object[] { ids });
            Assert.Equal(true, Json(result, "success"));
        }
    }

    // ---------------------------------------------------------------- customers, suppliers, products, menu

    [Fact]
    public async Task Customer_And_Supplier_Screens_KeepBalancesAndLockDeleteWhileOwing()
    {
        await Seed();
        var customers = _host.Controller<CustomerController>();
        Redirects<RedirectToActionResult>(await customers.Upsert(new CustomerViewModel { ArName = "Cust Ar", EnName = "Cust En", PhoneNumber = "01044444444", Debit = 300 }));
        var customer = await Db.Customers.AsNoTracking().Include(a => a.Account).OrderByDescending(a => a.Id).FirstAsync();
        var json = Assert.IsType<JsonResult>(await customers.CustomerBalance(customer.Id));
        Assert.NotNull(json.Value);
        Assert.Equal(false, Json(await customers.Delete(customer.Id), "success"));
        var opened = Assert.IsType<ViewResult>(await customers.Upsert(customer.Id));
        Assert.Equal("Cust En", ((CustomerViewModel)opened.Model).EnName);

        var suppliers = _host.Controller<SupplierController>();
        Redirects<RedirectToActionResult>(await suppliers.Upsert(new SupplierViewModel { ArName = "Supp Ar", EnName = "Supp En", PhoneNumber = "01066666666", Credit = 600 }));
        var supplier = await Db.Suppliers.AsNoTracking().Include(a => a.Account).OrderByDescending(a => a.Id).FirstAsync();
        Assert.Equal(600, supplier.Account.BalanceValue);
        Assert.Equal(false, Json(await suppliers.Delete(supplier.Id), "success"));
        Assert.IsType<ViewResult>(await suppliers.Upsert(supplier.Id));
    }

    [Fact]
    public async Task Product_And_Menu_Screens_SaveStockAndRecipes()
    {
        var seed = await Seed();
        var products = _host.Controller<ProductController>();
        Redirects<RedirectToActionResult>(await products.Upsert(new ProductViewModel
        {
            ArName = "Flour", EnName = "Flour", Cost = 4,
            Stocks = new List<ProductStockViewModel> { new() { StoreId = seed.store, Quantity = 30 } }
        }));
        var flour = await Db.Products.AsNoTracking().OrderByDescending(a => a.Id).FirstAsync();
        Assert.Equal(30, (await Db.StoreProductMovements.AsNoTracking().SingleAsync(a => a.ProductId == flour.Id)).QuantityDelta);

        var menu = _host.Controller<MenuController>();
        var sectionId = (await Db.MenuSections.AsNoTracking().FirstAsync()).Id;
        Redirects<RedirectToActionResult>(await menu.Upsert(new MenuItemViewModel
        {
            ArName = "Pizza", EnName = "Pizza", MenuSectionId = sectionId, StoreId = seed.store, Price = 60, IsAvailable = true,
            Components = new List<MenuItemComponentViewModel> { new() { ProductId = flour.Id, Quantity = 2 } }
        }));
        var dish = await Db.MenuItems.AsNoTracking().Include(a => a.Components).SingleAsync();
        Assert.Single(dish.Components);
        Assert.IsType<JsonResult>(await menu.GetProductsByStore(seed.store));
        Assert.IsType<ViewResult>(await menu.Upsert(dish.Id));
        Assert.Equal(true, Json(await menu.Delete(dish.Id), "success"));
        Assert.Equal(false, Json(await products.Delete(flour.Id), "success"));   // still holds stock
    }

    // ---------------------------------------------------------------- users and roles

    [Fact]
    public async Task Role_And_User_Screens_CreateRenameAndProtect()
    {
        await Seed();
        var roles = _host.Controller<RoleController>();
        Redirects<RedirectToActionResult>(await roles.Upsert(new RoleViewModel { Name = "Waiter" }));
        var role = await Db.Roles.AsNoTracking().SingleAsync(a => a.Name == "Waiter");

        var users = _host.Controller<UserController>();
        Redirects<RedirectToActionResult>(await users.Upsert(new UserViewModel { UserName = "sara", FirstName = "Sara", LastName = "A", Email = "sara@test.com", PhoneNumber = "01011112222", UserType = "Waiter", Password = "Pass123", ConfirmPassword = "Pass123" }));
        var user = await Db.Users.AsNoTracking().SingleAsync(a => a.UserName == "sara");
        Assert.Equal(false, Json(await roles.Delete(role.Id), "success"));      // a user is in it
        Assert.IsType<ViewResult>(await users.Upsert(user.Id));
        Assert.IsType<ViewResult>(await roles.Upsert(role.Id));
        Assert.Equal(true, Json(await users.Delete(user.Id), "success"));
        Assert.Equal(true, Json(await roles.Delete(role.Id), "success"));
    }

    [Fact]
    public async Task EmployeeAccount_Screen_CreatesTheLoginOfAnEmployee()
    {
        var seed = await Seed();
        var controller = _host.Controller<EmployeeAccountController>();

        Assert.IsType<ViewResult>(await controller.Upsert(null, seed.employee));
        Redirects<RedirectToActionResult>(await controller.Upsert(new EmployeeAccountViewModel { EmployeeId = seed.employee, UserName = "empuser", Email = "emp@test.com", Password = "Pass123", ConfirmPassword = "Pass123" }));

        var login = await Db.Users.AsNoTracking().SingleAsync(a => a.UserName == "empuser");
        Assert.Equal("J", login.UserType);
        Assert.IsType<ViewResult>(await controller.Index(new PageRequest()));
        Assert.IsType<ViewResult>(await controller.Upsert(login.Id, null));
    }

    // ---------------------------------------------------------------- accounting

    [Fact]
    public async Task ChartAccount_And_ManualEntry_Screens_MoveBalances()
    {
        await Seed();
        var chart = _host.Controller<ChartAccountController>();
        Redirects<RedirectToActionResult>(await chart.Upsert(new AccountDefinitionViewModel { ArName = "Cash", EnName = "Cash", Kind = AccountKind.GeneralLedger }));
        var root = await Db.Accounts.AsNoTracking().OrderByDescending(a => a.Id).FirstAsync();
        Redirects<RedirectToActionResult>(await chart.Upsert(new AccountDefinitionViewModel { ArName = "Till", EnName = "Till", Kind = AccountKind.GeneralLedger, ParentAccountId = root.Id, OpeningBalance = 1000 }));
        Redirects<RedirectToActionResult>(await chart.Upsert(new AccountDefinitionViewModel { ArName = "Revenue", EnName = "Revenue", Kind = AccountKind.GeneralLedger, ParentAccountId = root.Id }));
        var till = await Db.Accounts.AsNoTracking().SingleAsync(a => a.EnName == "Till");
        var revenue = await Db.Accounts.AsNoTracking().SingleAsync(a => a.EnName == "Revenue");
        Assert.IsType<ViewResult>(await chart.Upsert(till.Id, null));

        var entries = _host.Controller<EntryController>();
        Assert.IsType<ViewResult>(await entries.Upsert());
        Redirects<RedirectToActionResult>(await entries.Upsert(new EntryViewModel
        {
            EntryDate = new DateTime(2030, 1, 1), Remarks = "in",
            Lines = new List<EntryLineViewModel> { new() { AccountId = till.Id, Debit = 100, Credit = 0 }, new() { AccountId = revenue.Id, Debit = 0, Credit = 100 } }
        }));
        Db.ChangeTracker.Clear();
        Assert.Equal(1100, (await Db.Accounts.AsNoTracking().SingleAsync(a => a.Id == till.Id)).BalanceValue);
        var entry = await Db.EntryMasters.AsNoTracking().SingleAsync();
        Assert.IsType<ViewResult>(await entries.Details(entry.Id));
        Assert.Equal(true, Json(await entries.Delete(entry.Id), "success"));
        Db.ChangeTracker.Clear();
        Assert.Equal(1000, (await Db.Accounts.AsNoTracking().SingleAsync(a => a.Id == till.Id)).BalanceValue);
        Assert.Equal(true, Json(await chart.Delete(revenue.Id), "success"));
    }

    [Fact]
    public async Task AccountSetting_And_ApplicationSetting_Screens_SaveAndReload()
    {
        await Seed();
        var accountSetting = _host.Controller<AccountSettingController>();
        var page = Assert.IsType<ViewResult>(await accountSetting.Index());
        var model = Assert.IsType<AccountSettingViewModel>(page.Model);
        Redirects<RedirectToActionResult>(await accountSetting.Index(model));

        var app = _host.Controller<ApplicationSettingController>();
        Assert.IsType<ViewResult>(app.Index());
        var settings = Assert.IsType<ApplicationSettingViewModel>(((ViewResult)await app.ApplicationSetting()).Model);
        settings.OrderTax = 14;
        settings.Password = "newpass";
        Redirects<RedirectToActionResult>(await app.ApplicationSetting(settings));
        Assert.Equal(14, (await Db.ApplicationSettings.AsNoTracking().SingleAsync()).OrderTax);
        Assert.NotEqual("1234", (await Db.ApplicationSettings.AsNoTracking().SingleAsync()).Password);
        Assert.IsType<ViewResult>(await app.PrinterSetting());
    }

    // ---------------------------------------------------------------- invoices, payments and refunds

    private async Task<int> NewInvoice(InvoiceType type, (int store, int product, int unit, int supplier, int customer, int employee, int expense) seed, decimal quantity, decimal price, decimal paid)
    {
        var model = new InvoiceViewModel
        {
            InvoiceType = type, StoreId = seed.store, Paid = paid, PayType = paid > 0 ? PayType.Cash : PayType.Later, AutoPost = true, AutoStock = true,
            Items = { new InvoiceItemViewModel { ProductId = seed.product, UnitPriceId = seed.unit, Quantity = quantity, Price = price } }
        };
        if (model.IsSupplierInvoice) model.SupplierId = seed.supplier; else model.CustomerId = seed.customer;
        Redirects<RedirectToActionResult>(await _host.Controller<BillController>().Upsert(model));
        Db.ChangeTracker.Clear();

        return (await Db.BillMasters.AsNoTracking().OrderByDescending(a => a.Id).FirstAsync()).Id;
    }

    [Fact]
    public async Task Invoice_Screens_PurchaseSaleReturnPostDetailsAndDelete()
    {
        var seed = await Seed();
        var bills = _host.Controller<BillController>();
        var purchase = await NewInvoice(InvoiceType.Purchases, seed, 20, 10, 50);

        Assert.IsType<ViewResult>(await bills.Index(new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) }));
        Assert.IsType<ViewResult>(await bills.Details(purchase));
        Assert.IsType<ViewResult>(await bills.Upsert(purchase, null));
        Assert.IsType<ViewResult>(await bills.Upsert((int?)null, InvoiceType.Sales));
        Assert.IsType<JsonResult>(await bills.SupplierBalance(seed.supplier));
        Assert.IsType<JsonResult>(await bills.CustomerBalance(seed.customer));
        Assert.IsAssignableFrom<IActionResult>(await bills.Pdf(purchase));

        await NewInvoice(InvoiceType.Sales, seed, 5, 30, 0);
        var sale = (await Db.BillMasters.AsNoTracking().OrderByDescending(a => a.Id).FirstAsync()).Id;
        await NewInvoice(InvoiceType.SalesReturn, seed, 2, 30, 0);
        await NewInvoice(InvoiceType.Returns, seed, 3, 10, 0);

        Assert.IsType<JsonResult>(await bills.PostPending());
        Assert.IsType<JsonResult>(await bills.Post(purchase));
        Assert.NotNull(Json(await bills.Delete(sale + 100), "success"));
        Assert.Equal(true, Json(await bills.Delete(sale), "success"));
    }

    [Fact]
    public async Task CustomerPayment_And_Refund_Screens_CollectApproveRejectAndPrint()
    {
        var seed = await Seed();
        await NewInvoice(InvoiceType.Purchases, seed, 10, 5, 50);          // the goods first
        var bill = await NewInvoice(InvoiceType.Sales, seed, 2, 10, 0);    // then a credit sale of 20
        var payments = _host.Controller<CustomerPaymentController>();

        Redirects<RedirectToActionResult>(await payments.Upsert(new PaymentViewModel { PartnerId = seed.customer, BillMasterId = bill, Paid = 1, Description = "cash" }));
        var payment = await Db.PaymentInvoiceCustomer.AsNoTracking().SingleAsync();
        Assert.IsType<ViewResult>(await payments.Index(new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) }));
        Assert.IsType<ViewResult>(await payments.Upsert(payment.Id));
        Assert.IsType<JsonResult>(await payments.Balance(seed.customer));
        Assert.IsType<JsonResult>(await payments.OpenInvoices(seed.customer));
        Assert.IsAssignableFrom<IActionResult>(await payments.Pdf(payment.Id));
        Assert.IsType<ViewResult>(await payments.Refunds(payment.Id));

        Redirects<RedirectToActionResult>(await payments.RequestRefund(new PaymentRefundRequestViewModel { OriginalPaymentId = payment.Id, Amount = 1, Reason = "back" }));
        var refund = await Db.PaymentRefunds.AsNoTracking().SingleAsync();
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Redirects<RedirectToActionResult>(await payments.ApproveRefund(refund.Id, payment.Id));
        Db.ChangeTracker.Clear();
        Assert.Equal(RefundStatus.Approved, (await Db.PaymentRefunds.AsNoTracking().SingleAsync()).Status);
        Redirects<RedirectToActionResult>(await payments.RejectRefund(refund.Id, payment.Id));
        Assert.Equal(false, Json(await payments.Delete(payment.Id), "success"));   // has a refund
    }

    // ---------------------------------------------------------------- store transfer

    [Fact]
    public async Task StoreConvert_Screen_MovesStockAndPrints()
    {
        var seed = await Seed();
        await NewInvoice(InvoiceType.Purchases, seed, 10, 5, 50);
        var other = new Store { Code = "2", ArName = "B", EnName = "B" };
        Db.Stores.Add(other);
        await Db.SaveChangesAsync();
        var controller = _host.Controller<StoreConvertController>();

        Redirects<RedirectToActionResult>(await controller.Upsert(new StoreConvertViewModel
        {
            FromStoreId = seed.store, ToStoreId = other.Id,
            Items = new List<StoreConvertItemViewModel> { new() { ProductId = seed.product, Quantity = 4 } }
        }));

        var convert = await Db.StoreConverts.AsNoTracking().SingleAsync();
        Assert.IsType<ViewResult>(await controller.Index(new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) }));
        Assert.IsType<ViewResult>(await controller.Upsert(convert.Id));
        Assert.IsAssignableFrom<IActionResult>(await controller.Pdf(convert.Id));
        Assert.Equal(true, Json(await controller.Delete(convert.Id), "success"));
    }

    // ---------------------------------------------------------------- the cashier

    [Fact]
    public async Task Cashier_Screens_CreatePayMergeMoveAndDeleteOrders()
    {
        var seed = await Seed();
        await NewInvoice(InvoiceType.Purchases, seed, 100, 2, 200);
        var section = await Db.MenuSections.AsNoTracking().FirstAsync();
        var dish = new MenuItem
        {
            Code = "1", ArName = "Pizza", EnName = "Pizza", Price = 60, StoreId = seed.store, MenuSectionId = section.Id,
            Components = new List<MenuItemComponent> { new() { ProductId = seed.product, Quantity = 2 } }
        };
        var t1 = new Restaurant.DAL.Models.Table { Code = "1", ArName = "T1", EnName = "T1", NumOfPersons = 4 };
        var t2 = new Restaurant.DAL.Models.Table { Code = "2", ArName = "T2", EnName = "T2", NumOfPersons = 4 };
        Db.AddRange(dish, t1, t2);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        var orders = _host.Controller<OrderController>();

        Assert.IsType<ViewResult>(await orders.Upsert((int?)null, null));

        OrderViewModel New(OrderType type, int? table, bool cash) => new() { Type = type, Cash = cash, TableId = table, Items = { new OrderItemViewModel { MenuItemId = dish.Id, Quantity = 1, Price = 60 } } };
        Redirects<RedirectToActionResult>(await orders.Upsert(New(OrderType.TakeAway, null, true)));
        Redirects<RedirectToActionResult>(await orders.Upsert(New(OrderType.Hall, t1.Id, false)));
        Redirects<RedirectToActionResult>(await orders.Upsert(New(OrderType.Hall, t2.Id, false)));
        Db.ChangeTracker.Clear();
        var all = await Db.Orders.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var cashOrder = all[0];
        var first = all[1];
        var second = all[2];

        Assert.IsType<ViewResult>(await orders.List(new PageRequest { Status = "pending" }));
        Assert.IsType<ViewResult>(await orders.Details(first.Id));
        Assert.IsAssignableFrom<IActionResult>(await orders.Pdf(first.Id));
        Assert.IsType<JsonResult>(await orders.CustomerInfo(seed.customer));
        Assert.IsType<ViewResult>(await orders.Move(first.Id));
        Assert.IsType<ViewResult>(await orders.Merge(first.Id));

        var line = await Db.OrderMenuItems.AsNoTracking().FirstAsync(a => a.OrderId == first.Id);
        Redirects<RedirectToActionResult>(await orders.Move(new OrderMoveViewModel { Id = first.Id, TargetTableId = t2.Id, OrderMenuItemId = line.Id }));
        Db.ChangeTracker.Clear();
        Redirects<RedirectToActionResult>(await orders.Merge(new OrderMoveViewModel { Id = second.Id, TargetTableId = t1.Id }));

        Redirects<RedirectToActionResult>(await orders.Pay(second.Id, Shared.Enums.PaymentMethod.Cash));
        Assert.Equal(true, Json(await orders.Delete(cashOrder.Id), "success"));
        Assert.IsType<ViewResult>(await orders.List(new PageRequest { Status = "history" }));
    }

    [Fact]
    public async Task Dashboard_And_KitchenScreens_ShowOpenOrders()
    {
        await Seed();
        var dashboard = _host.Controller<DashboardController>();

        Assert.IsType<ViewResult>(await dashboard.Index());
        Assert.IsAssignableFrom<IActionResult>(await dashboard.Board());
    }

    [Fact]
    public async Task Product_And_Menu_Screens_UnitsPricesPerPriceType_AndAKiloRecipe()
    {
        var seed = await Seed();
        var gram = new UnitPrice { Code = "g", ArName = "Gram", EnName = "Gram" };
        var kilo = new UnitPrice { Code = "k", ArName = "Kilo", EnName = "Kilo" };
        var types = Enumerable.Range(1, 3).Select(n => new SellType { Code = n.ToString(), ArName = "T" + n, EnName = "T" + n, Show = true }).ToList();
        Db.AddRange(gram, kilo);
        Db.AddRange(types);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var products = _host.Controller<ProductController>();
        var form = new ProductViewModel
        {
            ArName = "Sugar", EnName = "Sugar", Cost = 0.012m,
            Units = new List<ProductUnitViewModel>
            {
                new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true, LastPurchase = 0.012m, Prices = types.Select(t => new ProductUnitPriceViewModel { SellTypeId = t.Id, Price = 1 }).ToList() },
                new() { UnitPriceId = kilo.Id, TransRate = 1000, Prices = types.Select(t => new ProductUnitPriceViewModel { SellTypeId = t.Id, Price = 12 }).ToList() }
            }
        };

        // a refused post shows the form again with every list it needs (units, price types)
        products.ModelState.AddModelError("Units", "x");
        var refused = Assert.IsType<ViewResult>(await products.Upsert(form));
        var refusedModel = Assert.IsType<ProductViewModel>(refused.Model);
        Assert.Equal(4, refusedModel.SellTypes.Count); // the 3 of the test + the one the invoice accounts need
        Assert.Contains(refusedModel.UnitList, u => u.Id == kilo.Id);
        products.ModelState.Clear();

        Redirects<RedirectToActionResult>(await products.Upsert(form));
        var sugar = await Db.Products.AsNoTracking().SingleAsync(a => a.EnName == "Sugar");
        Db.ChangeTracker.Clear();

        var opened = (ProductViewModel)((ViewResult)await products.Upsert(sugar.Id)).Model;
        Assert.Equal(2, opened.Units.Count);
        Assert.Equal(3, opened.Units.Single(u => u.UnitPriceId == kilo.Id).Prices.Count);
        Assert.Equal(4, opened.SellTypes.Count);

        var menu = _host.Controller<MenuController>();
        var sectionId = (await Db.MenuSections.AsNoTracking().FirstAsync()).Id;
        Redirects<RedirectToActionResult>(await menu.Upsert(new MenuItemViewModel
        {
            ArName = "Cake", EnName = "Cake", MenuSectionId = sectionId, StoreId = seed.store, Price = 60, IsAvailable = true,
            Components = new List<MenuItemComponentViewModel> { new() { ProductId = sugar.Id, UnitPriceId = kilo.Id, Quantity = 0.25m } }
        }));
        var cake = await Db.MenuItems.AsNoTracking().Include(a => a.Components).SingleAsync(a => a.EnName == "Cake");
        Assert.Equal(1000, cake.Components.Single().TransRate);

        var reopened = (MenuItemViewModel)((ViewResult)await menu.Upsert(cake.Id)).Model;
        Assert.Equal(3, reopened.TotalCost);
        Assert.Equal("Kilo", reopened.Components.Single().UnitEnName);

        var lookup = (IEnumerable<StoreProductLookupItem>)((JsonResult)await menu.GetProductsByStore(seed.store)).Value;
        Assert.Equal(2, lookup.Single(a => a.ProductId == sugar.Id).Units.Count);
    }
}
