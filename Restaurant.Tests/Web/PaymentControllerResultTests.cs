using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using ResturantPL.Controllers.Definitions;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The supplier and the customer payment screens are the same screen with another partner: every answer of the service
// (save, refuse, delete, refund request/approval/rejection) is checked on both.
public class PaymentControllerResultTests
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    public static IEnumerable<object[]> Controllers() => new[]
    {
        new object[] { typeof(SupplierPaymentController) },
        new object[] { typeof(CustomerPaymentController) }
    };

    private class Screen
    {
        public Mock Service;
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public Controller Controller;

        public Screen(Type type, MockBehavior serviceBehavior = MockBehavior.Loose)
        {
            var serviceType = type.GetConstructors().Single().GetParameters()[0].ParameterType;
            Service = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType), serviceBehavior);
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
            Lookup.Setup(a => a.SupplierLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Lookup.Setup(a => a.CustomerLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });

            Controller = (Controller)Activator.CreateInstance(type, Service.Object, Lookup.Object, Toastr.Object, localizer.Object);
            Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }, "test"))
                }
            };
        }

        // Upsert has two overloads: a number (or nothing) opens the form, a posted model saves it
        public async Task<IActionResult> Call(string action, params object[] arguments)
        {
            var candidates = Controller.GetType().GetMethods().Where(m => m.Name == action && m.GetParameters().Length == arguments.Length).ToList();
            var method = candidates.Count == 1
                ? candidates[0]
                : candidates.Single(m => (arguments[0] is null || arguments[0] is int) == (m.GetParameters()[0].ParameterType == typeof(int?)));

            return (IActionResult)await (dynamic)method.Invoke(Controller, arguments);
        }

        public void Answer(string method, object value) => ServiceMocks.Answer(Service, method, value);

        public void Throw(string method) => ServiceMocks.Throw(Service, method);
    }

    private static object Prop(IActionResult result, string name)
    {
        var value = Assert.IsType<JsonResult>(result).Value;

        return value.GetType().GetProperty(name).GetValue(value);
    }

    private static T ModelOf<T>(IActionResult result) => (T)Assert.IsType<ViewResult>(result).Model;

    // ---------------------------------------------------------------- list and open

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Index_ListsPayments_AndAFailureEndsOnTheErrorPage(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetPagedPaymentsAsync", PaginatedResult<PaymentViewModel>.Create(new List<PaymentViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await screen.Call("Index", (object)null));

        screen.Throw("GetPagedPaymentsAsync");
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await screen.Call("Index", new PageRequest())).ViewName);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Open_NewPayment_GetsItsCodeADefaultDescriptionAndThePartners(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetNextCodeAsync", "P-7");

        var model = ModelOf<PaymentViewModel>(await screen.Call("Upsert", (object)null));

        Assert.Equal("P-7", model.ProcessCode);
        Assert.Equal("DefaultPaymentDescription", model.Description);
        Assert.Single(model.Partners);
        Assert.Empty(model.OpenInvoices);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Open_ExistingPayment_LoadsTheOpenInvoicesOfItsPartner_OrBacksToTheListWhenMissing(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetPaymentByIdAsync", new PaymentViewModel { Id = 4, PartnerId = 9 });
        screen.Answer("GetOpenInvoicesForPartnerAsync", new List<LookupItem> { new() { Id = 50 } });

        var model = ModelOf<PaymentViewModel>(await screen.Call("Upsert", 4));

        Assert.Single(model.OpenInvoices);

        var missing = new Screen(type);
        var result = await missing.Call("Upsert", 5);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(result).ActionName);
        missing.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Open_WhenAServiceFails_EndsOnTheErrorPage(Type type)
    {
        var screen = new Screen(type, MockBehavior.Strict);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await screen.Call("Upsert", (object)null)).ViewName);
    }

    // ---------------------------------------------------------------- save

    private static PaymentViewModel Posted(int id = 0, int? partner = 9) => new() { Id = id, PartnerId = partner };

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Save_InvalidForm_ShowsItAgainWithThePartnersAndTheirOpenInvoices(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetOpenInvoicesForPartnerAsync", new List<LookupItem> { new() { Id = 50 } });
        screen.Controller.ModelState.AddModelError("Paid", "required");

        var model = ModelOf<PaymentViewModel>(await screen.Call("Upsert", Posted()));

        Assert.Single(model.Partners);
        Assert.Single(model.OpenInvoices);
        screen.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);

        var noPartner = ModelOf<PaymentViewModel>(await screen.Call("Upsert", Posted(0, null)));
        Assert.Empty(noPartner.OpenInvoices);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Save_Succeeds_GoesToTheListFilteredByTheNewCode(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("UpsertPaymentAsync", new ResultMessage { Success = true, Data = "P-7" });

        var created = await screen.Call("Upsert", Posted());
        var edited = await screen.Call("Upsert", Posted(3));

        Assert.Equal("P-7", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.IsType<RedirectToActionResult>(edited);
        screen.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        screen.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Save_Refused_ShowsTheFormAgainWithTheMessage_OrTheGenericOne(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetOpenInvoicesForPartnerAsync", new List<LookupItem> { new() { Id = 50 } });
        screen.Answer("UpsertPaymentAsync", new ResultMessage { Success = false, Message = "PaidTooBig" });

        var refused = ModelOf<PaymentViewModel>(await screen.Call("Upsert", Posted()));
        Assert.Single(refused.OpenInvoices);
        screen.Toastr.Verify(a => a.Error("PaidTooBig"), Times.Once);

        screen.Answer("UpsertPaymentAsync", new ResultMessage { Success = false });
        await screen.Call("Upsert", Posted());
        await screen.Call("Upsert", Posted(3));
        screen.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
        screen.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Save_WhenAServiceFails_EndsOnTheErrorPage(Type type)
    {
        var screen = new Screen(type, MockBehavior.Strict);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await screen.Call("Upsert", Posted())).ViewName);
    }

    // ---------------------------------------------------------------- delete, pdf, partner lookups

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("DeletePaymentAsync", new ResultMessage { Success = true });
        Assert.Equal(true, Prop(await screen.Call("Delete", 1), "success"));

        screen.Answer("DeletePaymentAsync", new ResultMessage { Success = false, Message = "PaymentHasRefunds" });
        Assert.Equal("PaymentHasRefunds", Prop(await screen.Call("Delete", 2), "errorMessage"));

        screen.Throw("DeletePaymentAsync");
        Assert.Equal("SomethingWentError", Prop(await screen.Call("Delete", 3), "errorMessage"));
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Pdf_OfAnUnknownPayment_IsNotFound(Type type)
    {
        Assert.IsType<NotFoundResult>(await new Screen(type).Call("Pdf", 404));
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Balance_AndOpenInvoices_AnswerWithJson(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetBalanceAsync", 250m);
        screen.Answer("GetOpenInvoicesForPartnerAsync", new List<LookupItem> { new() { Id = 50 } });

        Assert.Equal(250m, Assert.IsType<JsonResult>(await screen.Call("Balance", 9)).Value);
        Assert.Single((IEnumerable<LookupItem>)Assert.IsType<JsonResult>(await screen.Call("OpenInvoices", 9)).Value);
    }

    // ---------------------------------------------------------------- refunds

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Refunds_ShowsTheRequestForm_BacksToTheListWhenMissing_AndAFailureEndsOnTheErrorPage(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("GetRefundRequestAsync", new PaymentRefundRequestViewModel { OriginalPaymentId = 4 });
        Assert.Equal(4, ModelOf<PaymentRefundRequestViewModel>(await screen.Call("Refunds", 4)).OriginalPaymentId);

        var missing = new Screen(type);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await missing.Call("Refunds", 5)).ActionName);
        missing.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Screen(type, MockBehavior.Strict).Call("Refunds", 6)).ViewName);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task RequestRefund_InvalidFormBacksToTheRefundsScreen_WithoutCallingTheService(Type type)
    {
        var screen = new Screen(type, MockBehavior.Strict);
        screen.Controller.ModelState.AddModelError("Amount", "required");

        var result = await screen.Call("RequestRefund", new PaymentRefundRequestViewModel { OriginalPaymentId = 4 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Refunds", redirect.ActionName);
        Assert.Equal(4, redirect.RouteValues["paymentId"]);
        screen.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task RequestRefund_SavesForTheSignedInUser_OrShowsWhyNot(Type type)
    {
        var screen = new Screen(type);
        screen.Answer("RequestRefundAsync", new ResultMessage { Success = true });
        Assert.Equal("Refunds", Assert.IsType<RedirectToActionResult>(await screen.Call("RequestRefund", new PaymentRefundRequestViewModel { OriginalPaymentId = 4 })).ActionName);
        screen.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);

        screen.Answer("RequestRefundAsync", new ResultMessage { Success = false, Message = "RefundTooBig" });
        await screen.Call("RequestRefund", new PaymentRefundRequestViewModel { OriginalPaymentId = 4 });
        screen.Answer("RequestRefundAsync", new ResultMessage { Success = false });
        await screen.Call("RequestRefund", new PaymentRefundRequestViewModel { OriginalPaymentId = 4 });
        screen.Toastr.Verify(a => a.Error("RefundTooBig"), Times.Once);
        screen.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Screen(type, MockBehavior.Strict).Call("RequestRefund", new PaymentRefundRequestViewModel())).ViewName);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task ApproveAndRejectRefund_ReportTheResult_AndGoBackToTheRefundsOfThePayment(Type type)
    {
        foreach (var action in new[] { "ApproveRefund", "RejectRefund" })
        {
            var method = action + "Async";
            var screen = new Screen(type);

            screen.Answer(method, new ResultMessage { Success = true });
            var done = await screen.Call(action, 8, 4);
            Assert.Equal(4, ((RedirectToActionResult)done).RouteValues["paymentId"]);
            screen.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);

            screen.Answer(method, new ResultMessage { Success = false, Message = "RefundNotPending" });
            await screen.Call(action, 8, 4);
            screen.Answer(method, new ResultMessage { Success = false });
            await screen.Call(action, 8, 4);
            screen.Toastr.Verify(a => a.Error("RefundNotPending"), Times.Once);
            screen.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);

            Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Screen(type, MockBehavior.Strict).Call(action, 8, 4)).ViewName);
        }
    }
}
