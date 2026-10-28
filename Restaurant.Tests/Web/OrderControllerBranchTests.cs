using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Controllers.Definitions;
using Shared;
using Shared.Enums;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// The cashier controller, one answer of its services at a time: what the cashier sees when a save is refused,
// when an order cannot be moved/merged, when a payment fails, and that a failing service always ends on the error page.
public class OrderControllerBranchTests
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    private class Cashier
    {
        public Mock<IOrderService> Orders = new(MockBehavior.Loose);
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<ICustomerService> Customers = new(MockBehavior.Loose);
        public Mock<ISettingService> Settings = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public Mock<IStringLocalizer<SharedResource>> Localizer = new();
        public OrderController Controller;

        public Cashier()
        {
            Localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
            Lookup.Setup(a => a.CustomerLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Lookup.Setup(a => a.FreeTableLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 2 } });
            Lookup.Setup(a => a.DeliveryDriverLookupAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 3 } });
            Lookup.Setup(a => a.DeliveryZoneLookupAsync()).ReturnsAsync(new List<DeliveryZoneViewModel> { new() { Id = 4 } });
            Controller = new OrderController(Orders.Object, Lookup.Object, Customers.Object, Settings.Object, Toastr.Object, Localizer.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        // the order service fails every call it was not set up for
        public Cashier OrdersFail()
        {
            Orders = new Mock<IOrderService>(MockBehavior.Strict);
            Controller = new OrderController(Orders.Object, Lookup.Object, Customers.Object, Settings.Object, Toastr.Object, Localizer.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            return this;
        }
    }

    private static OrderViewModel Fresh() => new()
    {
        OrderCode = "7", TaxPercent = 14, DefaultService = 5,
        Menu = new List<CashierMenuItemViewModel> { new() { Id = 1, ArName = "بيتزا", EnName = "Pizza", Price = 40 } }
    };

    // ---------------------------------------------------------------- list

    [Fact]
    public async Task List_HandsTheRequestAndTheUserToTheService_AndAFailureEndsOnTheErrorPage()
    {
        var cashier = new Cashier();
        var request = new PageRequest { Status = "pending" };
        cashier.Orders.Setup(a => a.GetPagedOrdersAsync(request))
            .ReturnsAsync(PaginatedResult<OrderViewModel>.Create(new List<OrderViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await cashier.Controller.List(request));

        cashier.Orders.Setup(a => a.GetPagedOrdersAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await cashier.Controller.List(new PageRequest())).ViewName);
    }

    // ---------------------------------------------------------------- new order

    [Fact]
    public async Task Upsert_NewOrder_ShowsTheCashierWithItsLists()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetNewOrderAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(Fresh());

        var result = await cashier.Controller.Upsert(null, null);

        var model = Assert.IsType<OrderViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model.Customers);
        Assert.Single(model.Tables);
        Assert.Single(model.DeliveryDrivers);
        Assert.Single(model.DeliveryZones);
        cashier.Orders.Verify(a => a.GetNewOrderAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task Upsert_ForATable_StartsFromItsReservation_OrBacksOffWhenThereIsNone()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetNewOrderAsync(5, 9)).ReturnsAsync(Fresh());

        var found = await cashier.Controller.Upsert(5, 9);
        var missing = await cashier.Controller.Upsert(6, null);

        Assert.IsType<ViewResult>(found);
        Assert.Equal("List", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        cashier.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
    }

    [Fact]
    public async Task Upsert_OpeningTheCashier_WhenAServiceFails_EndsOnTheErrorPage()
    {
        var cashier = new Cashier().OrdersFail();

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await cashier.Controller.Upsert(null, null)).ViewName);
    }

    // ---------------------------------------------------------------- save an order

    private static OrderViewModel Posted() => new()
    {
        Items = new List<OrderItemViewModel>
        {
            new() { MenuItemId = 1, Quantity = 2 },
            new() { MenuItemId = 99, Quantity = 1 } // a dish that no longer exists
        }
    };

    [Fact]
    public async Task Save_InvalidForm_RebuildsTheCart_DropsUnknownDishes_AndShowsTheCashierAgain()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetNewOrderAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(Fresh());
        cashier.Controller.ModelState.AddModelError("Items", "required");

        var result = await cashier.Controller.Upsert(Posted());

        var model = Assert.IsType<OrderViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Upsert", ((ViewResult)result).ViewName);
        var line = Assert.Single(model.Items);
        Assert.Equal("Pizza", line.MenuItemEnName);
        Assert.Equal(40, line.PriceForUnit);
        Assert.Equal(80, line.Price);
        Assert.Equal("7", model.OrderCode);
        Assert.Equal(14, model.TaxPercent);
        Assert.Equal(5, model.DefaultService);
        cashier.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        cashier.Orders.Verify(a => a.CreateOrderAsync(It.IsAny<OrderViewModel>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Save_InvalidForm_WhenThereIsNoFreshOrder_StillShowsTheFormWithEmptyDefaults()
    {
        var cashier = new Cashier();
        cashier.Controller.ModelState.AddModelError("Items", "required");

        var result = await cashier.Controller.Upsert(Posted());

        var model = Assert.IsType<OrderViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Items);
        Assert.Null(model.OrderCode);
        Assert.Equal(0, model.TaxPercent);
    }

    [Fact]
    public async Task Save_Succeeds_GoesToTheReceipt()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.CreateOrderAsync(It.IsAny<OrderViewModel>(), It.IsAny<string>())).ReturnsAsync(new ResultMessage { Success = true, Data = "12" });

        var result = await cashier.Controller.Upsert(Posted());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(12, redirect.RouteValues["id"]);
    }

    [Theory]
    [InlineData("InsufficientStock", "InsufficientStock")]
    [InlineData(null, "ErrorWhileSaving")]
    public async Task Save_Refused_ShowsTheCashierAgainWithTheMessage(string message, string shown)
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetNewOrderAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(Fresh());
        cashier.Orders.Setup(a => a.CreateOrderAsync(It.IsAny<OrderViewModel>(), It.IsAny<string>())).ReturnsAsync(new ResultMessage { Success = false, Message = message });

        var result = await cashier.Controller.Upsert(Posted());

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Upsert", view.ViewName);
        var model = Assert.IsType<OrderViewModel>(view.Model);
        Assert.Single(model.Items);
        Assert.Single(model.Customers);
        cashier.Toastr.Verify(a => a.Error(shown), Times.Once);
    }

    [Fact]
    public async Task Save_WhenAServiceFails_EndsOnTheErrorPage()
    {
        var cashier = new Cashier().OrdersFail();

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await cashier.Controller.Upsert(Posted())).ViewName);
    }

    // ---------------------------------------------------------------- receipt, customer, pdf

    [Fact]
    public async Task Details_ShowsTheOrderWithThePrintSettings_NotFoundWhenMissing_ErrorPageOnFailure()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetOrderByIdAsync(3)).ReturnsAsync(new OrderViewModel { Id = 3 });
        cashier.Settings.Setup(a => a.GetApplicationSettingAsync()).ReturnsAsync(new ApplicationSettingViewModel { PrintAfterOrder = true });

        var found = await cashier.Controller.Details(3);
        var missing = await cashier.Controller.Details(4);

        Assert.IsType<ViewResult>(found);
        Assert.True(((ApplicationSettingViewModel)cashier.Controller.ViewBag.Print).PrintAfterOrder);
        Assert.IsType<NotFoundResult>(missing);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Details(3)).ViewName);
    }

    [Fact]
    public async Task Pdf_OfAnUnknownOrder_IsNotFound()
    {
        Assert.IsType<NotFoundResult>(await new Cashier().Controller.Pdf(404));
    }

    [Fact]
    public async Task CustomerInfo_FillsTheCashierFromTheCustomer_NotFoundWhenMissing()
    {
        var cashier = new Cashier();
        cashier.Customers.Setup(a => a.GetCustomerByIdAsync(1)).ReturnsAsync(new CustomerViewModel { ArName = "علي", EnName = "Ali", Address = "Street", PhoneNumber = "0100" });

        var found = await cashier.Controller.CustomerInfo(1);
        var missing = await cashier.Controller.CustomerInfo(2);

        var value = Assert.IsType<JsonResult>(found).Value;
        Assert.Equal("Ali", value.GetType().GetProperty("EnName").GetValue(value));
        Assert.Equal("0100", value.GetType().GetProperty("PhoneNumber").GetValue(value));
        Assert.IsType<NotFoundResult>(missing);
    }

    // ---------------------------------------------------------------- pay

    [Fact]
    public async Task Pay_Succeeds_GoesToTheReceipt_Refused_BacksToThePendingList_AndFailureEndsOnTheErrorPage()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.PayOrderAsync(1, It.IsAny<string>(), PaymentMethod.Visa, It.IsAny<List<Shared.ViewModels.Definitions.OrderPaymentViewModel>>())).ReturnsAsync(new ResultMessage { Success = true });
        cashier.Orders.Setup(a => a.PayOrderAsync(2, It.IsAny<string>(), PaymentMethod.Cash, It.IsAny<List<Shared.ViewModels.Definitions.OrderPaymentViewModel>>())).ReturnsAsync(new ResultMessage { Success = false, Message = "OrderAlreadyPaid" });

        var paid = await cashier.Controller.Pay(1, PaymentMethod.Visa);
        var refused = await cashier.Controller.Pay(2);

        Assert.Equal("Details", Assert.IsType<RedirectToActionResult>(paid).ActionName);
        var back = Assert.IsType<RedirectToActionResult>(refused);
        Assert.Equal("List", back.ActionName);
        Assert.Equal("pending", back.RouteValues["status"]);
        cashier.Toastr.Verify(a => a.Success("OrderPaid"), Times.Once);
        cashier.Toastr.Verify(a => a.Error("OrderAlreadyPaid"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Pay(1)).ViewName);
    }

    // ---------------------------------------------------------------- move and merge a table's order

    [Theory]
    [InlineData("Move")]
    [InlineData("Merge")]
    public async Task MoveOrMerge_Open_OnlyForAnUnpaidOrderOfATable(string action)
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.GetPendingHallOrderByIdAsync(1)).ReturnsAsync(new OrderViewModel { Id = 1, TableId = 5 });
        cashier.Orders.Setup(a => a.GetPendingTablesAsync(5)).ReturnsAsync(new List<LookupItem> { new() { Id = 6 } });
        Task<IActionResult> Open(int id) => action == "Move" ? cashier.Controller.Move(id) : cashier.Controller.Merge(id);

        var ok = await Open(1);
        var paid = await Open(2);
        var noTable = await Open(3);
        var unknown = await Open(4);

        Assert.IsType<ViewResult>(ok);
        Assert.Single((IEnumerable<LookupItem>)cashier.Controller.ViewBag.Tables);
        foreach (var refused in new[] { paid, noTable, unknown })
        {
            Assert.Equal("List", Assert.IsType<RedirectToActionResult>(refused).ActionName);
        }

        cashier.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Exactly(3));
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Move(1)).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Merge(1)).ViewName);
    }

    [Fact]
    public async Task Move_Post_Succeeds_OrBacksToThePendingListWithTheMessage()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.MoveItemAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 1))).ReturnsAsync(new ResultMessage { Success = true });
        cashier.Orders.Setup(a => a.MoveItemAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 2))).ReturnsAsync(new ResultMessage { Success = false, Message = "TableBusy" });
        cashier.Orders.Setup(a => a.MoveItemAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 3))).ReturnsAsync(new ResultMessage { Success = false });

        await cashier.Controller.Move(new OrderMoveViewModel { OrderMenuItemId = 1 });
        await cashier.Controller.Move(new OrderMoveViewModel { OrderMenuItemId = 2 });
        await cashier.Controller.Move(new OrderMoveViewModel { OrderMenuItemId = 3 });

        cashier.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        cashier.Toastr.Verify(a => a.Error("TableBusy"), Times.Once);
        cashier.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Move(new OrderMoveViewModel())).ViewName);
    }

    [Fact]
    public async Task Merge_Post_Succeeds_OrBacksToThePendingListWithTheMessage()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.MergeOrderAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 1))).ReturnsAsync(new ResultMessage { Success = true });
        cashier.Orders.Setup(a => a.MergeOrderAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 2))).ReturnsAsync(new ResultMessage { Success = false, Message = "TableBusy" });
        cashier.Orders.Setup(a => a.MergeOrderAsync(It.Is<OrderMoveViewModel>(m => m.OrderMenuItemId == 3))).ReturnsAsync(new ResultMessage { Success = false });

        var ok = await cashier.Controller.Merge(new OrderMoveViewModel { OrderMenuItemId = 1 });
        await cashier.Controller.Merge(new OrderMoveViewModel { OrderMenuItemId = 2 });
        await cashier.Controller.Merge(new OrderMoveViewModel { OrderMenuItemId = 3 });

        Assert.Equal("pending", ((RedirectToActionResult)ok).RouteValues["status"]);
        cashier.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        cashier.Toastr.Verify(a => a.Error("TableBusy"), Times.Once);
        cashier.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await new Cashier().OrdersFail().Controller.Merge(new OrderMoveViewModel())).ViewName);
    }

    // ---------------------------------------------------------------- delete

    [Fact]
    public async Task Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var cashier = new Cashier();
        cashier.Orders.Setup(a => a.DeleteOrderAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        cashier.Orders.Setup(a => a.DeleteOrderAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "OrderPaidCannotDelete" });

        var ok = (JsonResult)await cashier.Controller.Delete(1);
        var refused = (JsonResult)await cashier.Controller.Delete(2);
        var failed = (JsonResult)await new Cashier().OrdersFail().Controller.Delete(3);

        Assert.Equal(true, ok.Value.GetType().GetProperty("success").GetValue(ok.Value));
        Assert.Equal("OrderPaidCannotDelete", refused.Value.GetType().GetProperty("errorMessage").GetValue(refused.Value));
        Assert.Equal("SomethingWentError", failed.Value.GetType().GetProperty("errorMessage").GetValue(failed.Value));
    }
}
