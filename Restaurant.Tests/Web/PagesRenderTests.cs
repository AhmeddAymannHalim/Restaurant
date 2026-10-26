using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Tests;

public sealed class PagesRenderTests(RestaurantWebApplicationFactory factory) : IClassFixture<RestaurantWebApplicationFactory>
{
    public static IEnumerable<object[]> Pages()
    {
        var controllers = typeof(Program).Assembly.GetTypes()
            .Where(a => typeof(Controller).IsAssignableFrom(a) && a.Namespace == "ResturantPL.Controllers.Definitions");
        foreach (var controller in controllers)
        {
            var name = controller.Name.Replace("Controller", "");
            var gets = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(a => a.GetCustomAttribute<HttpPostAttribute>() is null)
                .Where(a => a.GetParameters().All(p => p.IsOptional || Nullable.GetUnderlyingType(p.ParameterType) != null || p.ParameterType.IsClass && p.ParameterType != typeof(string)))
                .Select(a => a.Name)
                .Distinct();
            foreach (var action in gets.Where(a => a is "Index" or "Upsert" or "List"))
            {
                foreach (var culture in new[] { "ar", "en" })
                {
                    yield return new object[] { $"/{name}/{action}", culture };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task Page_Opens_WithoutAnError(string path, string culture)
    {
        var client = factory.CreateClient(culture);

        var response = await client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("حدث خطأ غير متوقع", body);
        Assert.DoesNotContain("An unexpected error occurred", body);
        Assert.Contains(culture == "ar" ? "dir=\"rtl\"" : "dir=\"ltr\"", body);

        var page = WebUtility.HtmlDecode(body);
        Assert.DoesNotContain("field is required.", page);
        Assert.DoesNotContain("must be a number.", page);
        if (page.Contains("data-val-required="))
        {
            Assert.Contains(culture == "ar" ? "data-val-required=\"هذا الحقل مطلوب\"" : "data-val-required=\"This field is required\"", page);
        }
    }
}
