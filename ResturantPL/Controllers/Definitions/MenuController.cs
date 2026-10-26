using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.Models;
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
                else
                {
                    model.Code = await _service.NewMenuItemCodeAsync();
                }

                model.MenuSections = await _lookup.NamedLookupAsync<MenuSection>();
                model.Stores = await _lookup.NamedLookupAsync<Store>();
                model.Products = await _lookup.NamedLookupAsync<Product>();
                model.Categories = await _lookup.NamedLookupAsync<ProductCategory>();
                model.SellTypes = (await _lookup.SellTypeLookupAsync()).ToList();
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

                if (ModelState.IsValid)
                {
                    var result = await _service.UpsertMenuItemAsync(model);
                    if (result.Success)
                    {
                        _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                        return RedirectToAction(nameof(Index), new { searchString = model.ArName });
                    }

                    _toastr.Error(_localizer[result.Message ?? (isNew ? "ErrorWhileSaving" : "ErrorWhileUpdating")].Value);
                }
                else
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                }

                model.MenuSections = await _lookup.NamedLookupAsync<MenuSection>();
                model.Stores = await _lookup.NamedLookupAsync<Store>();
                model.Products = await _lookup.NamedLookupAsync<Product>();
                model.Categories = await _lookup.NamedLookupAsync<ProductCategory>();
                model.SellTypes = (await _lookup.SellTypeLookupAsync()).ToList();
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
            try
            {
                return Json(await _lookup.StoreProductLookupByStoreIdAsync(storeId));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
