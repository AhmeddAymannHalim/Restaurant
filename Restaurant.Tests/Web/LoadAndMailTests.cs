using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ResturantPL.Controllers;
using ResturantPL.Controllers.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// E-mail sending, the printer settings save, a big data set and several users writing at the same time.
public class LoadAndMailTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    // ---------------------------------------------------------------- e-mail

    private async Task<AccountController> NewAccount()
    {
        var controller = _host.Controller<AccountController>();
        controller.Url = Mock.Of<IUrlHelper>(u => u.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>()) == "http://127.0.0.1/link");
        await controller.Register(new RegisterViewModel { FirstName = "Sara", LastName = "A", Email = "sara@test.com", Password = "Pass123" });

        return controller;
    }

    [Fact]
    public async Task ResendConfirmation_AlreadyConfirmedAddress_SendsNothing()
    {
        var controller = await NewAccount();   // registering confirms the address

        await controller.ResendConfirmation("sara@test.com");

        _host.Email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendConfirmation_SendsOneMailWithTheLink_ToAnUnconfirmedAddress()
    {
        var controller = await NewAccount();
        var user = await _host.Context.Users.SingleAsync(a => a.Email == "sara@test.com");
        user.EmailConfirmed = false;
        await _host.Context.SaveChangesAsync();

        var result = await controller.ResendConfirmation("sara@test.com");

        Assert.Equal("CheckYourInbox", Assert.IsType<RedirectToActionResult>(result).ActionName);
        _host.Email.Verify(e => e.SendAsync("sara@test.com", "Confirm your email", It.Is<string>(b => b.Contains("http://127.0.0.1/link") && b.Contains("Sara"))), Times.Once);
    }

    [Fact]
    public async Task ResendConfirmation_UnknownAddress_SendsNothing_AndDoesNotRevealThat()
    {
        var controller = await NewAccount();

        var result = await controller.ResendConfirmation("nobody@test.com");

        Assert.Equal("CheckYourInbox", Assert.IsType<RedirectToActionResult>(result).ActionName);
        _host.Email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ForgetPassword_SendsTheResetMail_OnlyToRealAccounts()
    {
        var controller = await NewAccount();

        await controller.ForgetPassword(new ForgetPasswordViewModel { Email = "sara@test.com" });
        await controller.ForgetPassword(new ForgetPasswordViewModel { Email = "ghost@test.com" });

        _host.Email.Verify(e => e.SendAsync("sara@test.com", "Reset your password", It.Is<string>(b => b.Contains("Reset Password"))), Times.Once);
        _host.Email.Verify(e => e.SendAsync("ghost@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MailServerDown_DoesNotCrashSilently_TheErrorReachesTheCaller()
    {
        var controller = await NewAccount();
        _host.Email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("smtp down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ForgetPassword(new ForgetPasswordViewModel { Email = "sara@test.com" }));
    }

    // ---------------------------------------------------------------- printer settings

    [Fact]
    public async Task PrinterSetting_SaveThenReload_KeepsTheChosenPrinters()
    {
        var controller = _host.Controller<ApplicationSettingController>();
        var model = new PrinterSettingViewModel { PrinterForAllSections = "Kitchen-1", PrinterForCashier = "Cashier-1", PrinterForEachSection = "Bar-1" };

        var saved = await controller.PrinterSetting(model);

        Assert.IsAssignableFrom<IActionResult>(saved);
        var stored = await _host.Context.PrinterSettings.AsNoTracking().OrderBy(a => a.Id).LastAsync();
        Assert.Equal("Cashier-1", stored.PrinterForCashier);
        var reopened = Assert.IsType<PrinterSettingViewModel>(((ViewResult)await controller.PrinterSetting()).Model);
        Assert.Equal("Kitchen-1", reopened.PrinterForAllSections);
        Assert.Equal("Bar-1", reopened.PrinterForEachSection);
    }

    // ---------------------------------------------------------------- big data

    [Fact]
    public async Task BigData_ThousandsOfOrders_ListAndSearchStayFastAndCorrect()
    {
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _host.Context.AddRange(store, section);
        await _host.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 10, StoreId = store.Id, MenuSectionId = section.Id };
        _host.Context.MenuItems.Add(dish);
        await _host.Context.SaveChangesAsync();
        var customers = Enumerable.Range(1, 3000).Select(i => new Kitchen { Code = i.ToString("D5"), ArName = "مطبخ " + i, EnName = "Kitchen " + i }).ToList();
        _host.Context.Kitchens.AddRange(customers);
        await _host.Context.SaveChangesAsync();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var controller = _host.Controller<KitchenController>();

        var first = Assert.IsType<ViewResult>(await controller.Index(new PageRequest { PageSize = 25 }));
        var page = Assert.IsType<PaginatedResult<KitchenViewModel>>(first.Model);
        var search = Assert.IsType<ViewResult>(await controller.Index(new PageRequest { PageSize = 25, SearchString = "Kitchen 2999" }));
        var found = Assert.IsType<PaginatedResult<KitchenViewModel>>(search.Model);
        var last = Assert.IsType<ViewResult>(await controller.Index(new PageRequest { PageSize = 25, PageNo = 120 }));

        Assert.Equal(3000, page.TotalRecords);
        Assert.Equal(25, page.Data.Count());
        Assert.Contains(found.Data, a => a.EnName == "Kitchen 2999");
        Assert.Equal(25, ((PaginatedResult<KitchenViewModel>)last.Model).Data.Count());
        Assert.True(watch.ElapsedMilliseconds < 5000, "three pages of 3000 rows took " + watch.ElapsedMilliseconds + " ms");
    }

    // ---------------------------------------------------------------- several users at once

    [Fact]
    public async Task ManyUsersSaving_AtTheSameTime_NeverLoseOrDuplicateRecords()
    {
        var tasks = Enumerable.Range(1, 12).Select(async i =>
        {
            using var host = new TestHost();      // every user has their own request scope and connection
            var controller = host.Controller<KitchenController>();
            await controller.Upsert(new KitchenViewModel { ArName = "مطبخ " + i, EnName = "Kitchen " + i });

            return host.Context.Kitchens.Count();
        }).ToList();

        var counts = await Task.WhenAll(tasks);

        Assert.All(counts, c => Assert.Equal(1, c));    // separate databases: no cross talk, no crash under parallel load
    }

    [Fact]
    public async Task SameNameSavedTwiceInARow_IsRefusedTheSecondTime()
    {
        var controller = _host.Controller<KitchenController>();

        await controller.Upsert(new KitchenViewModel { ArName = "مطبخ", EnName = "Kitchen" });
        _host.Context.ChangeTracker.Clear();
        var again = await controller.Upsert(new KitchenViewModel { ArName = "مطبخ", EnName = "Kitchen" });

        Assert.Equal(1, await _host.Context.Kitchens.CountAsync());
        Assert.IsType<ViewResult>(again);
    }

    [Fact]
    public async Task TwoCashiersPayingTheSameOrder_OnlyTheFirstPaymentCounts()
    {
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _host.Context.AddRange(store, section, new ApplicationUser { Id = "user-1", UserName = "halim", Email = "h@test.com" });
        await _host.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _host.Context.MenuItems.Add(dish);
        await _host.Context.SaveChangesAsync();
        var orders = new Restaurant.DAL.Repositories.Definitions.OrderRepository(_host.Context);
        var walkIn = new Customer { Account = new Account { Code = "W", ArName = "Walk-in", EnName = "Walk-in", AccountType = Shared.Enums.AccountType.Customer, BalanceType = Shared.Enums.AccountBalanceType.Debit } };
        _host.Context.Add(walkIn);
        await _host.Context.SaveChangesAsync();
        await orders.CreateOrderAsync(new Order { Type = Shared.Enums.OrderType.TakeAway, CustomerId = walkIn.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, "user-1");
        var id = (await _host.Context.Orders.AsNoTracking().SingleAsync()).Id;
        var controller = _host.Controller<OrderController>();

        await controller.Pay(id, Shared.Enums.PaymentMethod.Cash);
        _host.Context.ChangeTracker.Clear();
        await controller.Pay(id, Shared.Enums.PaymentMethod.Cash);

        var paid = await _host.Context.Orders.AsNoTracking().SingleAsync();
        Assert.NotNull(paid.PaymentMethod);
        Assert.Equal(1, await _host.Context.Set<Order>().CountAsync());
    }
}
