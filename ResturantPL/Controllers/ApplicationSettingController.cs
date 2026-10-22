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
    [Authorize(Policy = "Admin")]
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

                if (!result.Success)
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                    return View(nameof(ApplicationSetting), model);
                }

                _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                return RedirectToAction(nameof(ApplicationSetting));
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
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View(model);
                }

                var result = await _service.UpdatePrinterSettingAsync(model);
                if (!result.Success)
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
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                    return View(nameof(PrinterSetting), model);
                }

                _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                return RedirectToAction(nameof(PrinterSetting));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }
    }
}
