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
    public class MenuController : Controller
    {
        private readonly IMenuItemService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public MenuController(
            IMenuItemService service,
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
                var result = await _service.GetPagedMenuItemsAsync(request);

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
                var model = new MenuItemViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetMenuItemByIdAsync(id.Value);
                }

                if (model == null)
                {
                    _toastr.Error(_localizer["RecordNotFound"].Value);
                    return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Upsert(MenuItemViewModel model)
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

                var result = await _service.UpsertMenuItemAsync(model);
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
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteMenuItemAsync(id);
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

        [HttpGet]
        public async Task<IActionResult> GetProductsByStore(int storeId)
        {
            return Json(await _lookup.StoreProductLookupByStoreIdAsync(storeId));
        }

        private async Task PopulateData(MenuItemViewModel model)
        {
            model.MenuSections = await _lookup.MenuSectionLookupAsync();
            model.Stores = await _lookup.StoreLookupAsync();
            model.Products = await _lookup.ProductLookupAsync();
            model.Categories = await _lookup.ProductCategoryLookupAsync();
        }
    }
}
