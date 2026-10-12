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

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetDashboardAsync(UserId));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Board()
        {
            try
            {
                return PartialView("_Board", await _service.GetDashboardAsync(UserId));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
