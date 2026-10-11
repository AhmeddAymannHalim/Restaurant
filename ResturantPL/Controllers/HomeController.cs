using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Commons;
using ResturantPL.ViewModels;
using Shared.ViewModels.Commons;
using System.Diagnostics;

namespace ResturantPL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebsiteService _service;

        public HomeController(IWebsiteService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetWebsiteAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        // AJAX: the menu items of one category (no id = every category)
        [HttpGet]
        public async Task<IActionResult> MenuItems(int? sectionId)
        {
            return Json(await _service.GetMenuItemsAsync(sectionId));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { ErrorMessage = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
