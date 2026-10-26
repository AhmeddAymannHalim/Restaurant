using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Restaurant.Tests;

public sealed class AuthorizationMatrixTests(RestaurantWebApplicationFactory factory) : IClassFixture<RestaurantWebApplicationFactory>
{
    // sign in, language and the error page must open without a login
    private static readonly string[] OpenControllers = { "AccountController", "HomeController", "SetLanguageController" };

    public static IEnumerable<Type> Controllers() => typeof(Program).Assembly.GetTypes()
        .Where(a => typeof(Controller).IsAssignableFrom(a) && !a.IsAbstract);

    [Fact]
    public void EveryAction_NeedsALogin_ExceptTheOpenScreens()
    {
        foreach (var controller in Controllers().Where(a => !OpenControllers.Contains(a.Name)))
        {
            var onController = controller.GetCustomAttributes<AuthorizeAttribute>(true).Any();
            var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(a => typeof(IActionResult).IsAssignableFrom(a.ReturnType) || a.ReturnType == typeof(Task<IActionResult>));
            foreach (var action in actions)
            {
                var allowed = onController || action.GetCustomAttributes<AuthorizeAttribute>(true).Any();
                Assert.True(allowed, $"{controller.Name}.{action.Name} opens without a login");
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
            }
        }
    }

    [Fact]
    public void EveryPost_IsCheckedForTheAntiForgeryToken()
    {
        var options = factory.Services.GetRequiredService<IOptions<MvcOptions>>().Value;

        Assert.Contains(options.Filters, a => a is AutoValidateAntiforgeryTokenAttribute);
        Assert.DoesNotContain(options.Filters, a => a is IgnoreAntiforgeryTokenAttribute);
    }

    [Fact]
    public async Task APostWithoutTheToken_IsRefused()
    {
        var client = factory.CreateClient("ar");

        var response = await client.PostAsync("/ContractType/Upsert", new FormUrlEncodedContent(new Dictionary<string, string> { ["ArName"] = "x", ["EnName"] = "x" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
