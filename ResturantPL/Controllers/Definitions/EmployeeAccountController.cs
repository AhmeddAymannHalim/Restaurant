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

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Management")]
    public class EmployeeAccountController : Controller
    {
        private readonly IEmployeeAccountService _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public EmployeeAccountController(
            IEmployeeAccountService service,
            IToastrService toastr,
            IStringLocalizer<SharedResource> localizer)
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
                var result = await _service.GetPagedAccountsAsync(request);

                return View(result);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> Upsert(string id, int? employeeId)
        {
            try
            {
                var model = new EmployeeAccountViewModel();
                if (!string.IsNullOrEmpty(id))
                {
                    model = await _service.GetAccountByIdAsync(id);
                }
                else if (employeeId.HasValue)
                {
                    model = await _service.GetAccountByEmployeeIdAsync(employeeId.Value);
                }

                if (model == null)
                {
                    _toastr.Error(_localizer["RecordNotFound"].Value);
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(model.Id) && model.EmployeeId == 0)
                {
                    model.Employees = await _service.GetEmployeesWithoutAccountAsync();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(EmployeeAccountViewModel model)
        {
            try
            {
                var isNew = string.IsNullOrEmpty(model.Id);

                if (!ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.Id) && model.EmployeeId == 0)
                    {
                        model.Employees = await _service.GetEmployeesWithoutAccountAsync();
                    }
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertAccountAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = model.UserName });
                }

                if (string.IsNullOrEmpty(model.Id) && model.EmployeeId == 0)
                {
                    model.Employees = await _service.GetEmployeesWithoutAccountAsync();
                }
                _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                return View("Upsert", model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }
    }
}
