using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Controllers.Definitions;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

public class ControllerTests
{
    // ---------------------------------------------------------------- the rules every controller must follow

    private static IEnumerable<Type> AllControllers() => typeof(KitchenController).Assembly.GetTypes()
        .Where(a => typeof(Controller).IsAssignableFrom(a) && !a.IsAbstract);

    public static IEnumerable<object[]> Controllers() => AllControllers().Select(a => new object[] { a });

    [Theory]
    [MemberData(nameof(Controllers))]
    public void Controller_RequiresSignIn(Type controller)
    {
        // sign in / register, the home redirect and the language switch are public on purpose; Account protects its private actions one by one
        var publicOnPurpose = new[] { "AccountController", "HomeController", "SetLanguageController" }.Contains(controller.Name);
        var anonymous = publicOnPurpose || controller.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        var authorized = controller.GetCustomAttributes<AuthorizeAttribute>().Any();

        Assert.True(authorized || anonymous, controller.Name + " has neither [Authorize] nor [AllowAnonymous]");
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public void Controller_NeverReceivesTheDatabaseOrARepository(Type controller)
    {
        var parameters = controller.GetConstructors().SelectMany(a => a.GetParameters()).Select(a => a.ParameterType);

        Assert.DoesNotContain(parameters, a => a == typeof(Restaurant.DAL.ApplicationDbContext));
        Assert.DoesNotContain(parameters, a => a.Name.EndsWith("Repository") || a.Name.Contains("UnitOfWork"));
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public void Controller_EveryPostActionIsProtectedAgainstForgedForms(Type controller)
    {
        // Delete / Complete style AJAX posts are allowed to skip the token only when they return JSON (the page sends the header)
        foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(a => a.GetCustomAttributes<HttpPostAttribute>().Any()))
        {
            var returnsView = typeof(Task<IActionResult>).IsAssignableFrom(action.ReturnType) && action.Name is "Upsert" or "Create" or "SignIn" or "Register";
            if (returnsView)
            {
                Assert.True(action.GetCustomAttributes<ValidateAntiForgeryTokenAttribute>().Any() || controller.GetCustomAttributes<AutoValidateAntiforgeryTokenAttribute>().Any(),
                    controller.Name + "." + action.Name + " posts a form without an anti-forgery token");
            }
        }
    }

    // ---------------------------------------------------------------- the definition template, on KitchenController

    private readonly Mock<IKitchenService> _service = new();
    private readonly Mock<IToastrService> _toastr = new();
    private readonly Mock<IStringLocalizer<SharedResource>> _localizer = new();

    private KitchenController Controller()
    {
        _localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        return new KitchenController(_service.Object, _toastr.Object, _localizer.Object);
    }

    [Fact]
    public async Task Index_ReturnsThePagedModel()
    {
        var page = PaginatedResult<KitchenViewModel>.Create(new[] { new KitchenViewModel { Id = 1 } }, 1, new PageRequest());
        _service.Setup(a => a.GetPagedKitchensAsync(It.IsAny<PageRequest>())).ReturnsAsync(page);

        var result = await Controller().Index(null);

        Assert.Same(page, Assert.IsType<ViewResult>(result).Model);
    }

    [Fact]
    public async Task Index_WhenTheServiceFails_ShowsTheErrorPageNotACrash()
    {
        _service.Setup(a => a.GetPagedKitchensAsync(It.IsAny<PageRequest>())).ThrowsAsync(new InvalidOperationException("boom"));

        var result = await Controller().Index(new PageRequest());

        Assert.Equal("~/Views/Shared/Error.cshtml", Assert.IsType<ViewResult>(result).ViewName);
    }

    [Fact]
    public async Task UpsertGet_New_GetsTheNextCode()
    {
        _service.Setup(a => a.NewKitchenCodeAsync()).ReturnsAsync("7");

        var result = await Controller().Upsert((int?)null);

        Assert.Equal("7", Assert.IsType<KitchenViewModel>(Assert.IsType<ViewResult>(result).Model).Code);
    }

    [Fact]
    public async Task UpsertGet_Existing_LoadsIt_Unknown_RedirectsWithAMessage()
    {
        _service.Setup(a => a.GetKitchenByIdAsync(3)).ReturnsAsync(new KitchenViewModel { Id = 3, EnName = "Main" });
        _service.Setup(a => a.GetKitchenByIdAsync(4)).ReturnsAsync((KitchenViewModel)null);
        var controller = Controller();

        var found = await controller.Upsert(3);
        var missing = await controller.Upsert(4);

        Assert.Equal("Main", ((KitchenViewModel)Assert.IsType<ViewResult>(found).Model).EnName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(missing).ActionName);
        _toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
    }

    [Fact]
    public async Task UpsertPost_Valid_SavesAndRedirectsToTheListSearchingTheName()
    {
        _service.Setup(a => a.UpsertKitchenAsync(It.IsAny<KitchenViewModel>())).ReturnsAsync(new ResultMessage { Success = true });

        var result = await Controller().Upsert(new KitchenViewModel { ArName = "مطبخ" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("مطبخ", redirect.RouteValues["searchString"]);
        _toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
    }

    [Fact]
    public async Task UpsertPost_Edit_UsesTheUpdatedMessage()
    {
        _service.Setup(a => a.UpsertKitchenAsync(It.IsAny<KitchenViewModel>())).ReturnsAsync(new ResultMessage { Success = true });

        await Controller().Upsert(new KitchenViewModel { Id = 5 });

        _toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
    }

    [Fact]
    public async Task UpsertPost_InvalidModel_ShowsTheFormAgainWithoutSaving()
    {
        var controller = Controller();
        controller.ModelState.AddModelError("EnName", "required");

        var result = await controller.Upsert(new KitchenViewModel());

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        _service.Verify(a => a.UpsertKitchenAsync(It.IsAny<KitchenViewModel>()), Times.Never);
        _toastr.Verify(a => a.Error("ValidationError"), Times.Once);
    }

    [Fact]
    public async Task UpsertPost_ServiceRefuses_ShowsTheLocalizedReasonOnTheForm()
    {
        _service.Setup(a => a.UpsertKitchenAsync(It.IsAny<KitchenViewModel>())).ReturnsAsync(new ResultMessage { Success = false, Message = "RecordNotFound" });

        var result = await Controller().Upsert(new KitchenViewModel { Id = 9 });

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        _toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsJson_ForSuccessRefusalAndCrash()
    {
        _service.Setup(a => a.DeleteKitchenAsync(1)).ReturnsAsync(new ResultMessage { Success = true });
        _service.Setup(a => a.DeleteKitchenAsync(2)).ReturnsAsync(new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" });
        _service.Setup(a => a.DeleteKitchenAsync(3)).ThrowsAsync(new Exception("boom"));
        var controller = Controller();

        Assert.Equal(true, Property(await controller.Delete(1), "success"));
        var refused = await controller.Delete(2);
        Assert.Equal(false, Property(refused, "success"));
        Assert.Equal("CannotDeleteLinkedRecord", Property(refused, "errorMessage"));
        var crashed = await controller.Delete(3);
        Assert.Equal(false, Property(crashed, "success"));
        Assert.Equal("SomethingWentError", Property(crashed, "errorMessage"));
    }

    private static object Property(IActionResult result, string name) =>
        Assert.IsType<JsonResult>(result).Value.GetType().GetProperty(name).GetValue(((JsonResult)result).Value);
}

// senior-halim rule: every method of a repository, service or controller is public and reads top to bottom on its own (controllers may keep PopulateData when they load 2+ lookups)
public class FlatCodeRuleTests
{
    public static IEnumerable<object[]> Types()
    {
        var all = new[]
        {
            typeof(Restaurant.DAL.Repositories.Definitions.KitchenRepository).Assembly,
            typeof(Restaurant.BLL.Services.Definitions.KitchenService).Assembly,
            typeof(ResturantPL.Controllers.Definitions.KitchenController).Assembly
        };

        return all.SelectMany(a => a.GetTypes())
            .Where(t => (t.Namespace ?? "").Contains("Repositories") && !(t.Namespace ?? "").Contains("IRepositories")
                     || (t.Namespace ?? "").StartsWith("Restaurant.BLL.Services.Definitions")
                     || (t.Namespace ?? "").StartsWith("Restaurant.BLL.Services.Commons")
                     || (t.Namespace ?? "").StartsWith("ResturantPL.Controllers"))
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested && !t.Name.Contains("<>"))
            .Select(t => new object[] { t });
    }

    [Theory]
    [MemberData(nameof(Types))]
    public void NoPrivateMethods(Type type)
    {
        var privateMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && !m.Name.Contains("<") && m.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() == null)
            // the one allowed exception: a controller that loads two or more lookups for its form keeps them in PopulateData
            .Where(m => !(type.Name.EndsWith("Controller") && m.Name == "PopulateData"))
            .Select(m => m.Name)
            .ToList();

        Assert.True(privateMethods.Count == 0, type.Name + " has private methods: " + string.Join(", ", privateMethods));
    }
}
