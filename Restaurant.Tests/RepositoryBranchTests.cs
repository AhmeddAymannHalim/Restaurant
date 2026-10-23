using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// Branches the broad theories do not reach: "used elsewhere" refusals, unknown records on edit, uploaded images that are replaced
// or removed, identity refusals, bill types and what posting a cashier order does when a step half way fails.
[Collection("global-state")]
public class RepositoryBranchTests : IDisposable
{
    private readonly TestHost _host = new();
    private readonly string _oldFolder = Environment.CurrentDirectory;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "repo-branches-" + Guid.NewGuid().ToString("N"));

    public RepositoryBranchTests()
    {
        Directory.CreateDirectory(_folder);
        Environment.CurrentDirectory = _folder;
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _oldFolder;
        _host.Dispose();
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // a locked temp file is not a test failure
        }
    }

    private ApplicationDbContext Db => _host.Context;
    private IHttpContextAccessor Http => _host.Db.Http;

    private static IFormFile Image(string name = "pic.png")
    {
        var bytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3, 4 };

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "f", name) { Headers = new HeaderDictionary(), ContentType = "image/png" };
    }

    private static string DiskPath(string url) => Path.Combine(Environment.CurrentDirectory, "wwwroot", url.TrimStart('/'));

    private async Task<Job> SeedJob(string name = "Cook")
    {
        var job = new Job { Code = name, ArName = name, EnName = name };
        Db.Jobs.Add(job);
        await Db.SaveChangesAsync();

        return job;
    }

    private static Employee NewEmployee(Job job, string name = "Omar", int? branchId = null) => new()
    {
        ArName = name, EnName = name, JobId = job.Id, BranchId = branchId, PhoneNumber = "0100000000" + name.Length, BasicSalary = 5000, WorkDays = 6, DailyHours = 8
    };

    // ---------------------------------------------------------------- "used elsewhere" refusals of the definitions

    [Fact]
    public async Task Job_Delete_LooksAtTheEmployeesOfThatJob_NotAtAnEmployeeWithTheSameNumber()
    {
        var unused = await SeedJob("Unused");   // job 1
        var used = await SeedJob("Used");       // job 2
        var employees = new EmployeeRepository(Db, Http);
        Assert.True((await employees.UpsertEmployeeAsync(NewEmployee(used), null, null)).Success);   // employee 1, the same number as the unused job
        var jobs = new JobRepository(Db, Http);

        var refused = await jobs.DeleteJobAsync(used.Id);
        var allowed = await jobs.DeleteJobAsync(unused.Id);

        Assert.Equal("CannotDeleteLinkedRecord", refused.Message);
        Assert.True(allowed.Success, allowed.Message);
        Db.ChangeTracker.Clear();
        Assert.Single(await Db.Jobs.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Branch_Delete_IsRefusedWhileItHasEmployees()
    {
        var branches = new BranchRepository(Db, Http);
        var branch = new CompanyBranch { ArName = "Main", EnName = "Main" };
        Assert.True((await branches.UpsertBranchAsync(branch)).Success);
        var job = await SeedJob();
        await new EmployeeRepository(Db, Http).UpsertEmployeeAsync(NewEmployee(job, "Omar", branch.Id), null, null);
        Db.ChangeTracker.Clear();

        var result = await branches.DeleteBranchAsync(branch.Id);

        Assert.Equal("BranchHasEmployees", result.Message);
    }

    [Fact]
    public async Task Store_Delete_IsRefusedWhileItHoldsStock_AndAllowedOnceItIsEmpty()
    {
        var stores = new StoreRepository(Db, Http);
        var store = new Store { ArName = "Main", EnName = "Main" };
        Assert.True((await stores.UpsertStoreAsync(store)).Success);
        var product = new Product { Code = "1", ArName = "Flour", EnName = "Flour", Cost = 1 };
        Db.Products.Add(product);
        await Db.SaveChangesAsync();
        var stock = new StockRepository(Db);
        await stock.AdjustAsync(store.Id, product.Id, 5);
        Db.ChangeTracker.Clear();

        Assert.Equal("CannotDeleteHasStock", (await stores.DeleteStoreAsync(store.Id)).Message);

        await stock.AdjustAsync(store.Id, product.Id, -5);
        Db.ChangeTracker.Clear();
        Assert.True((await stores.DeleteStoreAsync(store.Id)).Success);
    }

    // ---------------------------------------------------------------- editing a record that is gone

    [Fact]
    public async Task Salary_AndScrounge_EditOfAnUnknownRecord_IsRefused()
    {
        var journals = _host.Get<Restaurant.DAL.IRepositories.Commons.IJournalEntryRepository>();

        var salary = await new SalaryRepository(Db, journals).UpsertSalaryAsync(new Salary { Id = 404 }, "user-1");
        var scrounge = await new ScroungeRepository(Db, journals).UpsertScroungeAsync(new Scrounge { Id = 404 }, "user-1");

        Assert.Equal("RecordNotFound", salary.Message);
        Assert.Equal("RecordNotFound", scrounge.Message);
    }

    // ---------------------------------------------------------------- uploaded images

    private async Task<(MenuSection section, Store store, Product product)> SeedMenu()
    {
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var product = new Product { Code = "1", ArName = "Flour", EnName = "Flour", Cost = 1 };
        Db.AddRange(section, store, product);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        return (section, store, product);
    }

    private static MenuItem Dish(MenuSection section, Store store, Product product, string name = "Bread") => new()
    {
        ArName = name, EnName = name, Price = 10, MenuSectionId = section.Id, StoreId = store.Id,
        Components = new List<MenuItemComponent> { new() { ProductId = product.Id, Quantity = 1 } }
    };

    [Fact]
    public async Task Menu_Image_IsStoredOnCreate_ReplacedOnEdit_AndTheOldFileIsDeleted()
    {
        var (section, store, product) = await SeedMenu();
        var menu = new MenuItemRepository(Db, Http);
        var dish = Dish(section, store, product);

        Assert.True((await menu.UpsertMenuItemAsync(dish, Image("first.png"))).Success);
        Db.ChangeTracker.Clear();
        var first = (await Db.MenuItems.AsNoTracking().SingleAsync()).ImageUrl;
        Assert.True(File.Exists(DiskPath(first)));

        var edit = Dish(section, store, product);
        edit.Id = dish.Id;
        Assert.True((await menu.UpsertMenuItemAsync(edit, Image("second.png"))).Success);

        Db.ChangeTracker.Clear();
        var second = (await Db.MenuItems.AsNoTracking().SingleAsync()).ImageUrl;
        Assert.NotEqual(first, second);
        Assert.False(File.Exists(DiskPath(first)));
        Assert.True(File.Exists(DiskPath(second)));

        // an edit without a new image keeps the one it has
        Db.ChangeTracker.Clear();
        var keep = Dish(section, store, product);
        keep.Id = dish.Id;
        Assert.True((await menu.UpsertMenuItemAsync(keep, null)).Success);
        Db.ChangeTracker.Clear();
        Assert.Equal(second, (await Db.MenuItems.AsNoTracking().SingleAsync()).ImageUrl);
    }

    [Fact]
    public async Task Employee_Image_IsStoredOnCreate_ReplacedOnEdit_AndRemovedWithTheEmployee()
    {
        var job = await SeedJob();
        var employees = new EmployeeRepository(Db, Http);
        var employee = NewEmployee(job);

        Assert.True((await employees.UpsertEmployeeAsync(employee, Image("first.png"), null)).Success);
        Db.ChangeTracker.Clear();
        var first = (await Db.EmployeeImages.AsNoTracking().SingleAsync()).Url;
        Assert.True(File.Exists(DiskPath(first)));

        var edit = NewEmployee(job);
        edit.Id = employee.Id;
        Assert.True((await employees.UpsertEmployeeAsync(edit, Image("second.png"), null)).Success);

        Db.ChangeTracker.Clear();
        var second = (await Db.EmployeeImages.AsNoTracking().SingleAsync()).Url;
        Assert.NotEqual(first, second);
        Assert.False(File.Exists(DiskPath(first)));
        Assert.True(File.Exists(DiskPath(second)));

        Db.ChangeTracker.Clear();
        Assert.True((await employees.DeleteEmployeeAsync(employee.Id)).Success);
        Assert.False(File.Exists(DiskPath(second)));
    }

    [Fact]
    public async Task Employee_EditAddsAnImageToAnEmployeeThatHadNone()
    {
        var job = await SeedJob();
        var employees = new EmployeeRepository(Db, Http);
        var employee = NewEmployee(job);
        await employees.UpsertEmployeeAsync(employee, null, null);
        Db.ChangeTracker.Clear();

        var edit = NewEmployee(job);
        edit.Id = employee.Id;
        Assert.True((await employees.UpsertEmployeeAsync(edit, Image(), null)).Success);

        Db.ChangeTracker.Clear();
        Assert.Single(await Db.EmployeeImages.AsNoTracking().ToListAsync());
    }

    // ---------------------------------------------------------------- identity refusals

    [Fact]
    public async Task SignIn_AnUnconfirmedCustomer_IsToldToConfirmFirst_AStaffUserIsNot()
    {
        var accounts = _host.Get<IAccountRepository>();
        var users = _host.Get<UserManager<ApplicationUser>>();
        var roles = _host.Get<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole("User"));
        var customer = new ApplicationUser { UserName = "c@test.com", Email = "c@test.com", FirstName = "C", EmailConfirmed = false };
        await users.CreateAsync(customer, "Pass123");
        await users.AddToRoleAsync(customer, "User");
        var staff = new ApplicationUser { UserName = "s@test.com", Email = "s@test.com", FirstName = "S", EmailConfirmed = false };
        await users.CreateAsync(staff, "Pass123");

        var customerResult = await accounts.CheckPasswordAsync("c@test.com", "Pass123");
        var staffResult = await accounts.CheckPasswordAsync("s@test.com", "Pass123");

        Assert.False(customerResult.Success);
        Assert.Equal("Auth_ConfirmFirst", customerResult.Message);
        Assert.True(staffResult.Success);
    }

    [Fact]
    public async Task ResetPassword_ConfirmsTheEmailOfAnAccountThatWasNotConfirmed()
    {
        var accounts = _host.Get<IAccountRepository>();
        var users = _host.Get<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "u@test.com", Email = "u@test.com", FirstName = "U", EmailConfirmed = false };
        await users.CreateAsync(user, "Pass123");
        var token = await accounts.GeneratePasswordResetTokenAsync(user);

        var result = await accounts.ResetPasswordAsync("u@test.com", token, "Another123");

        Assert.True(result.Success, result.Message);
        Db.ChangeTracker.Clear();
        Assert.True((await users.FindByEmailAsync("u@test.com")).EmailConfirmed);
    }

    [Fact]
    public async Task EmployeeAccount_ANameAlreadyTaken_OrAnUpdateIdentityRefuses_IsReportedWithItsCode()
    {
        var job = await SeedJob("Cashier");
        var employees = new EmployeeRepository(Db, Http);
        var first = NewEmployee(job, "Ali");
        var second = NewEmployee(job, "Omar");
        var firstSaved = await employees.UpsertEmployeeAsync(first, null, null);
        var secondSaved = await employees.UpsertEmployeeAsync(second, null, null);
        Assert.True(firstSaved.Success, firstSaved.Message);
        Assert.True(secondSaved.Success, secondSaved.Message);
        var accounts = _host.Get<IEmployeeAccountRepository>();
        Db.ChangeTracker.Clear();

        // a new login has no id yet (a new IdentityUser would otherwise carry a random one)
        var created = await accounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "ali", Email = "ali@test.com" }, first.Id, "Pass123");
        var duplicate = await accounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "ali", Email = "omar@test.com" }, second.Id, "Pass123");

        Assert.True(created.Success, created.Message);
        Assert.False(duplicate.Success);
        Assert.StartsWith("Identity.", duplicate.Message);

        // a clash on update, and an update that leaves the password alone
        var users = _host.Get<UserManager<ApplicationUser>>();
        var ali = await users.FindByNameAsync("ali");
        Db.ChangeTracker.Clear();
        var omarCreated = await accounts.UpsertAccountAsync(new ApplicationUser { Id = null, UserName = "omar", Email = "omar@test.com" }, second.Id, "Pass123");
        Assert.True(omarCreated.Success, omarCreated.Message);
        Db.ChangeTracker.Clear();
        var clash = await accounts.UpsertAccountAsync(new ApplicationUser { Id = ali.Id, UserName = "omar", Email = "ali@test.com" }, first.Id, null);
        Assert.False(clash.Success);
        Assert.StartsWith("Identity.", clash.Message);
        Db.ChangeTracker.Clear();
        var renamed = await accounts.UpsertAccountAsync(new ApplicationUser { Id = ali.Id, UserName = "ali2", Email = "ali@test.com" }, first.Id, null);
        Assert.True(renamed.Success, renamed.Message);
    }

    // ---------------------------------------------------------------- bills

    private async Task<(Store store, Product product, UnitPrice unit, Supplier supplier, Customer customer)> SeedInvoiceData()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        var product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        Db.AddRange(store, product, unit, supplier, customer);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        return (store, product, unit, supplier, customer);
    }

    [Fact]
    public async Task BillList_EveryInvoiceTypeHasItsSettingCode_AndUnpaidOnlyKeepsWhatIsStillOwed()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceData();
        var invoices = _host.Get<IInvoiceService>();
        await _host.Get<Restaurant.DAL.IRepositories.Definitions.IStockRepository>().AdjustAsync(store.Id, product.Id, 100, allowNegative: true);
        Db.ChangeTracker.Clear();

        InvoiceViewModel Make(InvoiceType type, decimal paid) => new()
        {
            InvoiceType = type, StoreId = store.Id, Paid = paid, AutoPost = true, AutoStock = true,
            SupplierId = type is InvoiceType.Purchases or InvoiceType.Returns ? supplier.Id : null,
            CustomerId = type is InvoiceType.Sales or InvoiceType.SalesReturn ? customer.Id : null,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = product.Id, UnitPriceId = unit.Id, Quantity = 1, Price = 10 } }
        };

        foreach (var type in new[] { InvoiceType.Purchases, InvoiceType.Returns, InvoiceType.Sales, InvoiceType.SalesReturn, InvoiceType.Shortage })
        {
            // the first invoice is paid in full, the others are left owed
            var result = await invoices.UpsertInvoiceAsync(Make(type, type == InvoiceType.Purchases ? 10 : 0));
            Assert.True(result.Success, type + ": " + result.Message);
        }

        var bills = new BillRepository(Db);
        var all = await bills.GetPagedBillsAsync(new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1), PageSize = 50 });
        var codes = all.Data.Select(a => a.SettingCode).OrderBy(a => a).ToList();

        Assert.Equal(new[] { "OTHER", "PRET", "PUR", "SAL", "SRET" }, codes);
        foreach (var row in all.Data)
        {
            var details = await bills.GetBillByIdAsync(row.Id);
            Assert.Equal(row.SettingCode, details.SettingCode);
        }

        var unpaid = await bills.GetPagedBillsAsync(new PageRequest { UnpaidOnly = true, PageSize = 50 });
        Assert.Equal(3, unpaid.Data.Count);
        Assert.DoesNotContain(unpaid.Data, a => a.SettingCode == "PUR");
    }

    private async Task<(int orderId, Account sales, Account cash)> PostableOrder()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        Db.AddRange(user, store, section);
        await Db.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        Db.MenuItems.Add(dish);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var orders = new OrderRepository(Db);
        var created = await orders.CreateOrderAsync(new Order { Type = OrderType.TakeAway, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, user.Id);
        Assert.True(created.Success, created.Message);
        Db.ChangeTracker.Clear();

        var sales = new Account { Code = "SALES", ArName = "Sales", EnName = "Sales", BalanceType = AccountBalanceType.Credit };
        var cash = new Account { Code = "CASH", ArName = "Cash", EnName = "Cash", BalanceType = AccountBalanceType.Debit };
        Db.AddRange(sales, cash);
        await Db.SaveChangesAsync();
        var setting = await Db.BillSettings.SingleAsync();
        setting.AccountItemId = sales.Id;
        setting.AccountPayId = cash.Id;
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        return (int.Parse(created.Data), sales, cash);
    }

    [Fact]
    public async Task PostBill_AnOrderThatWasNotCollected_IsRefused_ACollectedOneIsPostedOnce()
    {
        var (orderId, _, _) = await PostableOrder();
        var bills = new BillRepository(Db);
        var order = await Db.Orders.SingleAsync(a => a.Id == orderId);
        order.Cash = false;
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        Assert.Equal("BillNotCollected", (await bills.PostBillAsync(orderId)).Message);

        order = await Db.Orders.SingleAsync(a => a.Id == orderId);
        order.Cash = true;
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        Assert.True((await bills.PostBillAsync(orderId)).Success);
        Db.ChangeTracker.Clear();
        Assert.Equal("BillAlreadyPosted", (await bills.PostBillAsync(orderId)).Message);
    }

    [Fact]
    public async Task PostPending_PostsEveryCollectedOrder_AndCountsThem()
    {
        var (first, _, _) = await PostableOrder();
        var bills = new BillRepository(Db);

        var result = await bills.PostPendingAsync();

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Db.ChangeTracker.Clear();
        Assert.Single(await Db.EntryMasters.AsNoTracking().Where(a => a.BillId == first).ToListAsync());
        Assert.Equal("0", (await bills.PostPendingAsync()).Data);
    }

    [Fact]
    public async Task PostBill_WhenAStepAfterTheCheckFails_RollsBackAndAnswersWithAFailure()
    {
        var (orderId, _, _) = await PostableOrder();
        var bills = new BillRepository(Db);
        // the order read and the "already posted" check run first, then the transaction starts and the next read fails
        _host.Db.QueryFailure.SkipFirst = 2;
        _host.Db.QueryFailure.Failure = new InvalidOperationException("forced");

        var result = await bills.PostBillAsync(orderId);

        _host.Db.QueryFailure.Failure = null;
        Assert.False(result.Success);
        Db.ChangeTracker.Clear();
        Assert.Empty(await Db.EntryMasters.AsNoTracking().ToListAsync());
    }
}
