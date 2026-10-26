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

// Roles, users, employee accounts and table reservations: the screens whose keys are strings or that have more than one action.
public class IdentityScreenControllerTests
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

    // a controller reuses one ViewData for every View(...) it returns, so the model is read right after its own call
    private static T ModelOf<T>(IActionResult result) => (T)Assert.IsType<ViewResult>(result).Model;

    private static object Prop(IActionResult result, string name)
    {
        var value = Assert.IsType<JsonResult>(result).Value;

        return value.GetType().GetProperty(name).GetValue(value);
    }

    // ---------------------------------------------------------------- roles

    private class Roles
    {
        public Mock<IRoleService> Service = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public RoleController Controller;

        public Roles() => Controller = WithContext(new RoleController(Service.Object, Toastr.Object, Localizer().Object));
    }

    [Fact]
    public async Task Role_Index_ListsRoles_AndAFailureEndsOnTheErrorPage()
    {
        var roles = new Roles();
        roles.Service.Setup(a => a.GetPagedRolesAsync(It.IsAny<PageRequest>())).ReturnsAsync(PaginatedResult<RoleViewModel>.Create(new List<RoleViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await roles.Controller.Index(null));

        roles.Service.Setup(a => a.GetPagedRolesAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await roles.Controller.Index(new PageRequest())).ViewName);
    }

    [Fact]
    public async Task Role_Open_NewOrExisting_OrBackToTheListWhenMissing_OrTheErrorPage()
    {
        var roles = new Roles();
        roles.Service.Setup(a => a.GetRoleByIdAsync("r1")).ReturnsAsync(new RoleViewModel { Id = "r1", Name = "Cashier" });
        roles.Service.Setup(a => a.GetRoleByIdAsync("boom")).ThrowsAsync(new InvalidOperationException());

        var fresh = ModelOf<RoleViewModel>(await roles.Controller.Upsert((string)null));
        var found = ModelOf<RoleViewModel>(await roles.Controller.Upsert("r1"));
        var missing = await roles.Controller.Upsert("r2");

        Assert.Null(fresh.Id);
        Assert.Equal("Cashier", found.Name);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        roles.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await roles.Controller.Upsert("boom")).ViewName);
    }

    [Fact]
    public async Task Role_Save_InvalidForm_ShowsItAgain_WithoutCallingTheService()
    {
        var roles = new Roles();
        roles.Controller.ModelState.AddModelError("Name", "required");

        var result = await roles.Controller.Upsert(new RoleViewModel());

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        roles.Toastr.Verify(a => a.Error("ValidationError"), Times.Once);
        roles.Service.Verify(a => a.UpsertRoleAsync(It.IsAny<RoleViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Role_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var roles = new Roles();
        roles.Service.SetupSequence(a => a.UpsertRoleAsync(It.IsAny<RoleViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "RoleNameExists" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var created = await roles.Controller.Upsert(new RoleViewModel { Name = "Chef" });
        var edited = await roles.Controller.Upsert(new RoleViewModel { Id = "r1", Name = "Chef" });
        var refused = await roles.Controller.Upsert(new RoleViewModel { Name = "Chef" });
        var bare = await roles.Controller.Upsert(new RoleViewModel { Name = "Chef" });
        var failed = await roles.Controller.Upsert(new RoleViewModel { Name = "Chef" });

        Assert.Equal("Chef", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        roles.Toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        roles.Toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
        roles.Toastr.Verify(a => a.Error("RoleNameExists"), Times.Once);
        roles.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
    }

    [Fact]
    public async Task Role_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var roles = new Roles();
        roles.Service.Setup(a => a.DeleteRoleAsync("a")).ReturnsAsync(new ResultMessage { Success = true });
        roles.Service.Setup(a => a.DeleteRoleAsync("b")).ReturnsAsync(new ResultMessage { Success = false, Message = "RoleHasUsers" });
        roles.Service.Setup(a => a.DeleteRoleAsync("c")).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await roles.Controller.Delete("a"), "success"));
        Assert.Equal("RoleHasUsers", Prop(await roles.Controller.Delete("b"), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await roles.Controller.Delete("c"), "errorMessage"));
    }

    // ---------------------------------------------------------------- users

    private class Users
    {
        public Mock<IUserService> Service = new(MockBehavior.Loose);
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public UserController Controller;

        public Users()
        {
            Lookup.Setup(a => a.RoleLookupAsync()).ReturnsAsync(new List<LookupItem_String> { new() { Id = "r1" } });
            Controller = WithContext(new UserController(Service.Object, Lookup.Object, Toastr.Object, Localizer().Object));
        }
    }

    [Fact]
    public async Task User_Index_ListsUsers_AndAFailureEndsOnTheErrorPage()
    {
        var users = new Users();
        users.Service.Setup(a => a.GetPagedUsersAsync(It.IsAny<PageRequest>())).ReturnsAsync(PaginatedResult<UserViewModel>.Create(new List<UserViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await users.Controller.Index(null));

        users.Service.Setup(a => a.GetPagedUsersAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await users.Controller.Index(new PageRequest())).ViewName);
    }

    [Fact]
    public async Task User_Open_NewOrExistingWithTheRolesToPick_OrBackToTheListWhenMissing_OrTheErrorPage()
    {
        var users = new Users();
        users.Service.Setup(a => a.GetUserByIdAsync("u1")).ReturnsAsync(new UserViewModel { Id = "u1", Email = "a@test.com" });
        users.Service.Setup(a => a.GetUserByIdAsync("boom")).ThrowsAsync(new InvalidOperationException());

        var fresh = ModelOf<UserViewModel>(await users.Controller.Upsert((string)null));
        var found = ModelOf<UserViewModel>(await users.Controller.Upsert("u1"));
        var missing = await users.Controller.Upsert("u2");

        Assert.Single(fresh.Roles);
        Assert.Equal("a@test.com", found.Email);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        users.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await users.Controller.Upsert("boom")).ViewName);
    }

    [Fact]
    public async Task User_Save_InvalidForm_ShowsItAgainWithTheRoles()
    {
        var users = new Users();
        users.Controller.ModelState.AddModelError("Email", "required");

        var result = await users.Controller.Upsert(new UserViewModel());

        var model = Assert.IsType<UserViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model.Roles);
        users.Service.Verify(a => a.UpsertUserAsync(It.IsAny<UserViewModel>()), Times.Never);
    }

    [Fact]
    public async Task User_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var users = new Users();
        users.Service.SetupSequence(a => a.UpsertUserAsync(It.IsAny<UserViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "EmailTaken" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var created = await users.Controller.Upsert(new UserViewModel { Email = "a@test.com" });
        var edited = await users.Controller.Upsert(new UserViewModel { Id = "u1", Email = "a@test.com" });
        var refused = await users.Controller.Upsert(new UserViewModel { Email = "a@test.com" });
        var bare = await users.Controller.Upsert(new UserViewModel { Email = "a@test.com" });
        var failed = await users.Controller.Upsert(new UserViewModel { Email = "a@test.com" });

        Assert.Equal("a@test.com", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(refused).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        users.Toastr.Verify(a => a.Error("EmailTaken"), Times.Once);
        users.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
    }

    [Fact]
    public async Task User_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var users = new Users();
        users.Service.Setup(a => a.DeleteUserAsync("a")).ReturnsAsync(new ResultMessage { Success = true });
        users.Service.Setup(a => a.DeleteUserAsync("b")).ReturnsAsync(new ResultMessage { Success = false, Message = "CannotDeleteYourself" });
        users.Service.Setup(a => a.DeleteUserAsync("c")).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await users.Controller.Delete("a"), "success"));
        Assert.Equal("CannotDeleteYourself", Prop(await users.Controller.Delete("b"), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await users.Controller.Delete("c"), "errorMessage"));
    }

    // ---------------------------------------------------------------- employee accounts

    private class EmployeeAccounts
    {
        public Mock<IEmployeeAccountService> Service = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public EmployeeAccountController Controller;

        public EmployeeAccounts()
        {
            Service.Setup(a => a.GetEmployeesWithoutAccountAsync()).ReturnsAsync(new List<LookupItem> { new() { Id = 8 } });
            Controller = WithContext(new EmployeeAccountController(Service.Object, Toastr.Object, Localizer().Object));
        }
    }

    [Fact]
    public async Task EmployeeAccount_Index_ListsAccounts_AndAFailureEndsOnTheErrorPage()
    {
        var accounts = new EmployeeAccounts();
        accounts.Service.Setup(a => a.GetPagedAccountsAsync(It.IsAny<PageRequest>())).ReturnsAsync(PaginatedResult<EmployeeAccountViewModel>.Create(new List<EmployeeAccountViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await accounts.Controller.Index(null));

        accounts.Service.Setup(a => a.GetPagedAccountsAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await accounts.Controller.Index(new PageRequest())).ViewName);
    }

    [Fact]
    public async Task EmployeeAccount_NewAccount_OffersTheEmployeesThatHaveNoAccountYet()
    {
        var accounts = new EmployeeAccounts();

        var result = await accounts.Controller.Upsert((string)null, null);

        var model = Assert.IsType<EmployeeAccountViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model.Employees);
    }

    [Fact]
    public async Task EmployeeAccount_ExistingAccount_OrAnEmployeeWithoutOne_OpensItsFormWithoutTheList()
    {
        var accounts = new EmployeeAccounts();
        accounts.Service.Setup(a => a.GetAccountByIdAsync("u1")).ReturnsAsync(new EmployeeAccountViewModel { Id = "u1", EmployeeId = 3 });
        accounts.Service.Setup(a => a.GetAccountByEmployeeIdAsync(5)).ReturnsAsync(new EmployeeAccountViewModel { EmployeeId = 5 });

        var existing = ModelOf<EmployeeAccountViewModel>(await accounts.Controller.Upsert("u1", null));
        var forEmployee = ModelOf<EmployeeAccountViewModel>(await accounts.Controller.Upsert(null, 5));

        Assert.Empty(existing.Employees);
        Assert.Empty(forEmployee.Employees);
    }

    [Fact]
    public async Task EmployeeAccount_Unknown_BacksToTheListWithAMessage_AndAFailureEndsOnTheErrorPage()
    {
        var accounts = new EmployeeAccounts();
        accounts.Service.Setup(a => a.GetAccountByIdAsync("boom")).ThrowsAsync(new InvalidOperationException());

        var byId = await accounts.Controller.Upsert("nobody", null);
        var byEmployee = await accounts.Controller.Upsert(null, 404);

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(byId).ActionName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(byEmployee).ActionName);
        accounts.Toastr.Verify(a => a.Error("RecordNotFound"), Times.Exactly(2));
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await accounts.Controller.Upsert("boom", null)).ViewName);
    }

    [Fact]
    public async Task EmployeeAccount_Save_InvalidForm_ShowsItAgainWithTheEmployeesOfANewAccount()
    {
        var accounts = new EmployeeAccounts();
        accounts.Controller.ModelState.AddModelError("Password", "required");

        var fresh = ModelOf<EmployeeAccountViewModel>(await accounts.Controller.Upsert(new EmployeeAccountViewModel()));
        var existing = ModelOf<EmployeeAccountViewModel>(await accounts.Controller.Upsert(new EmployeeAccountViewModel { Id = "u1", EmployeeId = 3 }));

        Assert.Single(fresh.Employees);
        Assert.Empty(existing.Employees);
        accounts.Service.Verify(a => a.UpsertAccountAsync(It.IsAny<EmployeeAccountViewModel>()), Times.Never);
    }

    [Fact]
    public async Task EmployeeAccount_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var accounts = new EmployeeAccounts();
        accounts.Service.SetupSequence(a => a.UpsertAccountAsync(It.IsAny<EmployeeAccountViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "UserNameTaken" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var created = await accounts.Controller.Upsert(new EmployeeAccountViewModel { UserName = "sara" });
        var edited = await accounts.Controller.Upsert(new EmployeeAccountViewModel { Id = "u1", UserName = "sara", EmployeeId = 3 });
        var refused = ModelOf<EmployeeAccountViewModel>(await accounts.Controller.Upsert(new EmployeeAccountViewModel { UserName = "sara" }));
        var bare = await accounts.Controller.Upsert(new EmployeeAccountViewModel { UserName = "sara" });
        var failed = await accounts.Controller.Upsert(new EmployeeAccountViewModel { UserName = "sara" });

        Assert.Equal("sara", ((RedirectToActionResult)created).RouteValues["searchString"]);
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.Single(refused.Employees);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bare).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        accounts.Toastr.Verify(a => a.Error("UserNameTaken"), Times.Once);
        accounts.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
    }

    // ---------------------------------------------------------------- table reservations

    private class Reservations
    {
        public Mock<ITableReservationService> Service = new(MockBehavior.Loose);
        public Mock<ILookupService> Lookup = new(MockBehavior.Loose);
        public Mock<IToastrService> Toastr = new();
        public TableReservationController Controller;

        public Reservations()
        {
            Lookup.Setup(a => a.NamedLookupAsync<Restaurant.DAL.Models.Table>()).ReturnsAsync(new List<LookupItem> { new() { Id = 1 } });
            Controller = WithContext(new TableReservationController(Service.Object, Lookup.Object, Toastr.Object, Localizer().Object));
            Controller.Url = Mock.Of<IUrlHelper>(u => u.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>()) == "/Order/Upsert?tableId=4");
        }
    }

    [Fact]
    public async Task Reservation_Index_ListsByStatus_AndAFailureEndsOnTheErrorPage()
    {
        var reservations = new Reservations();
        reservations.Service.Setup(a => a.GetPagedReservationsAsync(It.IsAny<PageRequest>(), ReservationStatus.Confirmed))
            .ReturnsAsync(PaginatedResult<TableReservationViewModel>.Create(new List<TableReservationViewModel>(), 0, new PageRequest()));

        Assert.IsType<ViewResult>(await reservations.Controller.Index(null, ReservationStatus.Confirmed));

        reservations.Service.Setup(a => a.GetPagedReservationsAsync(It.IsAny<PageRequest>(), It.IsAny<ReservationStatus?>())).ThrowsAsync(new InvalidOperationException());
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await reservations.Controller.Index(new PageRequest(), null)).ViewName);
    }

    [Fact]
    public async Task Reservation_Open_NewOrExistingWithTheTables_OrBackToTheListWhenMissing_OrTheErrorPage()
    {
        var reservations = new Reservations();
        reservations.Service.Setup(a => a.GetReservationByIdAsync(1)).ReturnsAsync(new TableReservationViewModel { Id = 1, CustomerName = "Ali" });
        reservations.Service.Setup(a => a.GetReservationByIdAsync(9)).ThrowsAsync(new InvalidOperationException());

        var fresh = ModelOf<TableReservationViewModel>(await reservations.Controller.Upsert((int?)null));
        var found = ModelOf<TableReservationViewModel>(await reservations.Controller.Upsert(1));
        var missing = await reservations.Controller.Upsert(2);

        Assert.Single(fresh.Tables);
        Assert.Equal("Ali", found.CustomerName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(await reservations.Controller.Upsert(9)).ViewName);
    }

    [Fact]
    public async Task Reservation_Save_Succeeds_RefusesWithOrWithoutAMessage_OrFails()
    {
        var reservations = new Reservations();
        reservations.Service.SetupSequence(a => a.UpsertReservationAsync(It.IsAny<TableReservationViewModel>()))
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = true })
            .ReturnsAsync(new ResultMessage { Success = false, Message = "Reservation.Error.TableTaken" })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ReturnsAsync(new ResultMessage { Success = false })
            .ThrowsAsync(new InvalidOperationException());

        var created = await reservations.Controller.Upsert(new TableReservationViewModel());
        var edited = await reservations.Controller.Upsert(new TableReservationViewModel { Id = 3 });
        var refused = ModelOf<TableReservationViewModel>(await reservations.Controller.Upsert(new TableReservationViewModel()));
        var bareNew = await reservations.Controller.Upsert(new TableReservationViewModel());
        var bareEdit = await reservations.Controller.Upsert(new TableReservationViewModel { Id = 3 });
        var failed = await reservations.Controller.Upsert(new TableReservationViewModel());

        Assert.IsType<RedirectToActionResult>(created);
        Assert.IsType<RedirectToActionResult>(edited);
        Assert.Single(refused.Tables);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareNew).ViewName);
        Assert.Equal("Upsert", Assert.IsType<ViewResult>(bareEdit).ViewName);
        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(failed).ViewName);
        reservations.Toastr.Verify(a => a.Error("Reservation.Error.TableTaken"), Times.Once);
        reservations.Toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
        reservations.Toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    [Fact]
    public async Task Reservation_SetStatus_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var reservations = new Reservations();
        reservations.Service.Setup(a => a.SetStatusAsync(1, ReservationStatus.Confirmed, null)).ReturnsAsync(new ResultMessage { Success = true });
        reservations.Service.Setup(a => a.SetStatusAsync(2, ReservationStatus.Rejected, "full")).ReturnsAsync(new ResultMessage { Success = false, Message = "Reservation.Error.Closed" });
        reservations.Service.Setup(a => a.SetStatusAsync(3, It.IsAny<ReservationStatus>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await reservations.Controller.SetStatus(1, ReservationStatus.Confirmed, null), "success"));
        Assert.Equal("Reservation.Error.Closed", Prop(await reservations.Controller.SetStatus(2, ReservationStatus.Rejected, "full"), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await reservations.Controller.SetStatus(3, ReservationStatus.Seated, null), "errorMessage"));
    }

    [Fact]
    public async Task Reservation_Arrive_OpensTheCashierOnItsTable_OrExplainsWhyNot()
    {
        var reservations = new Reservations();
        reservations.Service.Setup(a => a.ArriveAsync(1)).ReturnsAsync(new ResultMessage { Success = true, Data = "4" });
        reservations.Service.Setup(a => a.ArriveAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "Reservation.Error.NotConfirmed" });
        reservations.Service.Setup(a => a.ArriveAsync(3)).ThrowsAsync(new InvalidOperationException());

        var arrived = await reservations.Controller.Arrive(1);

        Assert.Equal(true, Prop(arrived, "success"));
        Assert.Equal("/Order/Upsert?tableId=4", Prop(arrived, "url"));
        Assert.Equal("Reservation.Error.NotConfirmed", Prop(await reservations.Controller.Arrive(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await reservations.Controller.Arrive(3), "errorMessage"));
    }

    [Fact]
    public async Task Reservation_Delete_Succeeds_IsRefusedWithItsMessage_OrFailsWithTheGenericOne()
    {
        var reservations = new Reservations();
        reservations.Service.Setup(a => a.DeleteReservationAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        reservations.Service.Setup(a => a.DeleteReservationAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "Reservation.Error.Seated" });
        reservations.Service.Setup(a => a.DeleteReservationAsync(3)).ThrowsAsync(new InvalidOperationException());

        Assert.Equal(true, Prop(await reservations.Controller.Delete(1), "success"));
        Assert.Equal("Reservation.Error.Seated", Prop(await reservations.Controller.Delete(2), "errorMessage"));
        Assert.Equal("SomethingWentError", Prop(await reservations.Controller.Delete(3), "errorMessage"));
    }
}
