using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using ResturantPL.Controllers;
using ResturantPL.Controllers.Definitions;
using Shared;
using Shared.Enums;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The two screens that do not follow the 3-line definition template: the chart of accounts (kind rules, owned accounts)
// and the settings (application and printer). Every answer of the service ends on a message, a form or the error page.
public class ChartAccountAndSettingControllerTests
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    private static Mock<IStringLocalizer<SharedResource>> Localizer()
    {
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

        return localizer;
    }

    private static T WithContext<T>(T controller) where T : Controller
    {
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        return controller;
    }

    // ---------------------------------------------------------------- chart of accounts

    private class Chart
    {
        public Mock<IAccountDefinitionService> Service = new(MockBehavior.Loose);
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public ChartAccountController Controller;

        public Chart()
        {
            Service.Setup(a => a.GetParentOptionsAsync(It.IsAny<int?>())).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Lookup.Setup(a => a.NamedLookupAsync<Currency>()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Controller = WithContext(new ChartAccountController(Service.Object, Lookup.Object, Toastr.Object, Localizer().Object));
        }
    }

    [Fact]
    public async Task Chart_Index_ListsAccounts_AndAFailureEndsOnTheErrorPage()
    {
        var chart = new Chart();
        chart.Service.Setup(a => a.GetPagedAccountsAsync(It.IsAny<PageRequest>(), false)).ReturnsAsync(PaginatedResult<AccountDefinitionViewModel>.Create(new List<AccountDefinitionViewModel>(), 0, new PageRequest()));

        var ok = await chart.Controller.Index(null);

        Assert.Null(Assert.IsType<ViewResult>(ok).ViewName);

        chart.Service.Setup(a => a.GetPagedAccountsAsync(It.IsAny<PageRequest>(), false)).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await chart.Controller.Index(new PageRequest())).ViewName);
    }

    [Theory]
    [InlineData(null, AccountKind.GeneralLedger)]
    [InlineData(AccountKind.Asset, AccountKind.GeneralLedger)]
    [InlineData(AccountKind.Owned, AccountKind.GeneralLedger)]
    [InlineData(AccountKind.Bank, AccountKind.Bank)]
    [InlineData(AccountKind.Expense, AccountKind.Expense)]
    public async Task Chart_NewAccount_AssetsAndOwnedAreNotCreatedHere_TheOtherKindsAre(AccountKind? asked, AccountKind opened)
    {
        var chart = new Chart();
        chart.Service.Setup(a => a.NewAccountCodeAsync(null, opened)).ReturnsAsync("1.7");

        var result = await chart.Controller.Upsert(null, asked);

        var model = Assert.IsType<AccountDefinitionViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(opened, model.Kind);
        Assert.Equal("1.7", model.Code);
        Assert.Single(model.Parents);
        Assert.Single(model.Currencies);
    }

    [Fact]
    public async Task Chart_OpenAccount_UnknownOrOwnedByAnotherScreen_GoesBackToTheListWithAMessage()
    {
        var chart = new Chart();

        // a loose service answers null: no such account
        var missing = await chart.Controller.Upsert(5, null);
        chart.Service.Setup(a => a.GetAccountByIdAsync(6)).ReturnsAsync(new AccountDefinitionViewModel { Id = 6, IsOwned = true });
        var owned = await chart.Controller.Upsert(6, null);
        chart.Service.Setup(a => a.GetAccountByIdAsync(7)).ReturnsAsync(new AccountDefinitionViewModel { Id = 7, Kind = AccountKind.Asset });
        var asset = await chart.Controller.Upsert(7, null);

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(owned).ActionName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(asset).ActionName);
        chart.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
        chart.Toastr.Verify(a => a.Error("AccountOwnedByAnotherScreen"), Times.Exactly(2));
    }

    [Fact]
    public async Task Chart_OpenAccount_ShowsTheFormWithItsLists_AndAFailureEndsOnTheErrorPage()
    {
        var chart = new Chart();
        chart.Service.Setup(a => a.GetAccountByIdAsync(8)).ReturnsAsync(new AccountDefinitionViewModel { Id = 8, Kind = AccountKind.Bank, ArName = "Bank" });

        var result = await chart.Controller.Upsert(8, null);

        var model = Assert.IsType<AccountDefinitionViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Bank", model.ArName);
        Assert.Single(model.Parents);
        chart.Service.Verify(a => a.GetParentOptionsAsync(8), Times.Once);

        chart.Service.Setup(a => a.GetAccountByIdAsync(9)).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await chart.Controller.Upsert(9, null)).ViewName);
    }

    [Theory]
    [InlineData(AccountKind.Asset)]
    [InlineData(AccountKind.Owned)]
    public async Task Chart_Save_AnAssetOrOwnedAccount_IsRefusedBeforeTheServiceIsCalled(AccountKind kind)
    {
        var chart = new Chart();

        var result = await chart.Controller.Upsert(new AccountDefinitionViewModel { Kind = kind, ArName = "x" });

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        chart.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        chart.Service.Verify(a => a.UpsertAccountAsync(It.IsAny<AccountDefinitionViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Chart_Save_InvalidForm_ShowsItAgainWithItsLists()
    {
        var chart = new Chart();
        chart.Controller.ModelState.AddModelError("ArName", "required");

        var result = await chart.Controller.Upsert(new AccountDefinitionViewModel { Id = 4, Kind = AccountKind.Bank });

        var model = Assert.IsType<AccountDefinitionViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model.Currencies);
        chart.Service.Verify(a => a.GetParentOptionsAsync(4), Times.Once);
    }

    [Fact]
    public async Task Chart_Save_Succeeds_RefusesWithAMessage_RefusesWithoutOne_OrFails()
    {
        var chart = new Chart();
        chart.Service.Setup(a => a.UpsertAccountAsync(It.Is<AccountDefinitionViewModel>(m => m.ArName == "ok"))).ReturnsAsync(new ResultMessage { Success = true });
        chart.Service.Setup(a => a.UpsertAccountAsync(It.Is<AccountDefinitionViewModel>(m => m.ArName == "dup"))).ReturnsAsync(new ResultMessage { Success = false, Message = "NameAlreadyExist" });
        chart.Service.Setup(a => a.UpsertAccountAsync(It.Is<AccountDefinitionViewModel>(m => m.ArName == "bare"))).ReturnsAsync(new ResultMessage { Success = false });
        chart.Service.Setup(a => a.UpsertAccountAsync(It.Is<AccountDefinitionViewModel>(m => m.ArName == "boom"))).ThrowsAsync(new InvalidOperationException());

        var created = await chart.Controller.Upsert(new AccountDefinitionViewModel { ArName = "ok" });
        var edited = await chart.Controller.Upsert(new AccountDefinitionViewModel { Id = 3, ArName = "ok" });
        var refused = await chart.Controller.Upsert(new AccountDefinitionViewModel { ArName = "dup" });
        var bareNew = await chart.Controller.Upsert(new AccountDefinitionViewModel { ArName = "bare" });
        var bareEdit = await chart.Controller.Upsert(new AccountDefinitionViewModel { Id = 3, ArName = "bare" });
        var failed = await chart.Controller.Upsert(new AccountDefinitionViewModel { ArName = "boom" });

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(created).ActionName);
        Assert.Equal("ok", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareNew).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareEdit).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        chart.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        chart.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        chart.Toastr.Verify(a => a.Error("NameAlreadyExist"), Times.Once);
        chart.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
        chart.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    [Fact]
    public async Task Chart_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var chart = new Chart();
        chart.Service.Setup(a => a.DeleteAccountAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        chart.Service.Setup(a => a.DeleteAccountAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" });
        chart.Service.Setup(a => a.DeleteAccountAsync(3)).ThrowsAsync(new InvalidOperationException());

        var ok = (JsonResult)await chart.Controller.Delete(1);
        var refused = (JsonResult)await chart.Controller.Delete(2);
        var failed = (JsonResult)await chart.Controller.Delete(3);

        Assert.Equal(true, ok.Value.GetType().GetProperty("success").GetValue(ok.Value));
        Assert.Equal("CannotDeleteLinkedRecord", refused.Value.GetType().GetProperty("errorMessage").GetValue(refused.Value));
        Assert.Equal("SomethingWentError", failed.Value.GetType().GetProperty("errorMessage").GetValue(failed.Value));
    }

    // ---------------------------------------------------------------- application settings

    private class Settings
    {
        public Mock<ISettingService> Service = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public ApplicationSettingController Controller;

        public Settings() => Controller = WithContext(new ApplicationSettingController(Service.Object, Toastr.Object, Localizer().Object));
    }

    [Fact]
    public void Settings_Index_ShowsThePage()
    {
        Assert.IsType<ViewResult>(new Settings().Controller.Index());
    }

    [Fact]
    public async Task ApplicationSetting_Open_ShowsTheCurrentValues_AndAFailureEndsOnTheErrorPage()
    {
        var settings = new Settings();
        settings.Service.Setup(a => a.GetApplicationSettingAsync()).ReturnsAsync(new ApplicationSettingViewModel { OrderTax = 14 });

        var ok = await settings.Controller.ApplicationSetting();

        Assert.Equal(14, ((ApplicationSettingViewModel)((ViewResult)ok).Model).OrderTax);

        settings.Service.Setup(a => a.GetApplicationSettingAsync()).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await settings.Controller.ApplicationSetting()).ViewName);
    }

    [Fact]
    public async Task ApplicationSetting_Save_InvalidForm_ShowsItAgainWithoutCallingTheService()
    {
        var settings = new Settings();
        settings.Controller.ModelState.AddModelError("OrderTax", "range");

        var result = await settings.Controller.ApplicationSetting(new ApplicationSettingViewModel());

        Assert.IsType<ViewResult>(result);
        settings.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        settings.Service.Verify(a => a.UpdateApplicationSettingAsync(It.IsAny<ApplicationSettingViewModel>()), Times.Never);
    }

    [Fact]
    public async Task ApplicationSetting_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var settings = new Settings();
        settings.Service.Setup(a => a.UpdateApplicationSettingAsync(It.Is<ApplicationSettingViewModel>(m => m.OrderTax == 1))).ReturnsAsync(new ResultMessage { Success = true });
        settings.Service.Setup(a => a.UpdateApplicationSettingAsync(It.Is<ApplicationSettingViewModel>(m => m.OrderTax == 2))).ReturnsAsync(new ResultMessage { Success = false, Message = "BadPassword" });
        settings.Service.Setup(a => a.UpdateApplicationSettingAsync(It.Is<ApplicationSettingViewModel>(m => m.OrderTax == 3))).ReturnsAsync(new ResultMessage { Success = false });
        settings.Service.Setup(a => a.UpdateApplicationSettingAsync(It.Is<ApplicationSettingViewModel>(m => m.OrderTax == 4))).ThrowsAsync(new InvalidOperationException());

        var saved = await settings.Controller.ApplicationSetting(new ApplicationSettingViewModel { OrderTax = 1 });
        var refused = await settings.Controller.ApplicationSetting(new ApplicationSettingViewModel { OrderTax = 2 });
        var bare = await settings.Controller.ApplicationSetting(new ApplicationSettingViewModel { OrderTax = 3 });
        var failed = await settings.Controller.ApplicationSetting(new ApplicationSettingViewModel { OrderTax = 4 });

        Assert.Equal("ApplicationSetting", Assert.IsType<RedirectToActionResult>(saved).ActionName);
        Assert.Equal("ApplicationSetting", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("ApplicationSetting", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        settings.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        settings.Toastr.Verify(a => a.Error("BadPassword"), Times.Once);
        settings.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    // ---------------------------------------------------------------- printer settings

    [Fact]
    public async Task PrinterSetting_Open_ShowsTheSettingWithThePrintersOfTheMachine_AndAFailureEndsOnTheErrorPage()
    {
        var settings = new Settings();
        settings.Service.Setup(a => a.GetPrinterSettingAsync()).ReturnsAsync(new PrinterSettingViewModel());

        var ok = await settings.Controller.PrinterSetting();

        var model = Assert.IsType<PrinterSettingViewModel>(Assert.IsType<ViewResult>(ok).Model);
        Assert.NotNull(model.Printers);

        settings.Service.Setup(a => a.GetPrinterSettingAsync()).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await settings.Controller.PrinterSetting()).ViewName);
    }

    [Fact]
    public async Task PrinterSetting_Save_InvalidForm_ShowsItAgainWithThePrinters()
    {
        var settings = new Settings();
        settings.Controller.ModelState.AddModelError("Printer", "required");

        var result = await settings.Controller.PrinterSetting(new PrinterSettingViewModel());

        var model = Assert.IsType<PrinterSettingViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.NotNull(model.Printers);
        settings.Service.Verify(a => a.UpdatePrinterSettingAsync(It.IsAny<PrinterSettingViewModel>()), Times.Never);
    }

    [Fact]
    public async Task PrinterSetting_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var settings = new Settings();
        settings.Service.Setup(a => a.UpdatePrinterSettingAsync(It.Is<PrinterSettingViewModel>(m => m.PrinterForCashier == "ok"))).ReturnsAsync(new ResultMessage { Success = true });
        settings.Service.Setup(a => a.UpdatePrinterSettingAsync(It.Is<PrinterSettingViewModel>(m => m.PrinterForCashier == "no"))).ReturnsAsync(new ResultMessage { Success = false, Message = "PrinterMissing" });
        settings.Service.Setup(a => a.UpdatePrinterSettingAsync(It.Is<PrinterSettingViewModel>(m => m.PrinterForCashier == "bare"))).ReturnsAsync(new ResultMessage { Success = false });
        settings.Service.Setup(a => a.UpdatePrinterSettingAsync(It.Is<PrinterSettingViewModel>(m => m.PrinterForCashier == "boom"))).ThrowsAsync(new InvalidOperationException());

        var saved = await settings.Controller.PrinterSetting(new PrinterSettingViewModel { PrinterForCashier = "ok" });
        var refused = await settings.Controller.PrinterSetting(new PrinterSettingViewModel { PrinterForCashier = "no" });
        var bare = await settings.Controller.PrinterSetting(new PrinterSettingViewModel { PrinterForCashier = "bare" });
        var failed = await settings.Controller.PrinterSetting(new PrinterSettingViewModel { PrinterForCashier = "boom" });

        Assert.Equal("PrinterSetting", Assert.IsType<RedirectToActionResult>(saved).ActionName);
        Assert.Equal("PrinterSetting", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("PrinterSetting", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        Assert.NotNull(((PrinterSettingViewModel)((ViewResult)refused).Model).Printers);
        settings.Toastr.Verify(a => a.Error("PrinterMissing"), Times.Once);
        settings.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }
}
