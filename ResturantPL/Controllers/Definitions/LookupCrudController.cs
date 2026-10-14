using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    /// <summary>
    /// One list + one form for every simple lookup table. A concrete controller only names the table, its icon and
    /// its extra columns; the views are shared (Views/Definitions/LookupCrud).
    /// </summary>
    [Authorize(Policy = "Staff")]
    public abstract class LookupCrudController<T> : Controller where T : BaseEntity, new()
    {
        private const string IndexView = "~/Views/Definitions/LookupCrud/Index.cshtml";
        private const string UpsertView = "~/Views/Definitions/LookupCrud/Upsert.cshtml";

        private readonly ILookupCrudService<T> _service;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        protected LookupCrudController(ILookupCrudService<T> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _toastr = toastr;
            _localizer = localizer;
        }

        /// <summary>Resource key of the table's plural title (for example Lookup.SellType).</summary>
        protected abstract string TitleKey { get; }

        protected abstract string Icon { get; }

        protected virtual IReadOnlyList<LookupField> Fields => Array.Empty<LookupField>();

        /// <summary>Loads the drop-down options of Reference fields (Parent options are loaded here already).</summary>
        protected virtual Task LoadReferenceOptionsAsync(LookupEditViewModel model)
        {
            return Task.CompletedTask;
        }

        public async Task<IActionResult> Index([FromQuery] PageRequest request)
        {
            try
            {
                request ??= new PageRequest();
                var result = await _service.GetPagedAsync(request);
                Describe();

                return View(IndexView, result);
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
                var model = new LookupEditViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetByIdAsync(id.Value, Fields);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.Code = await _service.NewCodeAsync();
                }

                await PopulateData(model);

                return View(UpsertView, model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(LookupEditViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;

                foreach (var error in _service.Validate(model, Fields))
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                if (!ModelState.IsValid)
                {
                    await PopulateData(model);
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View(UpsertView, model);
                }

                var result = await _service.UpsertAsync(model, Fields);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = model.ArName });
                }

                await PopulateData(model);
                _toastr.Error(_localizer[result.Message ?? (isNew ? "ErrorWhileSaving" : "ErrorWhileUpdating")].Value);
                return View(UpsertView, model);
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
                var result = await _service.DeleteAsync(id);
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

        private async Task PopulateData(LookupEditViewModel model)
        {
            Describe();

            foreach (var field in Fields.Where(a => a.Kind == LookupFieldKind.Parent))
            {
                model.Options[field.Name] = await _service.GetParentOptionsAsync(model.Id > 0 ? model.Id : null);
            }

            await LoadReferenceOptionsAsync(model);
        }

        // What the shared views need to draw this table.
        private void Describe()
        {
            ViewBag.TitleKey = TitleKey;
            ViewBag.Icon = Icon;
            ViewBag.Fields = Fields;
            ViewBag.HasCode = typeof(T).GetProperty("Code") is not null;
        }
    }
}
