using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Management")]
    public class AccountSettingController : Controller
    {
        private readonly IAccountSettingService _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountSettingController(
            IAccountSettingService service,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _toastr = toastr;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetAccountSettingAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AccountSettingViewModel model)
        {
            try
            {
                var result = await _service.UpdateAccountSettingAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index));
                }

                _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                return View(await _service.GetAccountSettingAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }
    }
}
