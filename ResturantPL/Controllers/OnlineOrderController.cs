using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using ResturantPL.Hubs;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class OnlineOrderController : Controller
    {
        private readonly IOnlineOrderService _service;
        private readonly ILookupService _lookup;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public OnlineOrderController(
            IOnlineOrderService service,
            ILookupService lookup,
            IHubContext<OrderHub> hub,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _lookup = lookup;
            _hub = hub;
            _toastr = toastr;
            _localizer = localizer;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Menu()
        {
            try
            {
                return View(await _service.GetMenuAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public IActionResult Basket()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var model = new OnlineOrderCreateViewModel
                {
                    Tables = await _lookup.TableLookupAsync(),
                    Zones = await _lookup.DeliveryZoneLookupAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [Authorize]
        public IActionResult MyOrders()
        {
            return RedirectToAction(nameof(PendingOrders));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(OnlineOrderCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return RedirectToAction(nameof(Checkout));
                }

                var result = await _service.CreateOrderAsync(model, UserId);
                if (!result.Success)
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                    return RedirectToAction(nameof(Checkout));
                }

                await _hub.Clients.All.SendAsync("RefreshOnlineOrders");
                _toastr.Success(_localizer["SavedSuccessfully"].Value);
                TempData["OrderPlaced"] = true;

                return RedirectToAction(nameof(PendingOrders));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [Authorize]
        public async Task<IActionResult> PendingOrders()
        {
            try
            {
                return View(await _service.GetCustomerOrdersAsync(UserId));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetOrderAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            if (model.CustomerId != UserId && User.IsInRole("User"))
            {
                return Forbid();
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isStaff = !User.IsInRole("User");
                var result = await _service.CancelOrderAsync(id, UserId, isStaff);
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOnlineOrders");
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
