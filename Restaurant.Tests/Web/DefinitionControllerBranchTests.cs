using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using ResturantPL.Controllers.Definitions;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// The error and refusal branches of every definition controller (service + toastr + localizer constructor):
// a failing service must end on the error page / JSON failure, never crash, and an invalid form never reaches the service.
public class DefinitionControllerBranchTests
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    public static IEnumerable<object[]> DefinitionControllers() =>
        typeof(KitchenController).Assembly.GetTypes()
            .Where(a => typeof(Controller).IsAssignableFrom(a) && !a.IsAbstract)
            .Where(a =>
            {
                var parameters = a.GetConstructors().Single().GetParameters();
                return parameters.Length == 3
                    && parameters[0].ParameterType.Name.EndsWith("Service")
                    && parameters[1].ParameterType == typeof(IToastrService)
                    && parameters[2].ParameterType == typeof(IStringLocalizer<SharedResource>);
            })
            // the int-keyed template only: Role keys by string and Entry has an Upsert without an id
            .Where(a => a.GetMethods().Any(m => m.Name == "Delete" && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(int) }))
                     && a.GetMethods().Any(m => m.Name == "Upsert" && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(int?) })))
            .Select(a => new object[] { a });

    private static Controller Build(Type controller, MockBehavior behavior, out Mock<IToastrService> toastr)
    {
        var serviceType = controller.GetConstructors().Single().GetParameters()[0].ParameterType;
        var service = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType), behavior);
        toastr = new Mock<IToastrService>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

        var built = (Controller)Activator.CreateInstance(controller, service.Object, toastr.Object, localizer.Object);
        // some actions read the signed-in user
        built.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        return built;
    }

    private static async Task<IActionResult> Call(Controller controller, MethodInfo action, params object[] arguments) =>
        (IActionResult)await (dynamic)action.Invoke(controller, arguments);

    private static MethodInfo UpsertGet(Type controller) =>
        controller.GetMethods().Single(m => m.Name == "Upsert" && m.GetParameters().Single().ParameterType == typeof(int?));

    private static MethodInfo UpsertPost(Type controller) =>
        controller.GetMethods().Single(m => m.Name == "Upsert" && m.GetParameters().Single().ParameterType != typeof(int?));

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task Index_WhenTheServiceFails_ShowsTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _);

        var result = await Call(controller, type.GetMethod("Index"), new PageRequest());

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(result).ViewName);
    }

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task UpsertGet_WhenTheServiceFails_ShowsTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _);

        var result = await Call(controller, UpsertGet(type), 5);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(result).ViewName);
    }

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task UpsertGet_UnknownId_RedirectsToTheListWithAMessage(Type type)
    {
        // a loose mock answers every service call with null, which is "record not found"
        var controller = Build(type, MockBehavior.Loose, out var toastr);

        var result = await Call(controller, UpsertGet(type), 5);

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(result).ActionName);
        toastr.Verify(a => a.Error("RecordNotFound"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task UpsertPost_InvalidModel_ShowsTheFormAgainWithoutCallingTheService(Type type)
    {
        // strict mock: any service call would throw and end on the error page instead of the form
        var controller = Build(type, MockBehavior.Strict, out var toastr);
        controller.ModelState.AddModelError("ArName", "required");
        var action = UpsertPost(type);
        var model = Activator.CreateInstance(action.GetParameters().Single().ParameterType);

        var result = await Call(controller, action, model);

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        toastr.Verify(a => a.Error("ValidationError"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task UpsertPost_WhenTheServiceFails_ShowsTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _);
        var action = UpsertPost(type);
        var model = Activator.CreateInstance(action.GetParameters().Single().ParameterType);

        var result = await Call(controller, action, model);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(result).ViewName);
    }

    [Theory]
    [MemberData(nameof(DefinitionControllers))]
    public async Task Delete_WhenTheServiceFails_AnswersJsonWithAGenericMessage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _);

        var result = await Call(controller, type.GetMethod("Delete"), 5);

        var value = Assert.IsType<JsonResult>(result).Value;
        Assert.Equal(false, value.GetType().GetProperty("success").GetValue(value));
        Assert.Equal("SomethingWentError", value.GetType().GetProperty("errorMessage").GetValue(value));
    }
}
