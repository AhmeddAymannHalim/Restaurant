using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class ScroungeController : Controller
    {
        private readonly IScroungeService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ScroungeController(
            IScroungeService service,
            ILookupService lookup,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _lookup = lookup;
            _toastr = toastr;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index([FromQuery] PageRequest request)
        {
            try
            {
                request ??= new PageRequest();
                var result = await _service.GetPagedScroungesAsync(request);

                return View(result);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            try
            {
                var model = new ScroungeViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetScroungeByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }

                model.Employees = await _lookup.NamedLookupAsync<Employee>();

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ScroungeViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!ModelState.IsValid)
                {
                    model.Employees = await _lookup.NamedLookupAsync<Employee>();
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertScroungeAsync(model, userId);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = (string)null });
                }

                model.Employees = await _lookup.NamedLookupAsync<Employee>();
                _toastr.Error(_localizer[result.Message ?? (isNew ? "ErrorWhileSaving" : "ErrorWhileUpdating")].Value);
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
                var result = await _service.DeleteScroungeAsync(id);
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
