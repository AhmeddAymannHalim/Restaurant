using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using ResturantPL.Hubs;
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
    public class OrderController : Controller
    {
        private readonly IOrderService _service;
        private readonly ILookupService _lookup;
        private readonly ICustomerService _customers;
        private readonly ISettingService _settings;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IToastrService _toastr;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public OrderController(
            IOrderService service, ILookupService lookup, ICustomerService customers, ISettingService settings,
            IHubContext<OrderHub> hub, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _lookup = lookup;
            _customers = customers;
            _settings = settings;
            _hub = hub;
            _toastr = toastr;
            _localizer = localizer;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _service.GetShiftDashboardAsync(UserId));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> List([FromQuery] PageRequest request)
        {
            try
            {
                request ??= new PageRequest();
                if (request.Status is not ("pending" or "history"))
                {
                    request.Status = "shift";
                }

                return View(await _service.GetPagedOrdersAsync(request, UserId));
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> Upsert(int? onlineOrderId)
        {
            try
            {
                if (HttpContext.Session.GetString("IsAuthenticated") != "true")
                {
                    return RedirectToAction("Login", "Shift");
                }

                var model = onlineOrderId.HasValue
                    ? await _service.GetNewOrderFromOnlineAsync(onlineOrderId.Value)
                    : await _service.GetNewOrderAsync();
                if (model == null)
                {
                    _toastr.Error(_localizer["RecordNotFound"].Value);
                    return RedirectToAction(nameof(Online));
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
        public async Task<IActionResult> Upsert(OrderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await Reload(model);
                    _toastr.Error(_localizer["ValidationError"].Value);
                    return View("Upsert", model);
                }

                var result = await _service.CreateOrderAsync(model, UserId);
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOrders");
                    return RedirectToAction(nameof(Details), new { id = int.Parse(result.Data) });
                }

                await Reload(model);
                _toastr.Error(_localizer[result.Message ?? "ErrorWhileSaving"].Value);
                return View("Upsert", model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _service.GetOrderByIdAsync(id);
                if (model == null)
                {
                    return NotFound();
                }

                ViewBag.Print = await _settings.GetApplicationSettingAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> Pdf(int id)
        {
            var model = await _service.GetOrderByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            ViewBag.IsPdf = true;

            return new ViewAsPdf("Details", model) { FileName = PdfFile.Name($"Order-{model.OrderCode}.pdf") };
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                var result = await _service.PayOrderAsync(id, UserId);
                if (!result.Success)
                {
                    _toastr.Error(_localizer[result.Message].Value);
                    return RedirectToAction(nameof(List), new { status = "pending" });
                }

                await _hub.Clients.All.SendAsync("RefreshOrders");
                _toastr.Success(_localizer["OrderPaid"].Value);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        public async Task<IActionResult> Move(int id)
        {
            return await ShowMove(id);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(OrderMoveViewModel model)
        {
            return await RunMove(model, await _service.MoveItemAsync(model));
        }

        public async Task<IActionResult> Merge(int id)
        {
            return await ShowMove(id);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Merge(OrderMoveViewModel model)
        {
            return await RunMove(model, await _service.MergeOrderAsync(model));
        }

        [HttpPost]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteOrderAsync(id);
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOrders");
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
        public async Task<IActionResult> CustomerInfo(int customerId)
        {
            var customer = await _customers.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return Json(new
            {
                customer.ArName,
                customer.EnName,
                customer.Address,
                customer.PhoneNumber,
                customer.MobileNumber
            });
        }

        public async Task<IActionResult> Online()
        {
            try
            {
                return View(await _service.GetPendingOnlineOrdersAsync());
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOnline(int id)
        {
            try
            {
                var result = await _service.CancelOnlineOrderAsync(id);
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOnlineOrders");
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

        private async Task<IActionResult> ShowMove(int id)
        {
            try
            {
                var model = await _service.GetOrderByIdAsync(id);
                if (model == null || model.Cash || model.TableId == null)
                {
                    _toastr.Error(_localizer["RecordNotFound"].Value);
                    return RedirectToAction(nameof(List), new { status = "pending" });
                }

                ViewBag.Tables = await _service.GetPendingTablesAsync(model.TableId.Value);

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        private async Task<IActionResult> RunMove(OrderMoveViewModel model, ResultMessage result)
        {
            try
            {
                if (result.Success)
                {
                    await _hub.Clients.All.SendAsync("RefreshOrders");
                    _toastr.Success(_localizer["UpdatedSuccessfully"].Value);
                    return RedirectToAction(nameof(List), new { status = "pending" });
                }

                _toastr.Error(_localizer[result.Message ?? "ErrorWhileUpdating"].Value);
                return RedirectToAction(nameof(List), new { status = "pending" });
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }

        private async Task PopulateData(OrderViewModel model)
        {
            model.Customers = await _lookup.CustomerLookupAsync();
            model.Tables = model.OnlineOrderId.HasValue ? await _lookup.TableLookupAsync() : await _lookup.FreeTableLookupAsync();
            model.DeliveryDrivers = await _lookup.DeliveryDriverLookupAsync();
            model.DeliveryZones = await _lookup.DeliveryZoneLookupAsync();
        }

        private async Task Reload(OrderViewModel model)
        {
            var fresh = model.OnlineOrderId.HasValue
                ? await _service.GetNewOrderFromOnlineAsync(model.OnlineOrderId.Value)
                : await _service.GetNewOrderAsync();
            model.OrderCode = fresh?.OrderCode;
            model.TaxPercent = fresh?.TaxPercent ?? 0;
            model.DefaultService = fresh?.DefaultService ?? 0;
            model.Menu = fresh?.Menu ?? new();
            if (model.OnlineOrderId.HasValue)
            {
                model.DeliveryZoneId = fresh?.DeliveryZoneId;
                model.DeliveryFee = fresh?.DeliveryFee ?? 0;
                model.DeliveryZoneArName = fresh?.DeliveryZoneArName;
                model.DeliveryZoneEnName = fresh?.DeliveryZoneEnName;
            }

            await PopulateData(model);
        }
    }
}
