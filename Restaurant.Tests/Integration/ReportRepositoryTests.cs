using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

public class ReportRepositoryTests : IDisposable
{
    private readonly TestDb _db = new(invoiceAccounts: true);
    private readonly ReportRepository _sut;
    private readonly DateTime _day = DateTime.Today;

    public ReportRepositoryTests() => _sut = new ReportRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private ReportRequest Request(string report, int? partnerId = null, string type = null) => new()
    {
        Report = report, FromDate = _day.AddDays(-30), ToDate = _day.AddDays(30), PartnerId = partnerId, Type = type
    };

    private static decimal Total(ReportViewModel report, string label) => report.Totals.Single(a => a.Label == label).Value;

    private static decimal Row(ReportViewModel report, string label) => (decimal)report.Rows.Single(a => (string)a[0] == label)[1];

    private InvoiceRepository Invoices() => new(_db.Context, new JournalEntryRepository(_db.Context));

    private async Task<(Store store, Product product, UnitPrice unit, Supplier supplier, Customer customer)> SeedInvoiceWorld()
    {
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var product = new Product { Code = "1", ArName = "P", EnName = "P", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "U", EnName = "U" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(store, product, unit, supplier, customer);
        await _db.Context.SaveChangesAsync();
        return (store, product, unit, supplier, customer);
    }

    private static InvoiceViewModel Invoice(InvoiceType type, Store store, Product product, UnitPrice unit, int? partnerId, decimal quantity, decimal price, decimal paid)
    {
        var model = new InvoiceViewModel
        {
            InvoiceType = type, StoreId = store.Id, Paid = paid, PayType = Shared.Enums.PayType.Later, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = product.Id, UnitPriceId = unit.Id, Quantity = quantity, Price = price } }
        };
        if (model.IsSupplierInvoice) model.SupplierId = partnerId; else model.CustomerId = partnerId;
        return model;
    }

    // ---------------------------------------------------------------- statements

    [Fact]
    public async Task SupplierStatement_InvoiceThenPayment_ClosesAtWhatIsStillOwed()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 0))).Success);
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        var payments = new SupplierPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
        Assert.True((await payments.UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 40 })).Success);
        _db.Context.ChangeTracker.Clear();

        var report = await _sut.BuildAsync(Request("SupplierStatement", supplier.Id));

        Assert.Equal(100, Total(report, "Debit"));
        Assert.Equal(40, Total(report, "Credit"));
        Assert.Equal(60, Total(report, "ClosingBalance"));
        Assert.Equal(60, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Code == "S")).BalanceValue);
    }

    [Fact]
    public async Task SupplierStatement_PartlyPaidInvoice_CountsOnlyWhatWasNotPaidAtOnce()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 30));

        var report = await _sut.BuildAsync(Request("SupplierStatement", supplier.Id));

        Assert.Equal(70, Total(report, "Debit"));
        Assert.Equal(70, Total(report, "ClosingBalance"));
    }

    [Fact]
    public async Task SuppliersBalances_AgreeWithTheStatementAndTheAccount()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 0));
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        var payments = new SupplierPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
        await payments.UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 40 });
        _db.Context.ChangeTracker.Clear();

        var report = await _sut.BuildAsync(Request("SuppliersBalances"));

        Assert.Equal(60, Total(report, "ClosingBalance"));
        Assert.Equal(60, (decimal)report.Rows.Single()[4]);
    }

    [Fact]
    public async Task SupplierStatement_OpeningBalanceAndEarlierActivity_FeedTheOpeningRow()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        var saved = await _db.Context.Suppliers.SingleAsync();
        saved.LastBalance = 25;
        await _db.Context.SaveChangesAsync();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 1, 100, 0));
        var request = Request("SupplierStatement", supplier.Id);
        request.FromDate = DateTime.Today.AddDays(1);
        request.ToDate = DateTime.Today.AddDays(5);

        var report = await _sut.BuildAsync(request);

        Assert.Equal(125, Total(report, "OpeningBalance"));
        Assert.Equal(125, Total(report, "ClosingBalance"));
        Assert.Single(report.Rows);
    }

    [Fact]
    public async Task CustomerStatement_ShowsCreditInvoicesPaymentsAndOnAccountOrders()
    {
        var (store, product, unit, _, customer) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, null, 50, 1, 0).WithSupplier(_db.Context));
        _db.Context.ChangeTracker.Clear();
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Sales, store, product, unit, customer.Id, 10, 20, 50))).Success);
        var bill = await _db.Context.BillMasters.AsNoTracking().OrderBy(a => a.Id).LastAsync();
        var payments = new CustomerPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
        Assert.True((await payments.UpsertPaymentAsync(new PaymentInvoiceCustomer { CustomerId = customer.Id, BillMasterId = bill.Id, Paid = 30 })).Success);
        _db.Context.ChangeTracker.Clear();

        var report = await _sut.BuildAsync(Request("CustomerStatement", customer.Id));

        Assert.Equal(0, Total(report, "OpeningBalance"));
        Assert.Equal(150, Total(report, "Debit"));
        Assert.Equal(30, Total(report, "Credit"));
        Assert.Equal(120, Total(report, "ClosingBalance"));
        Assert.Equal(120, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Code == "C")).BalanceValue);
    }

    [Fact]
    public async Task Statements_UnknownPartner_GiveAnEmptyReportWithThePartnerList()
    {
        await SeedInvoiceWorld();

        var supplier = await _sut.BuildAsync(Request("SupplierStatement", 999));
        var customer = await _sut.BuildAsync(Request("CustomerStatement", 999));

        Assert.True(supplier.UsesPartner);
        Assert.Empty(supplier.Rows);
        Assert.Single(supplier.Partners);
        Assert.True(customer.UsesPartner);
        Assert.Empty(customer.Rows);
    }

    // ---------------------------------------------------------------- money safe

    [Fact]
    public async Task MoneySafe_PurchaseThenItsPaymentVoucher_IsCountedOnce()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 20));
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        var payments = new SupplierPaymentRepository(_db.Context, new JournalEntryRepository(_db.Context));
        await payments.UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bill.Id, Paid = 40 });
        _db.Context.ChangeTracker.Clear();

        var report = await _sut.BuildAsync(Request("MoneySafe"));

        Assert.Equal(20, Row(report, "Purchases"));
        Assert.Equal(40, Row(report, "SupplierPayments"));
        Assert.Equal(60, Total(report, "TotalOut"));
    }

    [Fact]
    public async Task MoneySafe_AddsEveryCashMovementOfThePeriod()
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var expense = new Expense { Code = "1", ArName = "E", EnName = "E" };
        var employee = new Employee { Code = "1", ArName = "Emp", EnName = "Emp" };
        _db.Context.AddRange(user, expense, employee);
        await _db.Context.SaveChangesAsync();
        var now = DateTime.Now;
        _db.Context.Deposits.Add(new Deposit { ProcessCode = "1", DeposerName = "Ali", Amount = 1000, CreatedTime = now, UserId = user.Id });
        _db.Context.ExitExpenses.Add(new ExitExpense { ExpenseId = expense.Id, Price = 120, Date = now, UserId = user.Id });
        _db.Context.Scrounges.Add(new Scrounge { EmployeeId = employee.Id, Amount = 80, State = MoneyState.Dept, Date = now, UserId = user.Id });
        _db.Context.Scrounges.Add(new Scrounge { EmployeeId = employee.Id, Amount = 500, State = MoneyState.Incentive, Date = now, UserId = user.Id });
        _db.Context.Salaries.Add(new Salary { SalaryCode = "1", EmployeeId = employee.Id, SalaryAmount = 700, SalaryDate = now, UserId = user.Id });
        await _db.Context.SaveChangesAsync();
        var request = Request("MoneySafe");
        request.FromDate = now.Date;
        request.ToDate = now.Date;

        var report = await _sut.BuildAsync(request);

        Assert.Equal(1000, Row(report, "Deposits"));
        Assert.Equal(120, Row(report, "Expenses"));
        Assert.Equal(80, Row(report, "EmployeeAdvances"));
        Assert.Equal(700, Row(report, "Salaries"));
        Assert.Equal(1000, Total(report, "TotalIn"));
        Assert.Equal(900, Total(report, "TotalOut"));
        Assert.Equal(100, Total(report, "MoneySafeBalance"));
    }

    [Fact]
    public async Task MoneySafe_SalesCountPaidOrders()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var walkIn = new Customer { Account = new Account { Code = "W", ArName = "Walk-in", EnName = "Walk-in", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(user, store, section, walkIn);
        await _db.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _db.Context.MenuItems.Add(dish);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        var orders = new OrderRepository(_db.Context);
        Order Take() => new() { Type = OrderType.TakeAway, CustomerId = walkIn.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } };
        await orders.CreateOrderAsync(Take(), user.Id);
        var request = Request("MoneySafe");
        request.FromDate = DateTime.Today;
        request.ToDate = DateTime.Today;
        _db.Context.ChangeTracker.Clear();

        Assert.Equal(40, Row(await _sut.BuildAsync(request), "Sales"));
    }

    [Fact]
    public async Task Profit_IsSalesMinusCostDiscountsPlusServiceMinusExpensesAndSalaries()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var flour = new Product { Code = "1", ArName = "F", EnName = "F", Cost = 4 };
        var expense = new Expense { Code = "1", ArName = "E", EnName = "E" };
        var table = new Table { Code = "1", ArName = "T1", EnName = "T1", NumOfPersons = 4 };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(user, store, section, flour, expense, table, customer);
        await _db.Context.SaveChangesAsync();
        var dish = new MenuItem
        {
            Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id,
            Components = new List<MenuItemComponent> { new() { ProductId = flour.Id, Quantity = 2 } }
        };
        _db.Context.MenuItems.Add(dish);
        _db.Context.ApplicationSettings.Add(new ApplicationSetting { Password = "x", OrderService = 5, OrderTax = 10 });
        await _db.Context.SaveChangesAsync();
        await new StockRepository(_db.Context).AdjustAsync(store.Id, flour.Id, 100);
        _db.Context.ChangeTracker.Clear();
        var orders = new OrderRepository(_db.Context);
        await orders.CreateOrderAsync(new Order { Type = OrderType.Hall, Cash = true, TableId = table.Id, CustomerId = customer.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 2 } } } }, user.Id);
        _db.Context.ExitExpenses.Add(new ExitExpense { ExpenseId = expense.Id, Price = 30, Date = DateTime.Now, UserId = user.Id });
        await _db.Context.SaveChangesAsync();
        var request = Request("Profit");
        request.FromDate = DateTime.Today;
        request.ToDate = DateTime.Today;

        var report = await _sut.BuildAsync(request);

        Assert.Equal(80, Row(report, "ItemsSales"));
        Assert.Equal(16, Row(report, "CostOfGoods"));
        Assert.Equal(5, Row(report, "Service"));
        Assert.Equal(80 - 16 + 5, Row(report, "GrossProfit"));
        Assert.Equal(80 - 16 + 5 - 30, Total(report, "NetProfit"));
    }

    // ---------------------------------------------------------------- list reports

    [Fact]
    public async Task ListReports_AllRunOnAnEmptyDatabase_WithColumnsAndNoRows()
    {
        foreach (var name in new[] { "MoneySafe", "Profit", "Orders", "Drivers", "Invoices", "SupplierStatement", "CustomerStatement", "SuppliersBalances", "SupplierPayments", "CustomerPayments", "Deposits", "Expenses", "Salaries", "Scrounges" })
        {
            var report = await _sut.BuildAsync(Request(name));

            Assert.Equal(name, report.Report);
            Assert.NotEmpty(report.Columns);
        }
    }

    [Fact]
    public async Task ListReports_DatesAreInclusive_AndAReversedRangeBecomesOneDay()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var employee = new Employee { Code = "1", ArName = "Emp", EnName = "Emp" };
        _db.Context.AddRange(user, employee);
        await _db.Context.SaveChangesAsync();
        _db.Context.Scrounges.AddRange(
            new Scrounge { EmployeeId = employee.Id, Amount = 10, State = MoneyState.Dept, Date = new DateTime(2030, 3, 10, 23, 59, 0), UserId = user.Id },
            new Scrounge { EmployeeId = employee.Id, Amount = 20, State = MoneyState.Incentive, Date = new DateTime(2030, 3, 11, 0, 1, 0), UserId = user.Id });
        await _db.Context.SaveChangesAsync();

        var day = await _sut.BuildAsync(new ReportRequest { Report = "Scrounges", FromDate = new DateTime(2030, 3, 10), ToDate = new DateTime(2030, 3, 10) });
        var reversed = await _sut.BuildAsync(new ReportRequest { Report = "Scrounges", FromDate = new DateTime(2030, 3, 10), ToDate = new DateTime(2030, 3, 1) });
        var byType = await _sut.BuildAsync(new ReportRequest { Report = "Scrounges", FromDate = new DateTime(2030, 3, 1), ToDate = new DateTime(2030, 3, 31), Type = "Incentive" });

        Assert.Equal(10, Total(day, "Amount"));
        Assert.Equal(new DateTime(2030, 3, 10), reversed.ToDate);
        Assert.Equal(10, Total(reversed, "Amount"));
        Assert.Equal(20, Total(byType, "Amount"));
    }

    [Fact]
    public async Task InvoicesReport_FiltersByTypeAndIgnoresCashierOrders()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 0));
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Sales, store, product, unit, customer.Id, 2, 20, 40));
        var request = Request("Invoices");
        request.FromDate = DateTime.Today;
        request.ToDate = DateTime.Today;

        var all = await _sut.BuildAsync(request);
        request.Type = "Sales";
        var sales = await _sut.BuildAsync(request);

        Assert.Equal(2, Total(all, "Count"));
        Assert.Equal(140, Total(all, "NetTotal"));
        Assert.Equal(1, Total(sales, "Count"));
        Assert.Equal(40, Total(sales, "Paid"));
        Assert.True(sales.UsesType);
    }

    [Fact]
    public async Task PaymentAndDepositReports_ListAndSum()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 0));
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Sales, store, product, unit, customer.Id, 2, 20, 0));
        var bills = await _db.Context.BillMasters.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var journals = new JournalEntryRepository(_db.Context);
        await new SupplierPaymentRepository(_db.Context, journals).UpsertPaymentAsync(new PaymentInvoiceSupplier { SupplierId = supplier.Id, BillMasterId = bills[0].Id, Paid = 25 });
        await new CustomerPaymentRepository(_db.Context, journals).UpsertPaymentAsync(new PaymentInvoiceCustomer { CustomerId = customer.Id, BillMasterId = bills[1].Id, Paid = 15 });
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();
        _db.Context.Deposits.Add(new Deposit { ProcessCode = "1", DeposerName = "Ali", Amount = 300, UserId = user.Id });
        _db.Context.Deposits.Add(new Deposit { ProcessCode = "2", DeposerName = "Gone", Amount = 999, IsDeleted = true, UserId = user.Id });
        await _db.Context.SaveChangesAsync();
        var request = Request("SupplierPayments");
        request.FromDate = DateTime.Today;
        request.ToDate = DateTime.Today;

        Assert.Equal(25, Total(await _sut.BuildAsync(request), "Paid"));
        request.Report = "CustomerPayments";
        Assert.Equal(15, Total(await _sut.BuildAsync(request), "Paid"));
        request.Report = "Deposits";
        Assert.Equal(300, Total(await _sut.BuildAsync(request), "Amount"));
    }

    // ---------------------------------------------------------------- returns, earlier activity and the lists with real rows

    private async Task MoveBillsBeforeThePeriod()
    {
        foreach (var bill in await _db.Context.BillMasters.ToListAsync())
        {
            bill.Date = _day.AddDays(-60);
        }

        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SupplierStatement_PurchasesAndTheirReturns_CloseAtTheBalanceOfTheAccount(bool earlier)
    {
        var (store, product, unit, supplier, _) = await SeedInvoiceWorld();
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 10, 10, 0))).Success);
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Returns, store, product, unit, supplier.Id, 3, 10, 0))).Success);
        _db.Context.ChangeTracker.Clear();
        if (earlier)
        {
            await MoveBillsBeforeThePeriod();
        }

        var request = Request("SupplierStatement", supplier.Id);
        request.FromDate = _day.AddDays(-5);
        request.ToDate = _day.AddDays(5);
        var report = await _sut.BuildAsync(request);

        var account = (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Code == "S")).BalanceValue;
        Assert.Equal(account, Total(report, "ClosingBalance"));
        Assert.Equal(earlier ? account : 0, Total(report, "OpeningBalance"));
        // the opening row, then one row per invoice of the period
        Assert.Equal(earlier ? 1 : 3, report.Rows.Count);

        var balances = await _sut.BuildAsync(new ReportRequest { Report = "SuppliersBalances", FromDate = request.FromDate, ToDate = request.ToDate });
        Assert.Equal(account, Total(balances, "ClosingBalance"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CustomerStatement_SalesAndTheirReturns_CloseAtTheBalanceOfTheAccount(bool earlier)
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceWorld();
        await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, store, product, unit, supplier.Id, 50, 1, 50));
        _db.Context.ChangeTracker.Clear();
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.Sales, store, product, unit, customer.Id, 10, 20, 0))).Success);
        Assert.True((await Invoices().UpsertInvoiceAsync(Invoice(InvoiceType.SalesReturn, store, product, unit, customer.Id, 2, 20, 0))).Success);
        _db.Context.ChangeTracker.Clear();
        if (earlier)
        {
            await MoveBillsBeforeThePeriod();
        }

        var request = Request("CustomerStatement", customer.Id);
        request.FromDate = _day.AddDays(-5);
        request.ToDate = _day.AddDays(5);
        var report = await _sut.BuildAsync(request);

        var account = (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Code == "C")).BalanceValue;
        Assert.Equal(account, Total(report, "ClosingBalance"));
        Assert.Equal(earlier ? account : 0, Total(report, "OpeningBalance"));
    }

    private async Task<(string user, Employee employee, Expense expense, MenuItem dish)> SeedMoney()
    {
        var user = new ApplicationUser { UserName = "boss", Email = "b@test.com", FirstName = "Boss", LastName = "B" };
        var driverJob = new Job { Code = "1", ArName = "طيار", EnName = "Driver" };
        var employee = new Employee { Code = "1", ArName = "Driver", EnName = "Driver", BasicSalary = 3000, Job = driverJob };
        var expense = new Expense { Code = "1", ArName = "Rent", EnName = "Rent", Price = 100 };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _db.Context.AddRange(user, employee, expense, store, section);
        await _db.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _db.Context.MenuItems.Add(dish);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        return (user.Id, employee, expense, dish);
    }

    [Fact]
    public async Task OrdersAndDriversReports_ListTheOrdersOfThePeriodWithTheirTotals()
    {
        var (user, driver, _, dish) = await SeedMoney();
        var orders = new OrderRepository(_db.Context);
        var zone = new DeliveryZone { Code = "1", ArName = "Z", EnName = "Z", Fee = 10 };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(zone, customer);
        await _db.Context.SaveChangesAsync();
        Assert.True((await orders.CreateOrderAsync(new Order { Type = OrderType.TakeAway, CustomerId = customer.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 2 } } } }, user)).Success);
        Assert.True((await orders.CreateOrderAsync(new Order
        {
            Type = OrderType.Delivery, EmployeeId = driver.Id, DeliveryZoneId = zone.Id, CustomerId = customer.Id, CustomerName = "Ali", PhoneNumber = "0100", Address = "Road",
            Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } }
        }, user)).Success);
        _db.Context.ChangeTracker.Clear();

        var all = await _sut.BuildAsync(Request("Orders"));
        var drivers = await _sut.BuildAsync(Request("Drivers", driver.Id));

        Assert.Equal(2, Total(all, "Count"));
        Assert.Equal(2, all.Rows.Count);
        Assert.True(Total(all, "TotalPrice") > 0);
        Assert.Equal(1, Total(drivers, "Count"));
        Assert.True(drivers.UsesPartner);
        Assert.Single(drivers.Partners);
    }

    [Fact]
    public async Task ExpenseSalaryAndScroungeReports_ListTheirRowsAndSumThem()
    {
        var (user, employee, expense, _) = await SeedMoney();
        var journals = new JournalEntryRepository(_db.Context);
        Assert.True((await new ExitExpenseRepository(_db.Context, journals).UpsertExitExpenseAsync(new ExitExpense { ExpenseId = expense.Id, Price = 80 }, user)).Success);
        Assert.True((await new SalaryRepository(_db.Context, journals).UpsertSalaryAsync(new Salary { EmployeeId = employee.Id, SalaryAmount = 2500 }, user)).Success);
        Assert.True((await new ScroungeRepository(_db.Context, journals).UpsertScroungeAsync(new Scrounge { EmployeeId = employee.Id, Amount = 200, State = MoneyState.Dept, Info = "advance" }, user)).Success);
        _db.Context.ChangeTracker.Clear();

        var expenses = await _sut.BuildAsync(Request("Expenses"));
        var salaries = await _sut.BuildAsync(Request("Salaries"));
        var scrounges = await _sut.BuildAsync(Request("Scrounges"));

        Assert.Equal(80, Total(expenses, "Price"));
        Assert.Single(expenses.Rows);
        Assert.Equal(2500, Total(salaries, "SalaryAmount"));
        Assert.Single(salaries.Rows);
        Assert.Single(scrounges.Rows);
    }

}

internal static class InvoiceTestExtensions
{
    // helper for the customer statement test: a purchase needs a supplier so the shop has goods to sell
    public static InvoiceViewModel WithSupplier(this InvoiceViewModel model, Restaurant.DAL.ApplicationDbContext context)
    {
        model.SupplierId = context.Suppliers.AsNoTracking().Select(a => (int?)a.Id).First();
        return model;
    }
}
