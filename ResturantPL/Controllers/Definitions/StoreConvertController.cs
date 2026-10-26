using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.Models;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using Rotativa.AspNetCore;
using Shared;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class StoreConvertController : Controller
    {
        private readonly IStoreConvertService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public StoreConvertController(
            IStoreConvertService service,
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

                return View(await _service.GetPagedConvertsAsync(request));
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
                var model = new StoreConvertViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetConvertByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.ConvertCode = await _service.GetNextCodeAsync();
                }

                model.Stores = await _lookup.NamedLookupAsync<Store>();
                model.Categories = await _lookup.NamedLookupAsync<ProductCategory>();

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(StoreConvertViewModel model)
        {
            try
            {
                if (model.Id > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    var result = await _service.CreateConvertAsync(model);
                    if (result.Success)
                    {
                        _toastr.Success(_localizer["SavedSuccessfully"].Value);
                        return RedirectToAction(nameof(Index), new { searchString = result.Data });
                    }

                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                }
                else
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                }

                model.ConvertCode = await _service.GetNextCodeAsync();
                model.Stores = await _lookup.NamedLookupAsync<Store>();
                model.Categories = await _lookup.NamedLookupAsync<ProductCategory>();

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
                var result = await _service.DeleteConvertAsync(id);
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

        public async Task<IActionResult> Pdf(int id)
        {
            try
            {
                var model = await _service.GetConvertByIdAsync(id);
                if (model == null)
                {
                    return NotFound();
                }

                return new ViewAsPdf("Pdf", model) { FileName = Uri.EscapeDataString($"StoreConvert-{model.ConvertCode}.pdf") };
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }
    }
}
