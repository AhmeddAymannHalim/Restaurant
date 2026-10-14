using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Commons;
using Shared.ViewModels.Commons;
using System.Diagnostics;

namespace ResturantPL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _service;

        public HomeController(IHomeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetHomeAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Pricing()
        {
            return Json(await _service.GetPricingAsync());
        }

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
