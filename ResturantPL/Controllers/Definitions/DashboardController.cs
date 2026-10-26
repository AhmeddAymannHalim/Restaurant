using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared.ViewModels.Commons;
using System.Security.Claims;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetDashboardAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Board()
        {
            try
            {
                return PartialView("_Board", await _service.GetDashboardAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
