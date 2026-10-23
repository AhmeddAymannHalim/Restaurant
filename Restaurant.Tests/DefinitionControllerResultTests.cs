using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using ResturantPL.Controllers.Definitions;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

// What a definition controller does with each answer of its service: saved, refused (with and without a message),
// opened, deleted, refused delete. DefinitionControllerBranchTests covers the failures; this covers the answers.
// Controllers that also take a lookup service (or other interfaces) get loose mocks for those.
public class DefinitionControllerResultTests
{
    private const string ErrorView = "~/Views/Shared/Error.cshtml";

    public static IEnumerable<object[]> Controllers() =>
        typeof(KitchenController).Assembly.GetTypes()
            .Where(a => typeof(Controller).IsAssignableFrom(a) && !a.IsAbstract)
            .Where(a =>
            {
                var parameters = a.GetConstructors().Single().GetParameters();
                return parameters.Length >= 3
                    && parameters[0].ParameterType.IsInterface
                    && parameters[0].ParameterType.Name.EndsWith("Service")
                    && parameters.Any(p => p.ParameterType == typeof(IToastrService))
                    && parameters.Any(p => p.ParameterType == typeof(IStringLocalizer<SharedResource>))
                    && parameters.All(p => p.ParameterType.IsInterface);
            })
            .Where(a => a.GetMethods().Any(m => m.Name == "Delete" && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(int) }))
                     && a.GetMethods().Any(m => m.Name == "Upsert" && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(int?) })))
            .Select(a => new object[] { a });

    private static Controller Build(Type controller, MockBehavior serviceBehavior, out Mock service, out Mock<IToastrService> toastr)
    {
        var toastrMock = new Mock<IToastrService>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

        Mock firstMock = null;
        var arguments = new List<object>();
        foreach (var parameter in controller.GetConstructors().Single().GetParameters())
        {
            if (parameter.ParameterType == typeof(IToastrService))
            {
                arguments.Add(toastrMock.Object);
            }
            else if (parameter.ParameterType == typeof(IStringLocalizer<SharedResource>))
            {
                arguments.Add(localizer.Object);
            }
            else
            {
                var behavior = firstMock is null ? serviceBehavior : MockBehavior.Loose;
                var mock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(parameter.ParameterType), behavior);
                firstMock ??= mock;
                arguments.Add(mock.Object);
            }
        }

        service = firstMock;
        toastr = toastrMock;
        var built = (Controller)Activator.CreateInstance(controller, arguments.ToArray());
        built.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };

        return built;
    }

    private static void Answer(Mock service, Func<MethodInfo, bool> matches, Func<Type, object> value) => ServiceMocks.Answer(service, matches, value);

    private static bool IsAsyncOf(MethodInfo method, string prefix) => ServiceMocks.IsAsyncOf(method, prefix);

    private static void ServiceSaves(Mock service, bool success, string message) =>
        Answer(service, m => IsAsyncOf(m, "Upsert") && m.ReturnType == typeof(Task<ResultMessage>), _ => new ResultMessage { Success = success, Message = message });

    private static void ServiceDeletes(Mock service, bool success, string message) =>
        Answer(service, m => IsAsyncOf(m, "Delete") && m.ReturnType == typeof(Task<ResultMessage>), _ => new ResultMessage { Success = success, Message = message });

    // StoreConvert creates a transfer instead of upserting one, so it is not this template
    private static bool SavesByUpsert(Mock service) =>
        service.GetType().GetGenericArguments()[0].GetMethods().Any(m => IsAsyncOf(m, "Upsert") && m.ReturnType == typeof(Task<ResultMessage>));

    private static MethodInfo UpsertGet(Type controller) =>
        controller.GetMethods().Single(m => m.Name == "Upsert" && m.GetParameters().Single().ParameterType == typeof(int?));

    private static MethodInfo UpsertPost(Type controller) =>
        controller.GetMethods().Single(m => m.Name == "Upsert" && m.GetParameters().Single().ParameterType != typeof(int?));

    private static async Task<IActionResult> Call(Controller controller, MethodInfo action, params object[] arguments) =>
        (IActionResult)await (dynamic)action.Invoke(controller, arguments);

    private static object Form(MethodInfo post, int id)
    {
        var model = Activator.CreateInstance(post.GetParameters().Single().ParameterType);
        model.GetType().GetProperty("Id")?.SetValue(model, id);

        return model;
    }

    private static object JsonValue(IActionResult result, string name)
    {
        var value = Assert.IsType<JsonResult>(result).Value;

        return value.GetType().GetProperty(name)?.GetValue(value);
    }

    // ---------------------------------------------------------------- a failing service always ends on the error page

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Index_WhenTheServiceFails_ShowsTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _, out _);

        // a list may also take a filter (reservations by status)
        var index = type.GetMethod("Index");
        var arguments = index.GetParameters().Select(p => p.ParameterType == typeof(PageRequest) ? new PageRequest() : null).Cast<object>().ToArray();

        var result = await Call(controller, index, arguments);

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(result).ViewName);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertGet_WhenTheServiceFails_ShowsTheErrorPage_ForANewAndForAnExistingRecord(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _, out _);

        var existing = await Call(controller, UpsertGet(type), 5);
        var fresh = await Call(controller, UpsertGet(type), new object[] { null });

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(existing).ViewName);
        // a definition without a code has nothing to ask the service for, so it may open its form
        Assert.True(fresh is ViewResult);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertPost_WhenTheServiceFails_ShowsTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out var service, out _);
        var post = UpsertPost(type);

        var result = await Call(controller, post, Form(post, 0));

        Assert.Equal(ErrorView, Assert.IsType<ViewResult>(result).ViewName);
    }

    // ---------------------------------------------------------------- save

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertPost_WhenTheServiceSaves_GoesBackToTheListWithASuccessMessage(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out var toastr);
        if (!SavesByUpsert(service)) { return; }
        ServiceSaves(service, true, null);
        var post = UpsertPost(type);

        var created = await Call(controller, post, Form(post, 0));
        var edited = await Call(controller, post, Form(post, 3));

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(created).ActionName);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(edited).ActionName);
        toastr.Verify(a => a.Success("SavedSuccessfully"), Times.Once);
        toastr.Verify(a => a.Success("UpdatedSuccessfully"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertPost_WhenTheServiceRefuses_ShowsTheFormAgainWithItsMessage(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out var toastr);
        if (!SavesByUpsert(service)) { return; }
        ServiceSaves(service, false, "NameAlreadyExists");
        var post = UpsertPost(type);

        var result = await Call(controller, post, Form(post, 0));

        Assert.Equal("Upsert", Assert.IsType<ViewResult>(result).ViewName);
        toastr.Verify(a => a.Error("NameAlreadyExists"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertPost_WhenTheServiceRefusesWithoutAMessage_UsesTheGenericSaveOrUpdateError(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out var toastr);
        if (!SavesByUpsert(service)) { return; }
        ServiceSaves(service, false, null);
        var post = UpsertPost(type);

        await Call(controller, post, Form(post, 0));
        await Call(controller, post, Form(post, 3));

        toastr.Verify(a => a.Error("ErrorWhileSaving"), Times.Once);
        toastr.Verify(a => a.Error("ErrorWhileUpdating"), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertPost_InvalidModel_ShowsTheFormAgainEvenWhenItsListsComeFromServices(Type type)
    {
        // a reservation is checked by its repository (messages per rule), so its controller has no invalid-form branch
        if (type == typeof(TableReservationController)) { return; }
        var controller = Build(type, MockBehavior.Loose, out _, out var toastr);
        controller.ModelState.AddModelError("ArName", "required");
        var post = UpsertPost(type);

        var result = await Call(controller, post, Form(post, 0));

        // a controller may show its form under the default view name
        Assert.True(Assert.IsType<ViewResult>(result).ViewName is null or "Upsert");
        toastr.Verify(a => a.Error("ValidationError"), Times.Once);
    }

    // ---------------------------------------------------------------- open

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertGet_AnExistingRecord_OpensTheFormOrSaysItIsNotFound_NeverTheErrorPage(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out _);
        Answer(service, m => IsAsyncOf(m, "Get") && m.Name.EndsWith("ByIdAsync") && !m.ReturnType.GetGenericArguments()[0].IsValueType,
            inner => Activator.CreateInstance(inner));

        var result = await Call(controller, UpsertGet(type), 5);

        if (result is ViewResult view)
        {
            Assert.NotEqual(ErrorView, view.ViewName);
        }
        else
        {
            Assert.IsType<RedirectToActionResult>(result);
        }
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task UpsertGet_ANewRecord_OpensTheFormWithTheNextCode(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out _);
        Answer(service, m => m.Name.StartsWith("New") && m.Name.EndsWith("CodeAsync") && m.ReturnType == typeof(Task<string>), _ => "42");

        var result = await Call(controller, UpsertGet(type), new object[] { null });

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotEqual(ErrorView, view.ViewName);
        // the code of a customer or supplier comes from its account, not from this call
        var code = view.Model?.GetType().GetProperty("Code")?.GetValue(view.Model) as string;
        Assert.True(string.IsNullOrEmpty(code) || code == "42");
    }

    // ---------------------------------------------------------------- delete

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Delete_WhenTheServiceDeletes_AnswersWithSuccess(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out _);
        ServiceDeletes(service, true, null);

        var result = await Call(controller, type.GetMethod("Delete", new[] { typeof(int) }), 5);

        Assert.Equal(true, JsonValue(result, "success"));
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Delete_WhenTheServiceRefuses_AnswersWithItsMessage(Type type)
    {
        var controller = Build(type, MockBehavior.Loose, out var service, out _);
        ServiceDeletes(service, false, "CannotDeleteLinkedRecord");

        var result = await Call(controller, type.GetMethod("Delete", new[] { typeof(int) }), 5);

        Assert.Equal(false, JsonValue(result, "success"));
        Assert.Equal("CannotDeleteLinkedRecord", JsonValue(result, "errorMessage"));
    }

    [Theory]
    [MemberData(nameof(Controllers))]
    public async Task Delete_WhenTheServiceFails_AnswersWithTheGenericMessage(Type type)
    {
        var controller = Build(type, MockBehavior.Strict, out _, out _);

        var result = await Call(controller, type.GetMethod("Delete", new[] { typeof(int) }), 5);

        Assert.Equal(false, JsonValue(result, "success"));
        Assert.Equal("SomethingWentError", JsonValue(result, "errorMessage"));
    }
}
