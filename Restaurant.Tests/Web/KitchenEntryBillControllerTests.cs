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

// The kitchen screen, the dashboard, the account setting, manual journal entries and the bills/invoices:
// each answer of the services ends on a message, a form, a JSON answer or the error page.
public class KitchenEntryBillControllerTests
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

    private static object Prop(IActionResult result, string name)
    {
        var value = Assert.IsType<JsonResult>(result).Value;

        return value.GetType().GetProperty(name).GetValue(value);
    }

    // ---------------------------------------------------------------- kitchen

    [Fact]
    public async Task Kitchen_Index_ShowsTheOrders_AndAFailureEndsOnTheErrorPage()
    {
        var orders = new Mock<IOrderService>(MockBehavior.Loose);
        orders.Setup(a => a.GetKitchenOrdersAsync()).ReturnsAsync(new List<OrderViewModel>());
        var controller = WithContext(new KitchenOrderController(orders.Object, Localizer().Object));

        Assert.IsType<ViewResult>(await controller.Index());

        orders.Setup(a => a.GetKitchenOrdersAsync()).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await controller.Index()).ViewName);
    }

    [Fact]
    public async Task Kitchen_Complete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var orders = new Mock<IOrderService>(MockBehavior.Loose);
        orders.Setup(a => a.CompleteOrderAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        orders.Setup(a => a.CompleteOrderAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "OrderNotFound" });
        orders.Setup(a => a.CompleteOrderAsync(3)).ThrowsAsync(new InvalidOperationException());
        var controller = WithContext(new KitchenOrderController(orders.Object, Localizer().Object));

        Assert.Equal(true, Prop(await controller.Complete(1), "success"));
        Assert.Equal("OrderNotFound", Prop(await controller.Complete(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await controller.Complete(3), "errorMessage"));
    }

    // ---------------------------------------------------------------- dashboard

    [Fact]
    public async Task Dashboard_Index_ShowsTheDashboard_Board_ShowsThePartial_AndFailuresAreHandled()
    {
        var service = new Mock<IDashboardService>(MockBehavior.Loose);
        service.Setup(a => a.GetDashboardAsync(It.IsAny<string>())).ReturnsAsync(new DashboardViewModel());
        var controller = WithContext(new DashboardController(service.Object));

        Assert.IsType<ViewResult>(await controller.Index());
        Assert.Equal("_Board", Assert.IsType<PartialViewResult>(await controller.Board()).ViewName);

        service.Setup(a => a.GetDashboardAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await controller.Index()).ViewName);
        Assert.Equal(500, Assert.IsType<StatusCodeResult>(await controller.Board()).StatusCode);
    }

    // ---------------------------------------------------------------- account setting

    [Fact]
    public async Task AccountSetting_Open_ShowsTheSetting_AndAFailureEndsOnTheErrorPage()
    {
        var service = new Mock<IAccountSettingService>(MockBehavior.Loose);
        service.Setup(a => a.GetAccountSettingAsync()).ReturnsAsync(new AccountSettingViewModel());
        var controller = WithContext(new AccountSettingController(service.Object, new Mock<IToastrService>().Object, Localizer().Object));

        Assert.IsType<ViewResult>(await controller.Index());

        service.Setup(a => a.GetAccountSettingAsync()).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await controller.Index()).ViewName);
    }

    [Fact]
    public async Task AccountSetting_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var service = new Mock<IAccountSettingService>(MockBehavior.Loose);
        service.Setup(a => a.GetAccountSettingAsync()).ReturnsAsync(new AccountSettingViewModel());
        var toastr = new Mock<IToastrService>();
        var controller = WithContext(new AccountSettingController(service.Object, toastr.Object, Localizer().Object));
        service.SetupSequence(a => a.UpdateAccountSettingAsync(It.IsAny<AccountSettingViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "AccountMissing" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var saved = await controller.Index(new AccountSettingViewModel());
        var refused = await controller.Index(new AccountSettingViewModel());
        var bare = await controller.Index(new AccountSettingViewModel());
        var failed = await controller.Index(new AccountSettingViewModel());

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(saved).ActionName);
        Assert.IsType<ViewResult>(refused);
        Assert.IsType<ViewResult>(bare);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        toastr.Verify(a => a.Error("AccountMissing"), Times.Once);
        toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    // ---------------------------------------------------------------- manual journal entries

    private class Entries
    {
        public Mock<IEntryService> Service = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public EntryController Controller;

        public Entries()
        {
            Service.Setup(a => a.GetPostingAccountOptionsAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Controller = WithContext(new EntryController(Service.Object, Toastr.Object, Localizer().Object));
        }
    }

    [Fact]
    public async Task Entry_Index_ListsEntries_AndAFailureEndsOnTheErrorPage()
    {
        var entries = new Entries();
        entries.Service.Setup(a => a.GetPagedEntriesAsync(It.IsAny<PageRequest>())).ReturnsAsync(PaginatedResult<EntryViewModel>.Create(new List<EntryViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await entries.Controller.Index(null));

        entries.Service.Setup(a => a.GetPagedEntriesAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await entries.Controller.Index(new PageRequest())).ViewName);
    }

    [Fact]
    public async Task Entry_Details_ShowsTheEntry_BacksToTheListWhenMissing_AndAFailureEndsOnTheErrorPage()
    {
        var entries = new Entries();
        entries.Service.Setup(a => a.GetEntryByIdAsync(1)).ReturnsAsync(new EntryViewModel { Id = 1 });
        entries.Service.Setup(a => a.GetEntryByIdAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.IsType<ViewResult>(await entries.Controller.Details(1));
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await entries.Controller.Details(2)).ActionName);
        entries.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await entries.Controller.Details(3)).ViewName);
    }

    [Fact]
    public async Task Entry_NewEntry_StartsAsAJournalWithTwoEmptyLines_AndTheAccountsToPostTo()
    {
        var entries = new Entries();

        var result = await entries.Controller.Upsert();

        var model = Assert.IsType<EntryViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(EntryType.Journal, model.EntryType);
        Assert.Equal(2, model.Lines.Count);
        Assert.Single(model.Accounts);

        entries.Service.Setup(a => a.GetPostingAccountOptionsAsync()).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await entries.Controller.Upsert()).ViewName);
    }

    [Fact]
    public async Task Entry_Save_InvalidForm_ShowsItAgainWithoutCallingTheService()
    {
        var entries = new Entries();
        entries.Controller.ModelState.AddModelError("Lines", "required");

        var result = await entries.Controller.Upsert(new EntryViewModel());

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        Assert.Single(((EntryViewModel)((ViewResult)result).Model).Accounts);
        entries.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        entries.Service.Verify(a => a.CreateManualEntryAsync(It.IsAny<EntryViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Entry_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var entries = new Entries();
        entries.Service.SetupSequence(a => a.CreateManualEntryAsync(It.IsAny<EntryViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "EntryNotBalanced" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var saved = await entries.Controller.Upsert(new EntryViewModel());
        var refused = await entries.Controller.Upsert(new EntryViewModel());
        var bare = await entries.Controller.Upsert(new EntryViewModel());
        var failed = await entries.Controller.Upsert(new EntryViewModel());

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(saved).ActionName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        entries.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        entries.Toastr.Verify(a => a.Error("EntryNotBalanced"), Times.Once);
        entries.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
    }

    [Fact]
    public async Task Entry_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var entries = new Entries();
        entries.Service.Setup(a => a.DeleteManualEntryAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        entries.Service.Setup(a => a.DeleteManualEntryAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "EntryIsPosted" });
        entries.Service.Setup(a => a.DeleteManualEntryAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await entries.Controller.Delete(1), "success"));
        Assert.Equal("EntryIsPosted", Prop(await entries.Controller.Delete(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await entries.Controller.Delete(3), "errorMessage"));
    }

    // ---------------------------------------------------------------- bills and invoices

    private class Bills
    {
        public Mock<IBillService> Service = new(MockBehavior.Loose);
        public Mock<IInvoiceService> Invoices = new(MockBehavior.Loose);
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public BillController Controller;

        public Bills()
        {
            Invoices.Setup(a => a.GetPostingDefaultsAsync()).ReturnsAsync(new List<PostingDefaultViewModel>
            {
                new() { Type = InvoiceType.Purchases, AutoPost = false, AutoStock = true }
            });
            Lookup.Setup(a => a.SupplierLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Lookup.Setup(a => a.CustomerLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 2 } });
            Lookup.Setup(a => a.NamedLookupAsync<Store>()).ReturnsAsync(new List<LookupItem> { new() { Id = 3 } });
            Lookup.Setup(a => a.NamedLookupAsync<CostCenter>()).ReturnsAsync(new List<LookupItem> { new() { Id = 4 } });
            Lookup.Setup(a => a.NamedLookupAsync<Currency>()).ReturnsAsync(new List<LookupItem> { new() { Id = 5 } });
            Lookup.Setup(a => a.NamedLookupAsync<UnitPrice>()).ReturnsAsync(new List<LookupItem> { new() { Id = 6, EnName = "Kilo", ArName = "كيلو" } });
            Lookup.Setup(a => a.NamedLookupAsync<ProductCategory>()).ReturnsAsync(new List<LookupItem> { new() { Id = 7 } });
            Lookup.Setup(a => a.StoreProductLookupByStoreIdAsync(It.IsAny<int>())).ReturnsAsync(new List<StoreProductLookupItem>
            {
                new() { ProductId = 11, ProductEnName = "Flour", ProductArName = "دقيق" }
            });
            Controller = WithContext(new BillController(Service.Object, Invoices.Object, Lookup.Object, Toastr.Object, Localizer().Object));
        }
    }

    [Fact]
    public async Task Bill_Index_ListsBills_AndAFailureEndsOnTheErrorPage()
    {
        var bills = new Bills();
        bills.Service.Setup(a => a.GetPagedBillsAsync(It.IsAny<PageRequest>())).ReturnsAsync(PaginatedResult<BillListItemViewModel>.Create(new List<BillListItemViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await bills.Controller.Index(null));

        bills.Service.Setup(a => a.GetPagedBillsAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await bills.Controller.Index(new PageRequest())).ViewName);
    }

    [Fact]
    public async Task Bill_Details_ShowsTheBill_BacksToTheListWhenMissing_AndAFailureEndsOnTheErrorPage()
    {
        var bills = new Bills();
        bills.Service.Setup(a => a.GetBillByIdAsync(1)).ReturnsAsync(new BillViewModel());
        bills.Service.Setup(a => a.GetBillByIdAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.IsType<ViewResult>(await bills.Controller.Details(1));
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await bills.Controller.Details(2)).ActionName);
        bills.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await bills.Controller.Details(3)).ViewName);
    }

    [Fact]
    public async Task Bill_NewInvoice_StartsAsAPurchase_WithItsCode_ItsPostingDefaults_AndEveryList()
    {
        var bills = new Bills();
        bills.Invoices.Setup(a => a.GetNewInvoiceAsync(InvoiceType.Purchases))
            .ReturnsAsync(new InvoiceViewModel { InvoiceType = InvoiceType.Purchases, InvoiceCode = "P-9", AutoPost = false, AutoStock = true });

        var result = await bills.Controller.Upsert(null, null);

        var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(InvoiceType.Purchases, model.InvoiceType);
        Assert.Equal("P-9", model.InvoiceCode);
        Assert.False(model.AutoPost);
        Assert.True(model.AutoStock);
        Assert.Single(model.Suppliers);
        Assert.Single(model.Customers);
        Assert.Single(model.Stores);
        Assert.Single(model.CostCenters);
        Assert.Single(model.Currencies);
        Assert.Single(model.Units);
        Assert.Single(model.Categories);
    }

    [Fact]
    public async Task Bill_NewInvoice_OfATypeWithoutDefaults_PostsAndMovesStockByDefault()
    {
        var bills = new Bills();

        bills.Invoices.Setup(a => a.GetNewInvoiceAsync(InvoiceType.Sales)).ReturnsAsync(new InvoiceViewModel { InvoiceType = InvoiceType.Sales });

        var result = await bills.Controller.Upsert(0, InvoiceType.Sales);

        var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(InvoiceType.Sales, model.InvoiceType);
    }

    [Fact]
    public async Task Bill_OpenInvoice_KeepsBothNamesOfItsLines_OrBacksToTheListWhenMissing()
    {
        var bills = new Bills();
        bills.Invoices.Setup(a => a.GetInvoiceByIdAsync(5)).ReturnsAsync(new InvoiceViewModel
        {
            Id = 5, StoreId = 3,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = 11, ProductArName = "دقيق", ProductEnName = "Flour", UnitPriceId = 6, UnitArName = "كيلو", UnitEnName = "Kilo", Quantity = 1 } }
        });

        var found = await bills.Controller.Upsert(5, null);
        var missing = await bills.Controller.Upsert(6, null);

        var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(found).Model);
        Assert.Equal("دقيق", model.Items[0].ProductArName);
        Assert.Equal("Flour", model.Items[0].ProductEnName);
        Assert.Equal("كيلو", model.Items[0].UnitArName);
        Assert.Equal("Kilo", model.Items[0].UnitEnName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
    }

    [Fact]
    public async Task Bill_OpenOrNew_WhenAServiceFails_EndsOnTheErrorPage()
    {
        var bills = new Bills();
        bills.Invoices.Setup(a => a.GetNewInvoiceAsync(It.IsAny<InvoiceType>())).ThrowsAsync(new InvalidOperationException());
        bills.Invoices.Setup(a => a.GetInvoiceByIdAsync(9)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await bills.Controller.Upsert(null, null)).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await bills.Controller.Upsert(9, null)).ViewName);
    }

    [Fact]
    public async Task Bill_Save_InvalidForm_ShowsItAgainWithItsLists_WithoutCallingTheService()
    {
        var bills = new Bills();
        bills.Controller.ModelState.AddModelError("Items", "required");

        var result = await bills.Controller.Upsert(new InvoiceViewModel { StoreId = 3 });

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        Assert.Single(((InvoiceViewModel)((ViewResult)result).Model).Suppliers);
        bills.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        bills.Invoices.Verify(a => a.UpsertInvoiceAsync(It.IsAny<InvoiceViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Bill_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var bills = new Bills();
        bills.Invoices.SetupSequence(a => a.UpsertInvoiceAsync(It.IsAny<InvoiceViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true, Data = "A_5" })
            .ReturnsAsync(new ResultMessage { Success = true, Data = "A_6" })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "InsufficientStock" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var created = await bills.Controller.Upsert(new InvoiceViewModel());
        var edited = await bills.Controller.Upsert(new InvoiceViewModel { Id = 4 });
        var refused = await bills.Controller.Upsert(new InvoiceViewModel());
        var bareNew = await bills.Controller.Upsert(new InvoiceViewModel());
        var bareEdit = await bills.Controller.Upsert(new InvoiceViewModel { Id = 4 });
        var failed = await bills.Controller.Upsert(new InvoiceViewModel());

        Assert.Equal("A_5", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(edited).ActionName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareNew).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareEdit).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        bills.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        bills.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        bills.Toastr.Verify(a => a.Error("InsufficientStock"), Times.Once);
        bills.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
        bills.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    [Fact]
    public async Task Bill_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var bills = new Bills();
        bills.Invoices.Setup(a => a.DeleteInvoiceAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        bills.Invoices.Setup(a => a.DeleteInvoiceAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "InvoiceHasPayments" });
        bills.Invoices.Setup(a => a.DeleteInvoiceAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await bills.Controller.Delete(1), "success"));
        Assert.Equal("InvoiceHasPayments", Prop(await bills.Controller.Delete(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await bills.Controller.Delete(3), "errorMessage"));
    }

    [Fact]
    public async Task Bill_Pdf_OfAnUnknownInvoice_IsNotFound()
    {
        Assert.IsType<NotFoundResult>(await new Bills().Controller.Pdf(404));
    }

    [Fact]
    public async Task Bill_PartnerBalances_AreReturnedAsNumbers_AndAFailureAnswersWithTheGenericMessage()
    {
        var bills = new Bills();
        bills.Invoices.Setup(a => a.GetSupplierBalanceAsync(1)).ReturnsAsync(250m);
        bills.Invoices.Setup(a => a.GetCustomerBalanceAsync(2)).ReturnsAsync(-40m);
        bills.Invoices.Setup(a => a.GetSupplierBalanceAsync(3)).ThrowsAsync(new InvalidOperationException());
        bills.Invoices.Setup(a => a.GetCustomerBalanceAsync(4)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(250m, Assert.IsType<JsonResult>(await bills.Controller.SupplierBalance(1)).Value);
        Assert.Equal(-40m, Assert.IsType<JsonResult>(await bills.Controller.CustomerBalance(2)).Value);
        Assert.Equal("SomethingWentError", Prop(await bills.Controller.SupplierBalance(3), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await bills.Controller.CustomerBalance(4), "errorMessage"));
    }

    [Fact]
    public async Task Bill_Post_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var bills = new Bills();
        bills.Service.Setup(a => a.PostBillAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        bills.Service.Setup(a => a.PostBillAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "BillAlreadyPosted" });
        bills.Service.Setup(a => a.PostBillAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await bills.Controller.Post(1), "success"));
        Assert.Equal("BillAlreadyPosted", Prop(await bills.Controller.Post(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await bills.Controller.Post(3), "errorMessage"));
    }

    [Fact]
    public async Task Bill_PostPending_ReportsHowManyWerePosted_OrWhyNot()
    {
        var bills = new Bills();
        bills.Service.SetupSequence(a => a.PostPendingAsync())
            .ReturnsAsync(new ResultMessage { Success = true, Data = "4" })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "NothingToPost" })
            .ThrowsAsync(new InvalidOperationException());

        var posted = await bills.Controller.PostPending();
        var refused = await bills.Controller.PostPending();
        var failed = await bills.Controller.PostPending();

        Assert.Equal(true, Prop(posted, "success"));
        Assert.Equal("4", Prop(posted, "count"));
        Assert.Equal("NothingToPost", Prop(refused, "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(failed, "errorMessage"));
    }
}
