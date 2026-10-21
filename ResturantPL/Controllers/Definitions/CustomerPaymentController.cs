using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using System.Security.Claims;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class CustomerPaymentController : Controller
    {
        private readonly ICustomerPaymentService _service;
        private readonly ILookupService _lookup;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CustomerPaymentController(
            ICustomerPaymentService service,
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
                var result = await _service.GetPagedPaymentsAsync(request);

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
                var model = new PaymentViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    model = await _service.GetPaymentByIdAsync(id.Value);
                    if (model == null)
                    {
                        _toastr.Error(_localizer["RecordNotFound"].Value);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    model.ProcessCode = await _service.GetNextCodeAsync();
                    model.Description = _localizer["DefaultPaymentDescription"].Value;
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
        public async Task<IActionResult> Upsert(PaymentViewModel model)
        {
            try
            {
                var isNew = model.Id == 0;

                if (!ModelState.IsValid)
                {
                    await PopulateData(model);                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.UpsertPaymentAsync(model);
                if (result.Success)
                {
                    _toastr.Success(_localizer[isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(Index), new { searchString = result.Data });
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
                var result = await _service.DeletePaymentAsync(id);
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
            var model = await _service.GetPaymentByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return new ViewAsPdf("Pdf", model) { FileName = Uri.EscapeDataString($"CustomerPayment-{model.ProcessCode}.pdf") };
        }

        [HttpGet]
        public async Task<IActionResult> Balance(int partnerId)
        {
            return Json(await _service.GetBalanceAsync(partnerId));
        }

        [HttpGet]
        public async Task<IActionResult> OpenInvoices(int partnerId)
        {
            return Json(await _service.GetOpenInvoicesForPartnerAsync(partnerId));
        }

        public async Task<IActionResult> Refunds(int paymentId)
        {
            try
            {
                var model = await _service.GetRefundRequestAsync(paymentId);
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(PaymentRefundRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return RedirectToAction(nameof(Refunds), new { paymentId = model.OriginalPaymentId });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _service.RequestRefundAsync(model, userId);
                if (result.Success)
                {
                    _toastr.Success(_localizer["SavedSuccessfully"].Value);
                }
                else
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                }

                return RedirectToAction(nameof(Refunds), new { paymentId = model.OriginalPaymentId });
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> ApproveRefund(int id, int paymentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _service.ApproveRefundAsync(id, userId);
                if (result.Success)
                {
                    _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                }
                else
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                }

                return RedirectToAction(nameof(Refunds), new { paymentId });
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> RejectRefund(int id, int paymentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _service.RejectRefundAsync(id, userId);
                if (result.Success)
                {
                    _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                }
                else
                {
                    _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                }

                return RedirectToAction(nameof(Refunds), new { paymentId });
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }

        private async Task PopulateData(PaymentViewModel model)
        {
            model.Partners = await _lookup.CustomerLookupAsync();
            model.OpenInvoices = model.PartnerId > 0
                ? await _service.GetOpenInvoicesForPartnerAsync(model.PartnerId.GetValueOrDefault())
                : Enumerable.Empty<LookupItem>();
        }
    }
}
