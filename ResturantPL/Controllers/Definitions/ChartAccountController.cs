using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using Restaurant.DAL.Models;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Management")]
    public class ChartAccountController : Controller
    {
        private readonly IAccountDefinitionService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ChartAccountController(IAccountDefinitionService service, ILookupService lookup, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
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
                var result = await _service.GetPagedAccountsAsync(request, assetsOnly: false);

                return View(result);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        public async Task<IActionResult> Upsert(int? id, AccountKind? kind)
        {
            try
            {
                var model = new AccountDefinitionViewModel { Kind = kind is null or AccountKind.Asset or AccountKind.Owned ? AccountKind.GeneralLedger : kind.Value };
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetAccountByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }

                    if (model.IsOwned || model.Kind == AccountKind.Asset)
                    {
                        _toastr.Error(_localizer["AccountOwnedByAnotherScreen"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.Code = await _service.NewAccountCodeAsync(null, model.Kind);
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
        public async Task<IActionResult> Upsert(AccountDefinitionViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;
                if (model.Kind == AccountKind.Asset || model.Kind == AccountKind.Owned)
                {
                    ModelState.AddModelError(nameof(model.Kind), _localizer["AccountOwnedByAnotherScreen"].Value);
                }

                if (!ModelState.IsValid)
                {
                    await PopulateData(model);                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertAccountAsync(model);
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
                var result = await _service.DeleteAccountAsync(id);
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

        private async Task PopulateData(AccountDefinitionViewModel model)
        {
            model.Parents = await _service.GetParentOptionsAsync(model.Id == 0 ? null : model.Id);
            model.Currencies = await _lookup.NamedLookupAsync<Currency>();
        }
    }
}
