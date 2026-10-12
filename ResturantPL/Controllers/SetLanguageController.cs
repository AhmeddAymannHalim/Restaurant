using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ResturantPL.Controllers
{
    public class SetLanguageController : Controller
    {
        private static readonly string[] SupportedCultures = { "ar", "en" };

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (SupportedCultures.Contains(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
        }
    }
}
