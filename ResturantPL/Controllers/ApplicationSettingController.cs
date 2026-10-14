using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using System.Drawing.Printing;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Management")]
    public class ApplicationSettingController : Controller
    {
        private readonly ISettingService _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ApplicationSettingController(
            ISettingService service,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _toastr = toastr;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ApplicationSetting()
        {
            try
            {
                return View(await _service.GetApplicationSettingAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationSetting(ApplicationSettingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View(model);
                }

                var result = await _service.UpdateApplicationSettingAsync(model);

                return Saved(result, nameof(ApplicationSetting), model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> RestaurantSetting()
        {
            try
            {
                return View(await _service.GetRestaurantSettingAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RestaurantSetting(RestaurantSettingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View(model);
                }

                var result = await _service.UpdateRestaurantSettingAsync(model);

                return Saved(result, nameof(RestaurantSetting), model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> PrinterSetting()
        {
            try
            {
                var model = await _service.GetPrinterSettingAsync();
                PopulatePrinters(model);

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PrinterSetting(PrinterSettingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PopulatePrinters(model);
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View(model);
                }

                var result = await _service.UpdatePrinterSettingAsync(model);
                if (!result.Success)
                {
                    PopulatePrinters(model);
                }

                return Saved(result, nameof(PrinterSetting), model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        private IActionResult Saved(ResultMessage result, string action, object model)
        {
            if (result.Success)
            {
                _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                return RedirectToAction(action);
            }

            _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
            return View(action, model);
        }

        private static void PopulatePrinters(PrinterSettingViewModel model)
        {
            var printers = new List<LookupItem_String>();
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                printers.AddRange(PrinterSettings.InstalledPrinters.Cast<string>()
                    .Select(a => new LookupItem_String
                        {
                            Id = a,
                            ArName = a,
                            EnName = a
                        }));
            }

            model.Printers = printers;
        }
    }
}
