using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Definitions;

namespace ResturantPL.ViewComponents
{
    public class SiteBrandViewComponent : ViewComponent
    {
        private readonly ISettingService _service;

        public SiteBrandViewComponent(ISettingService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var setting = await _service.GetRestaurantSettingAsync();
            var name = LanguageHelper.IsArabic() ? setting?.ArName : setting?.EnName;

            return View((object)(name ?? "Restaurant"));
        }
    }
}
