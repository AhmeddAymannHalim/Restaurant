using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Hubs;
using Shared;
using Shared.ViewModels.Commons;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Staff")]
    public class KitchenOrderController : Controller
    {
        private readonly IOrderService _service;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public KitchenOrderController(
            IOrderService service,
            IHubContext<OrderHub> hub,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _hub = hub;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetKitchenOrdersAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var result = await _service.CompleteOrderAsync(id);
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOrders");
                    return Json(new { success = true });
                }

                return Json(new
                {
                    success = false,
                    errorMessage = _localizer[result.Message].Value
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = _localizer["SomethingWentError"].Value
                });
            }
        }
    }
}
