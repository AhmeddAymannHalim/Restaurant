using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Helpers.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class EntryController : Controller
    {
        private readonly IEntryService _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public EntryController(IEntryService service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _toastr = toastr;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index([FromQuery] PageRequest request)
        {
            try
            {
                request ??= new PageRequest();
                var result = await _service.GetPagedEntriesAsync(request);

                return View(result);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _service.GetEntryByIdAsync(id);
                if (model == null)
                {
                    _toastr.Error(_localizer["RecordNotFound"].Value);
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> Upsert()
        {
            try
            {
                var model = new EntryViewModel
                {
                    EntryDate = DateTime.Now,
                    EntryType = EntryType.Journal,
                    Lines = new List<EntryLineViewModel> { new(), new() }
                };
                model.Accounts = await _service.GetPostingAccountOptionsAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(EntryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Accounts = await _service.GetPostingAccountOptionsAsync();
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.CreateManualEntryAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer["SavedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index));
                }

                model.Accounts = await _service.GetPostingAccountOptionsAsync();
                _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                return View("Upsert", model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteManualEntryAsync(id);
                if (result.Success)
                {
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
