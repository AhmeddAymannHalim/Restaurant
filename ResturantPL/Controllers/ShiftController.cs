using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Staff")]
    public class ShiftController : Controller
    {
        private readonly IOrderService _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ShiftController(IOrderService service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _toastr = toastr;
            _localizer = localizer;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string password)
        {
            try
            {
                if (!await _service.VerifyShiftPasswordAsync(password))
                {
                    _toastr.Error(_localizer["IncorrectShiftPassword"].Value);
                    return View();
                }

                HttpContext.Session.SetString("IsAuthenticated", "true");

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> End(string password, decimal countedCash, bool confirmOverride = false)
        {
            try
            {
                if (!await _service.VerifyShiftPasswordAsync(password))
                {
                    _toastr.Error(_localizer["IncorrectShiftPassword"].Value);
                    return RedirectToAction("Index", "Order");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!confirmOverride)
                {
                    var incompleteOrders = (await _service.GetIncompleteCashOrdersAsync(userId)).ToList();
                    if (incompleteOrders.Count > 0)
                    {
                        return View("ConfirmEnd", new ShiftCloseConfirmViewModel
                        {
                            Password = password,
                            CountedCash = countedCash,
                            IncompleteOrders = incompleteOrders
                        });
                    }
                }

                var result = await _service.FinishShiftAsync(userId, countedCash);
                if (!result.Success)
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                    return RedirectToAction("Index", "Order");
                }

                HttpContext.Session.SetString("IsAuthenticated", "false");
                _toastr.Success(_localizer["ShiftEnded"].Value);

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }
    }
}
