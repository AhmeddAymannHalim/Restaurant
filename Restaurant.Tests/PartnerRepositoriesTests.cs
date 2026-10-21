using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// Customers and suppliers: each one owns an account, keeps its phones and posts an opening balance.
public class PartnerRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly CustomerRepository _customers;
    private readonly SupplierRepository _suppliers;

    public PartnerRepositoriesTests()
    {
        _customers = new CustomerRepository(_db.Context, _db.Http);
        _suppliers = new SupplierRepository(_db.Context, _db.Http);
    }

    public void Dispose() => _db.Dispose();

    private static Customer NewCustomer(string name = "Ali", decimal debit = 0, decimal credit = 0) => new()
    {
        PhoneNumber1 = "01000000001",
        Account = new Account { ArName = name, EnName = name, DebitBalance = debit, CreditBalance = credit }
    };

    private static Supplier NewSupplier(string name = "Sup", decimal debit = 0, decimal credit = 0) => new()
    {
        PhoneNumber1 = "01000000002",
        Account = new Account { ArName = name, EnName = name, DebitBalance = debit, CreditBalance = credit }
    };

    private async Task<Account> AccountOf(int accountId) => await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == accountId);

    // ---------------------------------------------------------------- customers

    [Fact]
    public async Task Customer_Create_BuildsACustomerAccountWithTheOpeningBalance()
    {
        var customer = NewCustomer(debit: 500);

        var result = await _customers.UpsertCustomerAsync(customer, null);

        Assert.True(result.Success, result.Message);
        var saved = await _db.Context.Customers.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal(AccountType.Customer, saved.Account.AccountType);
        Assert.Equal(AccountBalanceType.Debit, saved.Account.BalanceType);
        Assert.Equal(500, saved.Account.BalanceValue);
        Assert.Equal("1", saved.Account.Code);
        Assert.NotNull(saved.CreatedAt);
    }

    [Fact]
    public async Task Customer_Create_NegativeBalance_IsRejected()
    {
        var result = await _customers.UpsertCustomerAsync(NewCustomer(debit: -5), null);

        Assert.Equal("NegativeAmount", result.Message);
        Assert.Empty(_db.Context.Customers);
        Assert.Empty(_db.Context.Accounts);
    }

    [Fact]
    public async Task Customer_Create_PostsAnOpeningEntryWhenTheMainAccountIsSet()
    {
        var main = new Account { Code = "OPEN", ArName = "Opening", EnName = "Opening", AccountType = AccountType.Normal, BalanceType = AccountBalanceType.Credit };
        _db.Context.EntrySettings.Add(new EntrySetting { EntryType = EntryType.CustomerOpeningBalance, ArName = "a", EnName = "b", Account = main });
        await _db.Context.SaveChangesAsync();

        await _customers.UpsertCustomerAsync(NewCustomer(debit: 500), null);

        var entry = await _db.Context.EntryMasters.AsNoTracking().Include(a => a.EntryDetails).SingleAsync();
        Assert.Equal(500, entry.EntryDetails.Sum(a => a.Debit));
        Assert.Equal(500, entry.EntryDetails.Sum(a => a.Credit));
        Assert.Equal(500, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Code == "OPEN")).BalanceValue);
    }

    [Fact]
    public async Task Customer_Edit_RenamesAndMovesTheBalanceByTheDifferenceOnly()
    {
        var customer = NewCustomer(debit: 500);
        await _customers.UpsertCustomerAsync(customer, null);
        _db.Context.ChangeTracker.Clear();

        var edit = NewCustomer("Ali 2", debit: 800);
        edit.Id = customer.Id;
        var result = await _customers.UpsertCustomerAsync(edit, null);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Customers.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal("Ali 2", saved.Account.EnName);
        Assert.Equal(800, saved.Account.BalanceValue);
        Assert.Single(_db.Context.Accounts);
    }

    [Fact]
    public async Task Customer_Edit_BalanceShownAsNetStaysTheSame()
    {
        var customer = NewCustomer(debit: 500);
        await _customers.UpsertCustomerAsync(customer, null);
        var account = await _db.Context.Accounts.SingleAsync();
        account.DebitBalance += 300;
        account.CreditBalance += 100;
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var edit = NewCustomer(debit: 700);
        edit.Id = customer.Id;
        await _customers.UpsertCustomerAsync(edit, null);

        _db.Context.ChangeTracker.Clear();
        Assert.Equal(700, (await _db.Context.Accounts.AsNoTracking().SingleAsync()).BalanceValue);
    }

    [Fact]
    public async Task Customer_Edit_Unknown_ReturnsRecordNotFound()
    {
        var edit = NewCustomer();
        edit.Id = 99;

        Assert.Equal("RecordNotFound", (await _customers.UpsertCustomerAsync(edit, null)).Message);
    }

    [Fact]
    public async Task Customer_Phones_AreCleanedAndSynced()
    {
        var customer = NewCustomer();
        customer.Phones = new List<CustomerPhone>
        {
            new() { PhoneNumber = " 01000000001 " }, // the main number again
            new() { PhoneNumber = "01111111111" },
            new() { PhoneNumber = "01111111111" },   // duplicate
            new() { PhoneNumber = "  " }
        };
        await _customers.UpsertCustomerAsync(customer, null);
        _db.Context.ChangeTracker.Clear();

        var saved = await _customers.GetCustomerByIdAsync(customer.Id);
        Assert.Equal("01111111111", saved.PhoneNumber2);
        Assert.Equal(new[] { "01111111111" }, saved.Phones.Select(a => a.PhoneNumber));

        _db.Context.ChangeTracker.Clear();
        var edit = NewCustomer();
        edit.Id = customer.Id;
        edit.Phones = new List<CustomerPhone> { new() { PhoneNumber = "01222222222" } };
        await _customers.UpsertCustomerAsync(edit, null);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(new[] { "01222222222" }, (await _customers.GetCustomerByIdAsync(customer.Id)).Phones.Select(a => a.PhoneNumber));
    }

    [Fact]
    public async Task Customer_Town_FillsGovernorateAndCountry()
    {
        var country = new Country { ArName = "مصر", EnName = "Egypt" };
        var gov = new Governorate { ArName = "القاهرة", EnName = "Cairo", Country = country };
        var town = new Town { ArName = "مدينة", EnName = "Town", Governorate = gov };
        _db.Context.Towns.Add(town);
        await _db.Context.SaveChangesAsync();
        var customer = NewCustomer();
        customer.TownId = town.Id;

        await _customers.UpsertCustomerAsync(customer, null);

        var saved = await _db.Context.Customers.AsNoTracking().SingleAsync();
        Assert.Equal(gov.Id, saved.GovId);
        Assert.Equal(country.Id, saved.CountryId);
    }

    [Fact]
    public async Task Customer_Delete_BlockedWhileHeOwesMoney_AllowedAtZero()
    {
        var customer = NewCustomer(debit: 100);
        await _customers.UpsertCustomerAsync(customer, null);

        Assert.Equal("LockedByBalance", (await _customers.DeleteCustomerAsync(customer.Id)).Message);
        Assert.Equal(100, await _customers.GetCustomerBalanceAsync(customer.Id));

        var zero = NewCustomer("Zero");
        await _customers.UpsertCustomerAsync(zero, null);
        Assert.True((await _customers.DeleteCustomerAsync(zero.Id)).Success);
        Assert.Single(_db.Context.Customers);
        Assert.Single(_db.Context.Accounts);
        Assert.True((await _customers.DeleteCustomerAsync(404)).Success);
        Assert.Null(await _customers.GetCustomerBalanceAsync(404));
    }

    [Fact]
    public async Task Customer_WithInvoices_CannotBeDeleted_AndOpenInvoicesAreCounted()
    {
        var customer = NewCustomer();
        await _customers.UpsertCustomerAsync(customer, null);
        var accountId = (await _db.Context.Customers.AsNoTracking().SingleAsync()).AccountId;
        _db.Context.BillMasters.AddRange(
            new BillMaster { Code = "1", Date = DateTime.Today, CustomerOrSupplierAccountId = accountId, TotalBillAmountAfterDiscountAndExtra = 100, AmountPaided = 40 },
            new BillMaster { Code = "2", Date = DateTime.Today, CustomerOrSupplierAccountId = accountId, TotalBillAmountAfterDiscountAndExtra = 50, AmountPaided = 50 });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Equal(1, await _customers.CountInvoicesAsync(customer.Id));
        Assert.Equal(0, await _customers.CountInvoicesAsync(404));
        Assert.Equal("CannotDeleteLinkedRecord", (await _customers.DeleteCustomerAsync(customer.Id)).Message);
        Assert.Single(_db.Context.Customers);
    }

    [Fact]
    public async Task Customer_Paging_SearchesByAccountNameAndCode()
    {
        await _customers.UpsertCustomerAsync(NewCustomer("Mohamed"), null);
        await _customers.UpsertCustomerAsync(NewCustomer("Sara"), null);

        Assert.Equal(2, (await _customers.GetPagedCustomersAsync(new PageRequest())).TotalRecords);
        Assert.Single((await _customers.GetPagedCustomersAsync(new PageRequest { SearchString = " MOHA " })).Data);
        Assert.Single((await _customers.GetPagedCustomersAsync(new PageRequest { SearchString = "2" })).Data);
    }

    // ---------------------------------------------------------------- suppliers

    [Fact]
    public async Task Supplier_Create_IsACreditAccountAndKeepsTheOpeningAsLastBalance()
    {
        var supplier = NewSupplier(credit: 300);

        var result = await _suppliers.UpsertSupplierAsync(supplier);

        Assert.True(result.Success, result.Message);
        var saved = await _db.Context.Suppliers.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal(AccountType.Supplier, saved.Account.AccountType);
        Assert.Equal(AccountBalanceType.Credit, saved.Account.BalanceType);
        Assert.Equal(300, saved.Account.BalanceValue);
        Assert.Equal(300, saved.LastBalance);
    }

    [Fact]
    public async Task Supplier_DebitOpening_MeansHeOwesUs()
    {
        await _suppliers.UpsertSupplierAsync(NewSupplier(debit: 120));

        Assert.Equal(-120, (await _db.Context.Accounts.AsNoTracking().SingleAsync()).BalanceValue);
    }

    [Fact]
    public async Task Supplier_Edit_MovesTheBalanceByTheDifference_AndKeepsLastBalance()
    {
        var supplier = NewSupplier(credit: 300);
        await _suppliers.UpsertSupplierAsync(supplier);
        _db.Context.ChangeTracker.Clear();

        var edit = NewSupplier("Sup 2", credit: 450);
        edit.Id = supplier.Id;
        var result = await _suppliers.UpsertSupplierAsync(edit);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Suppliers.AsNoTracking().Include(a => a.Account).SingleAsync();
        Assert.Equal(450, saved.Account.BalanceValue);
        Assert.Equal("Sup 2", saved.Account.EnName);
        Assert.Equal(300, saved.LastBalance);
    }

    [Fact]
    public async Task Supplier_NegativeUnknownAndDelete_Rules()
    {
        Assert.Equal("NegativeAmount", (await _suppliers.UpsertSupplierAsync(NewSupplier(credit: -1))).Message);
        var unknown = NewSupplier();
        unknown.Id = 99;
        Assert.Equal("RecordNotFound", (await _suppliers.UpsertSupplierAsync(unknown)).Message);

        var owed = NewSupplier(credit: 10);
        await _suppliers.UpsertSupplierAsync(owed);
        Assert.Equal("LockedByBalance", (await _suppliers.DeleteSupplierAsync(owed.Id)).Message);

        var zero = NewSupplier("Zero");
        await _suppliers.UpsertSupplierAsync(zero);
        Assert.True((await _suppliers.DeleteSupplierAsync(zero.Id)).Success);
        Assert.True((await _suppliers.DeleteSupplierAsync(404)).Success);
        Assert.Single(_db.Context.Suppliers);
    }

    [Fact]
    public async Task Supplier_PagingAndInvoiceCount()
    {
        var supplier = NewSupplier("Acme");
        await _suppliers.UpsertSupplierAsync(supplier);
        var accountId = (await _db.Context.Suppliers.AsNoTracking().SingleAsync()).AccountId;
        _db.Context.BillMasters.Add(new BillMaster { Code = "1", Date = DateTime.Today, CustomerOrSupplierAccountId = accountId, TotalBillAmountAfterDiscountAndExtra = 10, AmountPaided = 0 });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Single((await _suppliers.GetPagedSuppliersAsync(new PageRequest { SearchString = "acme" })).Data);
        Assert.Equal(1, await _suppliers.CountInvoicesAsync(supplier.Id));
        Assert.Equal("CannotDeleteLinkedRecord", (await _suppliers.DeleteSupplierAsync(supplier.Id)).Message);
    }

    // ---------------------------------------------------------------- phones

    [Fact]
    public void Phones_CleanNumbers_TrimsDedupesAndSkipsTheMainNumber()
    {
        var sut = new PhoneRepository(_db.Context);
        var rows = new List<CustomerPhone>
        {
            new() { PhoneNumber = " 111 " }, new() { PhoneNumber = "111" }, new() { PhoneNumber = "222" }, new() { PhoneNumber = null }, new() { PhoneNumber = "MAIN" }
        };

        Assert.Equal(new[] { "111", "222" }, sut.CleanNumbers(" MAIN ", rows));
        Assert.Empty(sut.CleanNumbers("1", null));
    }
}
