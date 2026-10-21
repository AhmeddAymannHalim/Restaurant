using Restaurant.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public EmployeeController(
            IEmployeeService service,
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
                var result = await _service.GetPagedEmployeesAsync(request);

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
                var model = new EmployeeViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetEmployeeByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.Code = await _service.NewEmployeeCodeAsync();
                    model.IsActive = true;
                }

                await PopulateData(model);
                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(EmployeeViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;

                if (!ModelState.IsValid)
                {
                    await PopulateData(model);                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertEmployeeAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = model.ArName });
                }

                await PopulateData(model);                _toastr.Error(_localizer[result.Message ?? (isNew ? "ErrorWhileSaving" : "ErrorWhileUpdating")].Value);
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
                var result = await _service.DeleteEmployeeAsync(id);
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

        private async Task PopulateData(EmployeeViewModel model)
        {
            model.Departments = await _lookup.NamedLookupAsync<Department>();
            model.ContractTypes = await _lookup.NamedLookupAsync<ContractType>();
            model.Specializations = await _lookup.NamedLookupAsync<Specialization>();
            model.JobTypes = await _lookup.NamedLookupAsync<JobType>();
            model.InsuranceCategories = await _lookup.InsuranceCategoryLookupAsync();
            model.InsuranceCompanies = await _lookup.NamedLookupAsync<InsuranceCompany>();
            model.Branches = await _lookup.NamedLookupAsync<CompanyBranch>();
            model.Jobs = await _lookup.NamedLookupAsync<Job>();
            model.Nationalities = await _lookup.NamedLookupAsync<Country>();
            model.LeaveTypes = await _lookup.LeaveTypeLookupAsync();
            model.AllowanceTypes = await _lookup.AllowanceTypeLookupAsync();
            model.DeductionTypes = await _lookup.DeductionTypeLookupAsync();
        }
    }
}
