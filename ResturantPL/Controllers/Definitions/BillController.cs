using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Helpers.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using Rotativa.AspNetCore;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class BillController : Controller
    {
        private readonly IErpBillService _service;
        private readonly IInvoiceService _invoices;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public BillController(IErpBillService service, IInvoiceService invoices, ILookupService lookup, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _invoices = invoices;
            _lookup = lookup;
            _toastr = toastr;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index([FromQuery] PageRequest request)
        {
            try
            {
                request ??= new PageRequest();
                var result = await _service.GetPagedBillsAsync(request);

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
                var model = await _service.GetBillByIdAsync(id);
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

        public async Task<IActionResult> Upsert(int? id, InvoiceType? type)
        {
            try
            {
                var model = new InvoiceViewModel { InvoiceType = type ?? InvoiceType.Purchases };
                if (id.HasValue && id.Value > 0)
                {
                    model = await _invoices.GetInvoiceByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.InvoiceCode = await _invoices.GetNextCodeAsync(model.InvoiceType);
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
        public async Task<IActionResult> Upsert(InvoiceViewModel model)
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

                var result = await _invoices.UpsertInvoiceAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = result.Data });
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
                var result = await _invoices.DeleteInvoiceAsync(id);
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
            var model = await _invoices.GetInvoiceByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return new ViewAsPdf("Pdf", model) { FileName = Uri.EscapeDataString($"Invoice-{model.InvoiceCode}.pdf") };
        }

        [HttpGet]
        public async Task<IActionResult> SupplierBalance(int supplierId)
        {
            return Json(await _invoices.GetSupplierBalanceAsync(supplierId));
        }

        [HttpGet]
        public async Task<IActionResult> CustomerBalance(int customerId)
        {
            return Json(await _invoices.GetCustomerBalanceAsync(customerId));
        }

        [HttpPost]
        public async Task<IActionResult> Post(int id)
        {
            try
            {
                var result = await _service.PostBillAsync(id);
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

        [HttpPost]
        public async Task<IActionResult> PostPending()
        {
            try
            {
                var result = await _service.PostPendingAsync();
                if (result.Success)
                {
                    return Json(new { success = true, count = result.Data });
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

        private async Task PopulateData(InvoiceViewModel model)
        {
            model.Suppliers = await _lookup.SupplierLookupAsync();
            model.Customers = await _lookup.CustomerLookupAsync();
            model.Stores = await _lookup.StoreLookupAsync();
            model.Units = await _lookup.UnitPriceLookupAsync();
            model.Categories = await _lookup.ProductCategoryLookupAsync();

            if (model.Items.Count > 0)
            {
                var products = (await _lookup.StoreProductLookupByStoreIdAsync(model.StoreId)).ToDictionary(p => p.ProductId);
                var isArabic = LanguageHelper.IsArabic();
                foreach (var item in model.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        item.ProductName = isArabic ? product.ProductArName : product.ProductEnName;
                    }

                    var unit = model.Units.FirstOrDefault(u => u.Id == item.UnitPriceId);
                    if (unit is not null)
                    {
                        item.UnitName = isArabic ? unit.ArName : unit.EnName;
                    }
                }
            }
        }
    }
}
