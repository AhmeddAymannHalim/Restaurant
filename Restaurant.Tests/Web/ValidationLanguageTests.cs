using System.Net;
using System.Text.RegularExpressions;

namespace Restaurant.Tests;

public sealed class ValidationLanguageTests(RestaurantWebApplicationFactory factory) : IClassFixture<RestaurantWebApplicationFactory>
{
    [Theory]
    [InlineData("Salary", "ar", "هذا الحقل مطلوب")]
    [InlineData("Salary", "en", "This field is required")]
    [InlineData("Scrounge", "ar", "هذا الحقل مطلوب")]
    [InlineData("Product", "ar", "هذا الحقل مطلوب")]
    [InlineData("Customer", "ar", "هذا الحقل مطلوب")]
    [InlineData("Customer", "en", "This field is required")]
    public async Task EmptyForm_ShowsItsMessages_InThePageLanguage(string screen, string culture, string required)
    {
        var client = factory.CreateClient(culture);
        var page = await (await client.GetAsync($"/{screen}/Upsert")).Content.ReadAsStringAsync();
        var form = Regex.Match(page, "<form[^>]*method=\"post\"[^>]*>(.*?)</form>", RegexOptions.Singleline).Groups[1].Value;
        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value
        };
        foreach (Match field in Regex.Matches(form, "<(?:select|input)[^>]*name=\"([^\"]+)\""))
        {
            fields.TryAdd(field.Groups[1].Value, "");
        }

        var response = await client.PostAsync($"/{screen}/Upsert", new FormUrlEncodedContent(fields));
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        var shown = Regex.Matches(body, "field-validation-error\"[^>]*>([^<]+)<").Select(a => a.Groups[1].Value).ToList();

        Assert.NotEmpty(shown);
        Assert.Contains(required, shown);
        Assert.All(shown, message => Assert.DoesNotContain("is invalid", message));
        Assert.All(shown, message => Assert.DoesNotContain("field is required.", message));
    }
}
