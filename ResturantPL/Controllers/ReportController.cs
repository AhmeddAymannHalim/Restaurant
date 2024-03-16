using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using ResturantPL.ViewModels;
using Rotativa.AspNetCore;
using Service.UnitOfWork;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace ResturantPL.Controllers
{
    public class ReportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            this._userManager = userManager;
            this._unitOfWork = unitOfWork;
        }

        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }

        // page for all reports
        public IActionResult Index()
        {
            return View();
        }





        // Orders between 2 dates
        public IActionResult SearchInOrders()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllOrdersByDates(DateTime startDate, DateTime endDate)
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(
                    a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems))
                .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date && a.Cash && a.FinishShift)
                .OrderByDescending(a => a.OrderDate).ToList();

            return PartialView("_AllOrdersByDatesPartial", orders);
        }

        public IActionResult OrdersReport(DateTime startDate, DateTime endDate)
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(
                    a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems))
                .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date && a.Cash && a.FinishShift)
                .OrderByDescending(a => a.OrderDate).ToList();

            ViewBag.StartDate = startDate.Date;
            ViewBag.EndDate = endDate.Date;
            var viewAsPdf = new ViewAsPdf
            {
                Model = orders,
                FileName = "تقرير طلبات بين تاريخين.pdf"
            };
            return viewAsPdf;
        }








        // Deposit between 2 dates
        public IActionResult SearchInImports()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllImportsByDates(DateTime startDate, DateTime endDate)
        {
            var imports = _unitOfWork.DepositRepository
                .GetAll(
                    a => a.Include(a => a.User))
                .Where(a => a.CreatedTime.Date >= startDate.Date && a.CreatedTime.Date <= endDate.Date && a.IsDeleted == false)
                .OrderByDescending(a => a.CreatedTime).ToList();

            return PartialView("_AllImportsByDatesPartial", imports);
        }

        public IActionResult ImportsReport(DateTime startDate, DateTime endDate)
        {

            var imports = _unitOfWork.DepositRepository
                .GetAll(
                    a => a.Include(a => a.User))
                .Where(a => a.CreatedTime.Date >= startDate.Date && a.CreatedTime.Date <= endDate.Date && a.IsDeleted == false)
                .OrderByDescending(a => a.CreatedTime).ToList();


            var viewAsPdf = new ViewAsPdf
            {
                Model = imports,
                FileName = "تقرير الايداعات بين تاريخين.pdf"
            };
            return viewAsPdf;
        }





        // Expense between 2 dates
        public IActionResult SearchInExpense()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllExpensesByDates(DateTime startDate, DateTime endDate)
        {
            var expense = _unitOfWork.ExitExpenseRepository
                .GetAll(a => a.Include(a => a.User).Include(a => a.Expense))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                .OrderByDescending(a => a.Date).ToList();

            return PartialView("_AllExpensesByDatesPartial", expense);
        }

        public IActionResult ExpensesReport(DateTime startDate, DateTime endDate)
        {
            var expense = _unitOfWork.ExitExpenseRepository
              .GetAll(a => a.Include(a => a.User).Include(a => a.Expense))
              .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
              .OrderByDescending(a => a.Date).ToList();

            var viewAsPdf = new ViewAsPdf
            {
                Model = expense,
                FileName = "تقرير المصروفات بين تاريخين.pdf"
            };
            return viewAsPdf;
        }






        // Supplier Payments between 2 dates
        public IActionResult SearchInSupplierPayment()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllSupplierPaymentsByDates(DateTime startDate, DateTime endDate)
        {
            var payment = _unitOfWork.PaymentInvoiceSupplierRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                .OrderByDescending(a => a.Date).ToList();

            return PartialView("_AllSupplierPaymentsByDatesPartial", payment);
        }

        public IActionResult SupplierPaymentsReport(DateTime startDate, DateTime endDate)
        {
            var payment = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
               .OrderByDescending(a => a.Date).ToList();

            var viewAsPdf = new ViewAsPdf
            {
                Model = payment,
                FileName = "تقرير سندات صرف الموردين بين تاريخين.pdf"
            };
            return viewAsPdf;
        }






        // Supplier Balance between 2 dates
        public IActionResult SearchInSupplierBalance()
        {
            ViewBag.Suppliers = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");

            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllSupplierBalanceByDates(int supplierId, DateTime startDate, DateTime endDate)
        {
            var sup = _unitOfWork.SupplierRepository.GetById(a => a.Id == supplierId, new string[] { "Invoices", "PaymentInvoices" });

            var LastPurchases = _unitOfWork.InvoiceRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.InvoiceDate.Date < startDate.Date
                && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
               .OrderByDescending(a => a.InvoiceDate).ToList();

            var lastReturns = _unitOfWork.InvoiceRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.InvoiceDate.Date < startDate.Date
                && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
                .OrderByDescending(a => a.InvoiceDate).ToList();

            var lastPayments = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date < startDate.Date
               && a.SupplierId == supplierId)
               .OrderByDescending(a => a.Date).ToList();



            var lastBalances = LastPurchases.Select(item => new SupplierBalanceViewModel
            {
                Deptor = item.Remaining,
                Creditor = 0,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مشتريات",
                InvoiceId = item.Id
            })
            .Concat(lastReturns.Select(item => new SupplierBalanceViewModel
            {
                Deptor = 0,
                Creditor = item.Remaining,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مرتجعات",
                InvoiceId = item.Id
            }))
            .Concat(lastPayments.Select(item => new SupplierBalanceViewModel
            {
                Deptor = 0,
                Creditor = item.Paid,
                Date = item.Date.Date,
                Info = "سندات قبض",
                InvoiceId = item.Id
            }))
            .OrderByDescending(a => a.Date)
            .ToList();


            decimal lastTotalDeptor = lastBalances.Sum(b => b.Deptor);
            decimal lastTotalCreditor = lastBalances.Sum(b => b.Creditor);
            decimal lastOverallBalance = lastTotalDeptor - lastTotalCreditor;




            var purchases = _unitOfWork.InvoiceRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.InvoiceDate.Date >= startDate.Date
                && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
                .OrderByDescending(a => a.InvoiceDate).ToList();

            var returns = _unitOfWork.InvoiceRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.InvoiceDate.Date >= startDate.Date
                && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
                .OrderByDescending(a => a.InvoiceDate).ToList();

            var payments = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date >= startDate.Date
               && a.Date.Date <= endDate.Date && a.SupplierId == supplierId)
               .OrderByDescending(a => a.Date).ToList();

            var balances = purchases.Select(item => new SupplierBalanceViewModel
            {
                Deptor = item.Remaining,
                Creditor = 0,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مشتريات",
                InvoiceId = item.Id
            })
             .Concat(returns.Select(item => new SupplierBalanceViewModel
             {
                 Deptor = 0,
                 Creditor = item.Remaining,
                 Date = item.InvoiceDate.Date,
                 Info = "فاتورة مرتجعات",
                 InvoiceId = item.Id
             }))
             .Concat(payments.Select(item => new SupplierBalanceViewModel
             {
                 Deptor = 0,
                 Creditor = item.Paid,
                 Date = item.Date.Date,
                 Info = "سندات قبض",
                 InvoiceId = item.Id
             }))
             .OrderByDescending(a => a.Date)
             .ToList();


            decimal totalDeptor = balances.Sum(b => b.Deptor);
            decimal totalCreditor = balances.Sum(b => b.Creditor);
            decimal overallBalance = totalDeptor - totalCreditor;

            SupplierTotalViewModel supplierTotalViewModel = new SupplierTotalViewModel()
            {
                Name = sup.Name,
                Date = DateTime.Now,
                balances = balances,
                TotalDeptor = totalDeptor,
                TotalCreditor = totalCreditor,
                Total = overallBalance,
                LastTotalCreditor = lastTotalCreditor,
                LastTotalDeptor = lastTotalDeptor,
                LastTotal = lastOverallBalance
            };


            return PartialView("_AllSupplierBalancesPartial", supplierTotalViewModel);
        }

        public IActionResult SupplierBalanceReport(int supplierId, DateTime startDate, DateTime endDate)
        {
            var sup = _unitOfWork.SupplierRepository.GetById(a => a.Id == supplierId, new string[] { "Invoices", "PaymentInvoices" });

            var LastPurchases = _unitOfWork.InvoiceRepository
              .GetAll(a => a.Include(a => a.Supplier))
              .Where(a => a.InvoiceDate.Date < startDate.Date
               && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
              .OrderByDescending(a => a.InvoiceDate).ToList();

            var lastReturns = _unitOfWork.InvoiceRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.InvoiceDate.Date < startDate.Date
                && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
                .OrderByDescending(a => a.InvoiceDate).ToList();

            var lastPayments = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date < startDate.Date
               && a.SupplierId == supplierId)
               .OrderByDescending(a => a.Date).ToList();



            var lastBalances = LastPurchases.Select(item => new SupplierBalanceViewModel
            {
                Deptor = item.Remaining,
                Creditor = 0,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مشتريات",
                InvoiceId = item.Id
            })
            .Concat(lastReturns.Select(item => new SupplierBalanceViewModel
            {
                Deptor = 0,
                Creditor = item.Remaining,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مرتجعات",
                InvoiceId = item.Id
            }))
            .Concat(lastPayments.Select(item => new SupplierBalanceViewModel
            {
                Deptor = 0,
                Creditor = item.Paid,
                Date = item.Date.Date,
                Info = "سندات قبض",
                InvoiceId = item.Id
            }))
            .OrderByDescending(a => a.Date)
            .ToList();


            decimal lastTotalDeptor = lastBalances.Sum(b => b.Deptor);
            decimal lastTotalCreditor = lastBalances.Sum(b => b.Creditor);
            decimal lastOverallBalance = lastTotalDeptor - lastTotalCreditor;















            var purchases = _unitOfWork.InvoiceRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.InvoiceDate.Date >= startDate.Date
               && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
               .OrderByDescending(a => a.InvoiceDate).ToList();

            var returns = _unitOfWork.InvoiceRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.InvoiceDate.Date >= startDate.Date
                && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
                .OrderByDescending(a => a.InvoiceDate).ToList();

            var payments = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date >= startDate.Date
               && a.Date.Date <= endDate.Date && a.SupplierId == supplierId)
               .OrderByDescending(a => a.Date).ToList();



            var balances = purchases.Select(item => new SupplierBalanceViewModel
            {
                Deptor = item.Remaining,
                Creditor = 0,
                Date = item.InvoiceDate.Date,
                Info = "فاتورة مشتريات",
                InvoiceId = item.Id
            })
             .Concat(returns.Select(item => new SupplierBalanceViewModel
             {
                 Deptor = 0,
                 Creditor = item.Remaining,
                 Date = item.InvoiceDate.Date,
                 Info = "فاتورة مرتجعات",
                 InvoiceId = item.Id
             }))
             .Concat(payments.Select(item => new SupplierBalanceViewModel
             {
                 Deptor = 0,
                 Creditor = item.Paid,
                 Date = item.Date.Date,
                 Info = "سندات قبض",
                 InvoiceId = item.Id
             }))
             .OrderByDescending(a => a.Date)
             .ToList();

            decimal totalDeptor = balances.Sum(b => b.Deptor);
            decimal totalCreditor = balances.Sum(b => b.Creditor);
            decimal overallBalance = totalDeptor - totalCreditor;

            SupplierTotalViewModel supplierTotalViewModel = new SupplierTotalViewModel()
            {
                Name = sup.Name,
                Date = DateTime.Now,
                balances = balances,
                TotalDeptor = totalDeptor,
                TotalCreditor = totalCreditor,
                Total = overallBalance,
                LastTotalCreditor = lastTotalCreditor,
                LastTotalDeptor = lastTotalDeptor,
                LastTotal = lastOverallBalance
            };


            var viewAsPdf = new ViewAsPdf
            {
                Model = supplierTotalViewModel,
                FileName = $"تقرير كشف حساب المورد {sup.Name} بين تاريخين.pdf"
            };
            return viewAsPdf;
        }













        // Scrounge between 2 dates
        public IActionResult SearchInScrounge()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllScroungesByDates(string type, DateTime startDate, DateTime endDate)
        {
            List<Scrounge> scrounge = new List<Scrounge>();
            if (type == "All")
            {
                scrounge = _unitOfWork.ScroungeRepository
                .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                .OrderByDescending(a => a.Date).ToList();
            }
            else if (type == "Dept")
            {
                scrounge = _unitOfWork.ScroungeRepository
                .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Dept)
                .OrderByDescending(a => a.Date).ToList();
            }
            else if (type == "Discount")
            {
                scrounge = _unitOfWork.ScroungeRepository
               .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Discount)
               .OrderByDescending(a => a.Date).ToList();
            }
            else
            {
                scrounge = _unitOfWork.ScroungeRepository
               .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Incentive)
               .OrderByDescending(a => a.Date).ToList();
            }

            return PartialView("_AllScroungesByDatesPartial", scrounge);
        }

        public IActionResult ScroungesReport(string type, DateTime startDate, DateTime endDate)
        {
            List<Scrounge> scrounge = new List<Scrounge>();
            if (type == "All")
            {
                scrounge = _unitOfWork.ScroungeRepository
                   .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
                   .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                   .OrderByDescending(a => a.Date).ToList();
            }
            else if (type == "Dept")
            {
                scrounge = _unitOfWork.ScroungeRepository
                .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Dept)
                .OrderByDescending(a => a.Date).ToList();
            }
            else if (type == "Discount")
            {
                scrounge = _unitOfWork.ScroungeRepository
               .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Discount)
               .OrderByDescending(a => a.Date).ToList();
            }
            else
            {
                scrounge = _unitOfWork.ScroungeRepository
               .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Incentive)
               .OrderByDescending(a => a.Date).ToList();
            }
            var viewAsPdf = new ViewAsPdf
            {
                Model = scrounge,
                FileName = "تقرير مصروفات الموظفين بين تاريخين.pdf"
            };
            return viewAsPdf;
        }




        // Salary between 2 dates
        public IActionResult SearchInSalary()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllSalaryByDates(DateTime startDate, DateTime endDate)
        {

            var salary = _unitOfWork.SalaryRepository
             .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
             .Where(a => a.SalaryDate.Date >= startDate.Date && a.SalaryDate.Date <= endDate.Date)
             .OrderByDescending(a => a.SalaryDate).ToList();


            return PartialView("_AllSalaryByDatesPartial", salary);
        }

        public IActionResult SalaryReport(DateTime startDate, DateTime endDate)
        {

            var salary = _unitOfWork.SalaryRepository
              .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
              .Where(a => a.SalaryDate.Date >= startDate.Date && a.SalaryDate.Date <= endDate.Date)
              .OrderByDescending(a => a.SalaryDate).ToList();

            var viewAsPdf = new ViewAsPdf
            {
                Model = salary,
                FileName = "تقرير مرتبات الموظفين بين تاريخين.pdf"
            };
            return viewAsPdf;
        }








        // Benefits between 2 dates
        public IActionResult SearchInBenefits()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult BenefitsByDates(DateTime startDate, DateTime endDate)
        {
            decimal orders = _unitOfWork.OrderRepository
                .GetAll(
                    a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems).ThenInclude(a => a.MenuItem))
                .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date && a.Cash && a.FinishShift)
                .OrderByDescending(a => a.OrderDate).ToList().Sum(a => a.OrderMenuItems.Sum(a => a.MenuItem.Benefit * a.Quantity));

            return Json(orders);
        }

        public IActionResult BenefitReport(DateTime startDate, DateTime endDate)
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(
                    a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems))
                .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date && a.Cash && a.FinishShift)
                .OrderByDescending(a => a.OrderDate).ToList();

            ViewBag.StartDate = startDate.Date;
            ViewBag.EndDate = endDate.Date;
            var viewAsPdf = new ViewAsPdf
            {
                Model = orders,
                FileName = "تقرير طلبات بين تاريخين.pdf"
            };
            return viewAsPdf;
        }







        // Invoices between 2 dates
        public IActionResult SearchInInvoices()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllInvoicesByDates(string type, DateTime startDate, DateTime endDate)
        {
            List<Invoice> invoices = new List<Invoice>();
            if (type == "All")
            {

                invoices = _unitOfWork.InvoiceRepository.GetAll(
                  a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
                  a => a.Include(a => a.Supplier),
                  a => a.Include(a => a.Store))
                   .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date)
                   .OrderByDescending(a => a.InvoiceDate).ToList();
            }
            else if (type == "Purchase")
            {
                invoices = _unitOfWork.InvoiceRepository.GetAll(
              a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
              a => a.Include(a => a.Supplier),
              a => a.Include(a => a.Store))
               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Purchases)
               .OrderByDescending(a => a.InvoiceDate).ToList();
            }
            else
            {
                invoices = _unitOfWork.InvoiceRepository.GetAll(
                              a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
                              a => a.Include(a => a.Supplier),
                              a => a.Include(a => a.Store))
                               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Returns)
                               .OrderByDescending(a => a.InvoiceDate).ToList();
            }

            return PartialView("_AllInvoicesPartial", invoices);
        }

        public IActionResult InvoicesReport(string type, DateTime startDate, DateTime endDate)
        {
            List<Invoice> invoices = new List<Invoice>();
            if (type == "All")
            {

                invoices = _unitOfWork.InvoiceRepository.GetAll(
                  a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
                  a => a.Include(a => a.Supplier),
                  a => a.Include(a => a.Store))
                   .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date)
                   .OrderByDescending(a => a.InvoiceDate).ToList();
            }
            else if (type == "Purchase")
            {
                invoices = _unitOfWork.InvoiceRepository.GetAll(
              a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
              a => a.Include(a => a.Supplier),
              a => a.Include(a => a.Store))
               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Purchases)
               .OrderByDescending(a => a.InvoiceDate).ToList();
            }
            else
            {
                invoices = _unitOfWork.InvoiceRepository.GetAll(
                              a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
                              a => a.Include(a => a.Supplier),
                              a => a.Include(a => a.Store))
                               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Returns)
                               .OrderByDescending(a => a.InvoiceDate).ToList();
            }
            var viewAsPdf = new ViewAsPdf
            {
                Model = invoices,
                FileName = "تقرير الفواتير بين تاريخين.pdf"
            };
            return viewAsPdf;
        }





        // Tayar Orders between 2 dates
        public IActionResult SearchInTayars()
        {
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");

            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllTayarOrdersByDates(int employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = _unitOfWork.EmployeeRepository.GetById(a => a.EmployeeId == employeeId, new string[] { "Orders" });

            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems))
                .Where(a => a.OrderDate.Date >= startDate.Date
                && a.OrderDate.Date <= endDate.Date && a.Cash &&
                a.FinishShift && a.EmployeeId == employee.EmployeeId)
                .OrderByDescending(a => a.OrderDate).ToList();

            return PartialView("_AllTayarOrdersPartial", orders);
        }

        public IActionResult TayarsReport(int employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = _unitOfWork.EmployeeRepository.GetById(a => a.EmployeeId == employeeId, new string[] { "Orders" });

            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller)
                                    .Include(a => a.Receiver)
                                    .Include(a => a.Table)
                                    .Include(a => a.Employee)
                                    .Include(a => a.OrderMenuItems))
                .Where(a => a.OrderDate.Date >= startDate.Date
                && a.OrderDate.Date <= endDate.Date && a.Cash &&
                a.FinishShift && a.EmployeeId == employee.EmployeeId)
                .OrderByDescending(a => a.OrderDate).ToList();

            var viewAsPdf = new ViewAsPdf
            {
                Model = orders,
                FileName = $"تقرير طلبات الطيار {employee.FullName} بين تاريخين.pdf"
            };
            return viewAsPdf;
        }





        // Money Safe between 2 dates
        public IActionResult SearchInMoneySafe()
        {
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult MoneySafeByDateas(DateTime startDate, DateTime endDate)
        {
            MoneySafeViewModel moneySafe = new MoneySafeViewModel()
            {
                Imports = _unitOfWork.DepositRepository.GetAll()
                .Where(a => a.CreatedTime.Date >= startDate.Date && a.CreatedTime.Date <= endDate.Date).Sum(a => a.Amount),
                Sales = _unitOfWork.OrderRepository.GetAll()
                .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date && a.Cash && a.FinishShift).Sum(a => a.TotalPrice),
                Returns = _unitOfWork.InvoiceRepository.GetAll()
                .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Returns).Sum(a => a.Paid),
                Purchases = _unitOfWork.InvoiceRepository.GetAll()
                .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Purchases).Sum(a => a.Paid),
                Expenses = _unitOfWork.ExitExpenseRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date).Sum(a => a.Price),
                EmployeeExpenses = _unitOfWork.ScroungeRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Dept).Sum(a => a.Amount) + _unitOfWork.SalaryRepository.GetAll()
                .Where(a => a.SalaryDate.Date >= startDate.Date && a.SalaryDate.Date <= endDate.Date).Sum(a => a.SalaryAmount),

                SupplierPayment = _unitOfWork.PaymentInvoiceSupplierRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date).Sum(a => a.Paid),
            };
            moneySafe.TotalImports = moneySafe.Imports + moneySafe.Sales + moneySafe.Returns;
            moneySafe.TotalExports = moneySafe.Purchases + moneySafe.Expenses + moneySafe.EmployeeExpenses + moneySafe.SupplierPayment;
            moneySafe.TotalMoney = moneySafe.TotalImports - moneySafe.TotalExports;

            return Json(moneySafe);
        }

        public IActionResult MoneySafeReport(DateTime startDate, DateTime endDate)
        {
            MoneySafeViewModel moneySafe = new MoneySafeViewModel()
            {
                Imports = _unitOfWork.DepositRepository.GetAll()
               .Where(a => a.CreatedTime.Date >= startDate.Date && a.CreatedTime.Date <= endDate.Date).Sum(a => a.Amount),
                Sales = _unitOfWork.OrderRepository.GetAll()
               .Where(a => a.OrderDate.Date >= startDate.Date && a.OrderDate.Date <= endDate.Date).Sum(a => a.TotalPrice),
                Returns = _unitOfWork.InvoiceRepository.GetAll()
               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Returns).Sum(a => a.Paid),
                Purchases = _unitOfWork.InvoiceRepository.GetAll()
               .Where(a => a.InvoiceDate.Date >= startDate.Date && a.InvoiceDate.Date <= endDate.Date && a.InvoiceType == InvoiceType.Purchases).Sum(a => a.Paid),
                Expenses = _unitOfWork.ExitExpenseRepository.GetAll()
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date).Sum(a => a.Price),
                EmployeeExpenses = _unitOfWork.ScroungeRepository.GetAll()
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.State == MoneyState.Dept).Sum(a => a.Amount) + _unitOfWork.SalaryRepository.GetAll()
               .Where(a => a.SalaryDate.Date >= startDate.Date && a.SalaryDate.Date <= endDate.Date).Sum(a => a.SalaryAmount),

                SupplierPayment = _unitOfWork.PaymentInvoiceSupplierRepository.GetAll()
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date).Sum(a => a.Paid),
            };
            moneySafe.TotalImports = moneySafe.Imports + moneySafe.Sales + moneySafe.Returns;
            moneySafe.TotalExports = moneySafe.Purchases + moneySafe.Expenses + moneySafe.EmployeeExpenses + moneySafe.SupplierPayment;
            moneySafe.TotalMoney = moneySafe.TotalImports - moneySafe.TotalExports;

            moneySafe.FromDate = startDate;
            moneySafe.ToDate = endDate;
            moneySafe.ReportDate = DateTime.Now;




            var viewAsPdf = new ViewAsPdf
            {
                Model = moneySafe,
                FileName = "تقرير حركة الخزينة بين تاريخين.pdf"
            };
            return viewAsPdf;
        }















        // Export Order to pdf
        public IActionResult OrderReport(int orderId)
        {
            var order = _unitOfWork.OrderRepository.GetById(a => a.Id == orderId,
                new string[] { "Seller", "Receiver", "Table", "Customer", "Employee", "OrderMenuItems" },
                new string[] { "OrderMenuItems.MenuItem" });

            //if (order.Complete == false)
            //{
            //    order.Complete = true;
            //    _unitOfWork.OrderRepository.Update(order);
            //}

            var viewAsPdf = new ViewAsPdf
            {
                Model = order,
                //ViewName = "OrderReport",
                FileName = $"{order.CustomerName}.pdf"
            };
            return viewAsPdf;
        }

        // Export Invoice to pdf
        public IActionResult InvoiceReport(int invoiceId)
        {
            Invoice invoice = _unitOfWork.InvoiceRepository.
                 GetById(a => a.Id == invoiceId, new string[] { "Supplier", "InvoiceItems", "Store" }
                 , new string[] { "InvoiceItems.StoreProduct", "InvoiceItems.UnitPrice" });

            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة رقم {invoice.InvoiceCode}.pdf"
            };
            return viewAsPdf;
        }

        // Report for convert product between stores invoice 
        public IActionResult StoreConvertReport(int invoiceId)
        {
            StoreConvert invoice = _unitOfWork.StoreConvertRepository.
                 GetById(a => a.Id == invoiceId, new string[] { "FromStore", "ToStore", "StoreConvertProducts" }, new string[] { "StoreConvertProducts.StoreProduct" });

            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة نقل اصناف رقم {invoice.ConvertCode}.pdf"
            };
            return viewAsPdf;
        }

        // Report for  payment invoice for supplier
        public IActionResult PayForSuppliertReport(int invoiceId)
        {
            PaymentInvoiceSupplier invoice = _unitOfWork.PaymentInvoiceSupplierRepository.
                GetById(a => a.Id == invoiceId, new string[] { "Supplier" });
            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة تسديد المورد : {invoice.Supplier.Name}.pdf"
            };
            return viewAsPdf;
        }

        // Report for  payment invoice for customer
        public IActionResult PayForCustomerReport(int invoiceId)
        {
            PaymentInvoiceCustomer invoice = _unitOfWork.PaymentInvoiceCustomerRepository
                .GetById(a => a.Id == invoiceId, new string[] { "Customer" });
            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة تسديد العميل : {invoice.Customer.Name}.pdf"
            };
            return viewAsPdf;
        }

        // Report for End Shift
        public IActionResult EndShiftReport(EndShiftViewModel model)
        {
            model.Name = CurrentUser().FullName;
            model.Date = DateTime.Now;

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
                FileName = $"فاتورة انهاء شيفت : {model.Name}.pdf"
            };
            return viewAsPdf;
        }




        //               foreach (var item in purchases)
        //            {
        //                SupplierBalanceViewModel supplier = new SupplierBalanceViewModel()
        //                {
        //                    Deptor = item.Remaining,
        //                    Creditor = 0,
        //                    Date = item.InvoiceDate.Date,
        //                    Info = "فاتورة مشتريات",
        //                    InvoiceId = item.Id
        //                };
        //        balances.Add(supplier);
        //            }
        //            foreach (var item in returns)
        //            {
        //                SupplierBalanceViewModel supplier = new SupplierBalanceViewModel()
        //                {
        //                    Deptor = 0,
        //                    Creditor = item.Remaining,
        //                    Date = item.InvoiceDate.Date,
        //                    Info = "فاتورة مرتجعات",
        //                    InvoiceId = item.Id
        //                };
        //    balances.Add(supplier);
        //            }
        //foreach (var item in payments)
        //{
        //    SupplierBalanceViewModel supplier = new SupplierBalanceViewModel()
        //    {
        //        Deptor = 0,
        //        Creditor = item.Paid,
        //        Date = item.Date.Date,
        //        Info = "سندات قبض",
        //        InvoiceId = item.Id
        //    };
        //    balances.Add(supplier);
        //}








    }
}
