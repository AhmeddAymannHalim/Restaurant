using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Restaurant.Tests;

// Every public action of every controller, called through the real service graph with a signed-in manager:
// each one must hand back a result (a view, redirect, json or file) and never let an exception escape.
public class ControllerActionTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private static IEnumerable<Type> Controllers() => typeof(ResturantPL.Controllers.Definitions.KitchenController).Assembly.GetTypes()
        .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract);

    private static IEnumerable<MethodInfo> Actions(Type controller) => controller
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => typeof(IActionResult).IsAssignableFrom(m.ReturnType)
                    || (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) && typeof(IActionResult).IsAssignableFrom(m.ReturnType.GetGenericArguments()[0])))
        .Where(m => !m.IsSpecialName);

    public static IEnumerable<object[]> AllActions() => Controllers()
        .SelectMany(c => Actions(c).Select((m, i) => new object[] { c.Name + "|" + m.Name + "|" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) }));

    private (Controller controller, MethodInfo action) Build(string key)
    {
        var parts = key.Split('|');
        var type = Controllers().Single(c => c.Name == parts[0]);
        var action = Actions(type).Single(m => m.Name == parts[1] && string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) == parts[2]);
        var controller = (Controller)_host.Get(type);

        var http = new DefaultHttpContext { RequestServices = _host.Services, User = TestHost.Manager() };
        http.Request.Scheme = "http";
        http.Request.Host = new HostString("127.0.0.1");
        http.Features.Set<ISessionFeature>(new SessionFeature { Session = new FakeSession() });
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/x");
        url.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        controller.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        controller.Url = url.Object;
        controller.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());

        return (controller, action);
    }

    [Theory]
    [MemberData(nameof(AllActions))]
    public async Task EveryAction_ReturnsAResult_AndNeverThrows(string key)
    {
        var (controller, action) = Build(key);

        var result = action.Invoke(controller, Args.For(action));
        if (result is Task task)
        {
            await task;
            result = task.GetType().GetProperty("Result").GetValue(task);
        }

        Assert.IsAssignableFrom<IActionResult>(result);
    }

    // what the page would show for a missing record or an invalid form: never a crash
    [Theory]
    [InlineData("Kitchen")]
    [InlineData("Job")]
    [InlineData("Table")]
    [InlineData("Currency")]
    [InlineData("Store")]
    [InlineData("Customer")]
    [InlineData("Supplier")]
    [InlineData("Employee")]
    [InlineData("Product")]
    [InlineData("Menu")]
    public async Task Upsert_UnknownId_RedirectsWithAMessage_InvalidModel_ShowsTheFormAgain(string name)
    {
        var type = Controllers().Single(c => c.Name == name + "Controller");
        var controller = (Controller)_host.Get(type);
        var key = Actions(type).Where(m => m.Name == "Upsert").Select(m => type.Name + "|Upsert|" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name))).ToList();
        var (c1, get) = Build(key.First(k => k.EndsWith("|Nullable`1")));
        var unknown = await Await(get.Invoke(c1, new object[] { 99999 }));

        Assert.IsType<RedirectToActionResult>(unknown);

        var (c2, post) = Build(key.First(k => !k.EndsWith("|Nullable`1")));
        c2.ModelState.AddModelError("ArName", "required");
        var model = Args.Make(post.GetParameters()[0].ParameterType);
        var invalid = await Await(post.Invoke(c2, new[] { model }));

        var view = Assert.IsType<ViewResult>(invalid);
        Assert.Equal("Upsert", view.ViewName);
        Assert.Same(model, view.Model);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("!!!not-base64!!!")]
    [InlineData("")]
    public async Task ConfirmEmail_MangledLink_ShowsFailureNotAServerError(string token)
    {
        var (controller, action) = Build("AccountController|ConfirmEmail|String,String");

        var result = await Await(action.Invoke(controller, new object[] { "user-1", token }));

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(false, view.Model);
    }

    private static async Task<object> Await(object result)
    {
        if (result is Task task)
        {
            await task;
            return task.GetType().GetProperty("Result").GetValue(task);
        }

        return result;
    }
}

public class FakeSession : ISession
{
    private readonly Dictionary<string, byte[]> _data = new();

    public bool IsAvailable => true;
    public string Id => "test";
    public IEnumerable<string> Keys => _data.Keys;

    public void Clear() => _data.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _data.Remove(key);
    public void Set(string key, byte[] value) => _data[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _data.TryGetValue(key, out value);
}

public class SessionFeature : ISessionFeature
{
    public ISession Session { get; set; }
}
