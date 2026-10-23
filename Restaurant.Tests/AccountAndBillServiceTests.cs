using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The account service (register, sign in, tokens, passwords, profile) and the bill service (post one bill, post everything pending)
// through the real mapper, repositories and database.
public class AccountAndBillServiceTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private IAccountService Accounts => _host.Get<IAccountService>();
    private IBillService Bills => _host.Get<IBillService>();
    private IInvoiceService Invoices => _host.Get<IInvoiceService>();

    private async Task<ApplicationUser> ConfirmedUser(string email = "sara@test.com", string password = "Pass123")
    {
        var users = _host.Get<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, FirstName = "Sara", LastName = "A", EmailConfirmed = true };
        Assert.True((await users.CreateAsync(user, password)).Succeeded);

        return user;
    }

    // ---------------------------------------------------------------- account service

    [Fact]
    public async Task Register_ANewEmail_Succeeds_TheSameEmailAgainIsRefused()
    {
        var model = new RegisterViewModel { FirstName = "Sara", LastName = "A", Email = "sara@test.com", Password = "Pass123" };

        var first = await Accounts.RegisterAsync(model);
        var second = await Accounts.RegisterAsync(model);

        Assert.True(first.Success, first.Message);
        Assert.False(second.Success);
        Assert.False(string.IsNullOrWhiteSpace(second.Message));
    }

    [Fact]
    public async Task SignIn_RightPassword_GivesTheIdentityWithItsRoles_WrongPasswordOrUnknownEmailIsRefused()
    {
        var user = await ConfirmedUser();
        await _host.Get<RoleManager<IdentityRole>>().CreateAsync(new IdentityRole("Admin"));
        await _host.Get<UserManager<ApplicationUser>>().AddToRoleAsync(user, "Admin");

        var good = await Accounts.SignInAsync(new SignInViewModel { Email = "sara@test.com", Password = "Pass123" });
        var wrong = await Accounts.SignInAsync(new SignInViewModel { Email = "sara@test.com", Password = "Wrong123" });
        var unknown = await Accounts.SignInAsync(new SignInViewModel { Email = "nobody@test.com", Password = "Pass123" });

        Assert.True(good.Success, good.Message);
        Assert.Equal(user.Id, good.Data.Id);
        Assert.Equal("sara@test.com", good.Data.Email);
        Assert.Contains("Admin", good.Data.Roles);
        Assert.False(wrong.Success);
        Assert.False(unknown.Success);
    }

    [Fact]
    public async Task ConfirmationToken_OnlyForAnUnconfirmedAccount_AndConfirmingWorks()
    {
        var users = _host.Get<UserManager<ApplicationUser>>();
        var unconfirmed = new ApplicationUser { UserName = "new@test.com", Email = "new@test.com", FirstName = "New", EmailConfirmed = false };
        await users.CreateAsync(unconfirmed, "Pass123");
        await ConfirmedUser("done@test.com");

        Assert.Null(await Accounts.GetConfirmationTokenAsync("nobody@test.com"));
        Assert.Null(await Accounts.GetConfirmationTokenAsync("done@test.com"));
        var token = await Accounts.GetConfirmationTokenAsync("new@test.com");

        Assert.Equal(unconfirmed.Id, token.UserId);
        Assert.Equal("New", token.FirstName);
        Assert.NotEmpty(token.Token);
        var confirmed = await Accounts.ConfirmEmailAsync(token.UserId, token.Token);
        Assert.True(confirmed.Success, confirmed.Message);
        Assert.Null(await Accounts.GetConfirmationTokenAsync("new@test.com"));
    }

    [Fact]
    public async Task PasswordReset_UnknownEmailGetsNoToken_AKnownOneCanResetItsPassword()
    {
        await ConfirmedUser();

        Assert.Null(await Accounts.GetPasswordResetTokenAsync("nobody@test.com"));
        var token = await Accounts.GetPasswordResetTokenAsync("sara@test.com");
        var reset = await Accounts.ResetPasswordAsync(new ResetPasswordViewModel { Email = "sara@test.com", Token = token.Token, Password = "Another123", ConfirmPassword = "Another123" });

        Assert.True(reset.Success, reset.Message);
        Assert.True((await Accounts.SignInAsync(new SignInViewModel { Email = "sara@test.com", Password = "Another123" })).Success);
        Assert.False((await Accounts.SignInAsync(new SignInViewModel { Email = "sara@test.com", Password = "Pass123" })).Success);
    }

    [Fact]
    public async Task ChangePassword_NeedsTheCurrentOne()
    {
        var user = await ConfirmedUser();

        var refused = await Accounts.ChangePasswordAsync(user.Id, new ChangePasswordViewModel { CurrentPassword = "Wrong123", NewPassword = "Another123", ConfirmPassword = "Another123" });
        var changed = await Accounts.ChangePasswordAsync(user.Id, new ChangePasswordViewModel { CurrentPassword = "Pass123", NewPassword = "Another123", ConfirmPassword = "Another123" });

        Assert.False(refused.Success);
        Assert.True(changed.Success, changed.Message);
    }

    [Fact]
    public async Task Profile_UnknownUserIsNull_AKnownOneCarriesItsTownCountryAndGovernorate()
    {
        var country = new Country { ArName = "مصر", EnName = "Egypt" };
        var governorate = new Governorate { ArName = "القاهرة", EnName = "Cairo", Country = country };
        var town = new Town { ArName = "المعادي", EnName = "Maadi", Governorate = governorate };
        _host.Context.AddRange(country, governorate, town);
        await _host.Context.SaveChangesAsync();
        var user = await ConfirmedUser();
        user.TownId = town.Id;
        user.Street = "Road 9";
        user.PhoneNumber = "0100";
        await _host.Get<UserManager<ApplicationUser>>().UpdateAsync(user);
        _host.Context.ChangeTracker.Clear();

        Assert.Null(await Accounts.GetProfileAsync("no-such-user"));
        var profile = await Accounts.GetProfileAsync(user.Id);

        Assert.Equal("Sara", profile.FirstName);
        Assert.Equal("Road 9", profile.Street);
        Assert.Equal("Maadi", profile.TownEnName);
        Assert.Equal(governorate.Id, profile.GovernorateId);
        Assert.Equal(country.Id, profile.CountryId);
    }

    // ---------------------------------------------------------------- bill service: post what is pending

    private async Task<(Store store, Product product, UnitPrice unit, Supplier supplier, Customer customer)> SeedInvoiceData()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        var product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        var unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _host.Context.AddRange(store, product, unit, supplier, customer);
        await _host.Context.SaveChangesAsync();
        _host.Context.ChangeTracker.Clear();

        return (store, product, unit, supplier, customer);
    }

    private static InvoiceViewModel Unposted(InvoiceType type, Store store, Product product, UnitPrice unit, Supplier supplier, Customer customer, decimal quantity, decimal price) => new()
    {
        InvoiceType = type, StoreId = store.Id, AutoPost = false, AutoStock = false,
        SupplierId = type == InvoiceType.Purchases ? supplier.Id : null,
        CustomerId = type == InvoiceType.Sales ? customer.Id : null,
        Items = new List<InvoiceItemViewModel> { new() { ProductId = product.Id, UnitPriceId = unit.Id, Quantity = quantity, Price = price } }
    };

    [Fact]
    public async Task PostPending_PostsEveryUnpostedInvoice_AndASecondRunHasNothingLeft()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceData();
        await Invoices.UpsertInvoiceAsync(Unposted(InvoiceType.Purchases, store, product, unit, supplier, customer, 10, 7));
        await Invoices.UpsertInvoiceAsync(Unposted(InvoiceType.Purchases, store, product, unit, supplier, customer, 5, 7));

        var first = await Bills.PostPendingAsync();
        var second = await Bills.PostPendingAsync();

        Assert.True(first.Success, first.Message);
        Assert.Equal("2", first.Data);
        Assert.True(second.Success, second.Message);
        Assert.Equal("0", second.Data);
        var stock = await new Restaurant.DAL.Repositories.Definitions.StockRepository(_host.Context).FindAsync(store.Id, product.Id);
        Assert.Equal(15, stock.Quantity);
    }

    [Fact]
    public async Task PostPending_StopsAtTheFirstInvoiceThatCannotBePosted_AndSaysHowManyWentThrough()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceData();
        await Invoices.UpsertInvoiceAsync(Unposted(InvoiceType.Purchases, store, product, unit, supplier, customer, 5, 7));
        await Invoices.UpsertInvoiceAsync(Unposted(InvoiceType.Sales, store, product, unit, supplier, customer, 10, 20));

        var result = await Bills.PostPendingAsync();

        Assert.False(result.Success);
        Assert.Equal("InsufficientStock", result.Message);
        Assert.Equal("1", result.Data);
    }

    [Fact]
    public async Task PostBill_AnInvoiceIsPostedThroughTheInvoices_AnUnknownIdIsRefused()
    {
        var (store, product, unit, supplier, customer) = await SeedInvoiceData();
        await Invoices.UpsertInvoiceAsync(Unposted(InvoiceType.Purchases, store, product, unit, supplier, customer, 10, 7));
        var id = (await _host.Context.BillMasters.AsNoTracking().SingleAsync()).Id;

        var posted = await Bills.PostBillAsync(id);
        var again = await Bills.PostBillAsync(id);
        var unknown = await Bills.PostBillAsync(404);

        Assert.True(posted.Success, posted.Message);
        Assert.Equal("BillAlreadyPosted", again.Message);
        Assert.False(unknown.Success);
    }
}
