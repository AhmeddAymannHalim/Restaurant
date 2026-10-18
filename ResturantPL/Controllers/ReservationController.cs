using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ITableReservationService _service;
        private readonly IAccountService _account;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ReservationController(
            ITableReservationService service,
            IAccountService account,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _account = account;
            _toastr = toastr;
            _localizer = localizer;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            try
            {
                var model = new ReservationCreateViewModel();

                if (User.Identity.IsAuthenticated)
                {
                    var profile = await _account.GetProfileAsync(UserId);
                    if (profile != null)
                    {
                        model.CustomerName = profile.FirstName + " " + profile.LastName;
                        model.PhoneNumber = profile.PhoneNumber;
                    }

                    model.Email = User.FindFirstValue(ClaimTypes.Email);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(ReservationCreateViewModel model)
        {
            try
            {
                var result = await _service.BookAsync(model, UserId);
                if (!result.Success)
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                    return View(model);
                }

                TempData["ReservationId"] = int.Parse(result.Data);

                return RedirectToAction(nameof(Done));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Done()
        {
            try
            {
                var reservationId = TempData["ReservationId"];
                if (reservationId == null)
                {
                    return RedirectToAction(nameof(Book));
                }

                return View(await _service.GetReservationByIdAsync((int)reservationId));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Slots(DateTime when, int guests)
        {
            try
            {
                return Json(await _service.GetTableSlotsAsync(when, guests));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
