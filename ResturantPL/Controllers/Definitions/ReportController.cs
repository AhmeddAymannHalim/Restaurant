using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Definitions;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Management")]
    public class ReportController : Controller
    {
        private static readonly string[] ReportNames =
        {
            "MoneySafe",
            "Profit",
            "Orders",
            "Drivers",
            "Invoices",
            "SupplierStatement",
            "CustomerStatement",
            "SuppliersBalances",
            "SupplierPayments",
            "CustomerPayments",
            "Deposits",
            "Expenses",
            "Salaries",
            "Scrounges"
        };

        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View(ReportNames);
        }

        public async Task<IActionResult> Show([FromQuery] ReportRequest request)
        {
            try
            {
                if (request == null || !ReportNames.Contains(request.Report))
                {
                    return RedirectToAction(nameof(Index));
                }

                var report = await _service.BuildReportAsync(request);

                return View(report);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex);
            }
        }
    }
}
