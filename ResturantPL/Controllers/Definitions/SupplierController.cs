using Restaurant.DAL.Models;
using Restaurant.BLL.Services.IServices.Commons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public SupplierController(ISupplierService service, ILookupService lookup, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
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
                var result = await _service.GetPagedSuppliersAsync(request);

                return View(result);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            try
            {
                var model = new SupplierViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetSupplierByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }

                await PopulateData(model);

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(SupplierViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;

                if (!ModelState.IsValid)
                {
                    await PopulateData(model);
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertSupplierAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = model.ArName });
                }

                await PopulateData(model);
                _toastr.Error(_localizer[result.Message ?? (isNew ? "ErrorWhileSaving" : "ErrorWhileUpdating")].Value);
                return View("Upsert", model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteSupplierAsync(id);
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

        private async Task PopulateData(SupplierViewModel model)
        {
            model.CostCenters = await _lookup.NamedLookupAsync<CostCenter>();
            model.Branches = await _lookup.BranchLookupAsync();
            model.Employees = await _lookup.EmployeeLookupAsync();
        }
    }
}
