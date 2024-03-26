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
using static NuGet.Packaging.PackagingConstants;

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

            var model = new OrdersByDatesViewModel()
            {
                Orders = orders,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = orders.Sum(a => a.TotalPrice)
            };


            return PartialView("_AllOrdersByDatesPartial", model);
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

            var model = new OrdersByDatesViewModel()
            {
                Orders = orders,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = orders.Sum(a => a.TotalPrice)
            };

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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

            var model = new ImportsByDatesViewModel()
            {
                Deposits = imports,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = imports.Sum(a => a.Amount)
            };


            return PartialView("_AllImportsByDatesPartial", model);
        }

        public IActionResult ImportsReport(DateTime startDate, DateTime endDate)
        {

            var imports = _unitOfWork.DepositRepository
                .GetAll(
                    a => a.Include(a => a.User))
                .Where(a => a.CreatedTime.Date >= startDate.Date && a.CreatedTime.Date <= endDate.Date && a.IsDeleted == false)
                .OrderByDescending(a => a.CreatedTime).ToList();

            var model = new ImportsByDatesViewModel()
            {
                Deposits = imports,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = imports.Sum(a => a.Amount)
            };


            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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


            var model = new ExpensesByDatesViewModel()
            {
                Expenses = expense,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = expense.Sum(a => a.Price)
            };

            return PartialView("_AllExpensesByDatesPartial", model);
        }

        public IActionResult ExpensesReport(DateTime startDate, DateTime endDate)
        {
            var expense = _unitOfWork.ExitExpenseRepository
              .GetAll(a => a.Include(a => a.User).Include(a => a.Expense))
              .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
              .OrderByDescending(a => a.Date).ToList();

            var model = new ExpensesByDatesViewModel()
            {
                Expenses = expense,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = expense.Sum(a => a.Price)
            };


            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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
            var payments = _unitOfWork.PaymentInvoiceSupplierRepository
                .GetAll(a => a.Include(a => a.Supplier))
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                .OrderByDescending(a => a.Date).ToList();

            var model = new PaymentByDatesViewModel()
            {
                Payments = payments,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = payments.Sum(a => a.Paid)
            };


            return PartialView("_AllSupplierPaymentsByDatesPartial", model);
        }

        public IActionResult SupplierPaymentsReport(DateTime startDate, DateTime endDate)
        {
            var payments = _unitOfWork.PaymentInvoiceSupplierRepository
               .GetAll(a => a.Include(a => a.Supplier))
               .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
               .OrderByDescending(a => a.Date).ToList();

            var model = new PaymentByDatesViewModel()
            {
                Payments = payments,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = payments.Sum(a => a.Paid)
            };

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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
            decimal currentBalance = lastOverallBalance;

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

            var balances = new List<SupplierBalanceViewModel>();

            var allTransactions = new List<(DateTime Date, decimal Amount, string Type)>();

            allTransactions.AddRange(purchases.Select(item => (item.InvoiceDate.Date, item.Remaining, "Purchase")));
            allTransactions.AddRange(returns.Select(item => (item.InvoiceDate.Date, -item.Remaining, "Return")));
            allTransactions.AddRange(payments.Select(item => (item.Date.Date, -item.Paid, "Payment")));

            allTransactions = allTransactions.OrderBy(t => t.Date).ToList();

            foreach (var transaction in allTransactions)
            {
                currentBalance += transaction.Amount;

                string info;
                int invoiceId = 0;
                if (transaction.Type == "Purchase")
                {
                    info = "فاتورة مشتريات";
                    invoiceId = purchases.FirstOrDefault(p => p.InvoiceDate.Date == transaction.Date)?.Id ?? 0;
                }
                else if (transaction.Type == "Return")
                {
                    info = "فاتورة مرتجعات";
                    invoiceId = returns.FirstOrDefault(r => r.InvoiceDate.Date == transaction.Date)?.Id ?? 0;
                }
                else
                {
                    info = "سندات قبض";
                    invoiceId = payments.FirstOrDefault(pm => pm.Date.Date == transaction.Date)?.Id ?? 0;
                }

                balances.Add(new SupplierBalanceViewModel
                {
                    Deptor = transaction.Amount > 0 ? transaction.Amount : 0,
                    Creditor = transaction.Amount < 0 ? -transaction.Amount : 0,
                    Balance = currentBalance,
                    Date = transaction.Date,
                    Info = info,
                    InvoiceId = invoiceId
                });
            }

            decimal totalDeptor = balances.Sum(b => b.Deptor);
            decimal totalCreditor = balances.Sum(b => b.Creditor);
            decimal overallBalance = currentBalance;

            decimal allTotalDeptor = totalDeptor + lastTotalDeptor;
            decimal allTotalCreditor = totalCreditor + lastTotalCreditor;
            decimal balance = allTotalDeptor - allTotalCreditor;

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
                LastTotal = lastOverallBalance,
                AllTotalCreditor = allTotalCreditor,
                AllTotalDeptor = allTotalDeptor,
                Balance = balance
            };

            return PartialView("_AllSupplierBalancesPartial", supplierTotalViewModel);
        }


        #region last Supplier balance



        //public IActionResult AllSupplierBalanceByDates(int supplierId, DateTime startDate, DateTime endDate)
        //{
        //    var sup = _unitOfWork.SupplierRepository.GetById(a => a.Id == supplierId, new string[] { "Invoices", "PaymentInvoices" });

        //    var LastPurchases = _unitOfWork.InvoiceRepository
        //       .GetAll(a => a.Include(a => a.Supplier))
        //       .Where(a => a.InvoiceDate.Date < startDate.Date
        //        && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
        //       .OrderByDescending(a => a.InvoiceDate).ToList();

        //    var lastReturns = _unitOfWork.InvoiceRepository
        //        .GetAll(a => a.Include(a => a.Supplier))
        //        .Where(a => a.InvoiceDate.Date < startDate.Date
        //        && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
        //        .OrderByDescending(a => a.InvoiceDate).ToList();

        //    var lastPayments = _unitOfWork.PaymentInvoiceSupplierRepository
        //       .GetAll(a => a.Include(a => a.Supplier))
        //       .Where(a => a.Date.Date < startDate.Date
        //       && a.SupplierId == supplierId)
        //       .OrderByDescending(a => a.Date).ToList();




        //    var lastBalances = LastPurchases.Select(item => new SupplierBalanceViewModel
        //    {
        //        Deptor = item.Remaining,
        //        Creditor = 0,
        //        Date = item.InvoiceDate.Date,
        //        Info = "فاتورة مشتريات",
        //        InvoiceId = item.Id
        //    })
        //    .Concat(lastReturns.Select(item => new SupplierBalanceViewModel
        //    {
        //        Deptor = 0,
        //        Creditor = item.Remaining,
        //        Date = item.InvoiceDate.Date,
        //        Info = "فاتورة مرتجعات",
        //        InvoiceId = item.Id
        //    }))
        //    .Concat(lastPayments.Select(item => new SupplierBalanceViewModel
        //    {
        //        Deptor = 0,
        //        Creditor = item.Paid,
        //        Date = item.Date.Date,
        //        Info = "سندات قبض",
        //        InvoiceId = item.Id
        //    }))
        //    .OrderByDescending(a => a.Date)
        //    .ToList();


        //    decimal lastTotalDeptor = lastBalances.Sum(b => b.Deptor);
        //    decimal lastTotalCreditor = lastBalances.Sum(b => b.Creditor);
        //    //decimal lastOverallBalance = lastTotalDeptor - lastTotalCreditor;
        //    //decimal currentBalance = lastOverallBalance;
        //    decimal lastOverallBalance = 0;
        //    foreach (var purchase in LastPurchases)
        //    {
        //        lastOverallBalance += purchase.Remaining;
        //    }

        //    foreach (var ret in lastReturns)
        //    {
        //        lastOverallBalance -= ret.Remaining;
        //    }

        //    foreach (var payment in lastPayments)
        //    {
        //        lastOverallBalance -= payment.Paid;
        //    }


        //    var purchases = _unitOfWork.InvoiceRepository
        //        .GetAll(a => a.Include(a => a.Supplier))
        //        .Where(a => a.InvoiceDate.Date >= startDate.Date
        //        && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Purchases)
        //        .OrderByDescending(a => a.InvoiceDate).ToList();

        //    var returns = _unitOfWork.InvoiceRepository
        //        .GetAll(a => a.Include(a => a.Supplier))
        //        .Where(a => a.InvoiceDate.Date >= startDate.Date
        //        && a.InvoiceDate.Date <= endDate.Date && a.SupplierId == supplierId && a.InvoiceType == InvoiceType.Returns)
        //        .OrderByDescending(a => a.InvoiceDate).ToList();

        //    var payments = _unitOfWork.PaymentInvoiceSupplierRepository
        //       .GetAll(a => a.Include(a => a.Supplier))
        //       .Where(a => a.Date.Date >= startDate.Date
        //       && a.Date.Date <= endDate.Date && a.SupplierId == supplierId)
        //       .OrderByDescending(a => a.Date).ToList();

        //    //var balances = purchases.Select(item => new SupplierBalanceViewModel
        //    //{
        //    //    Deptor = item.Remaining,
        //    //    Creditor = 0,
        //    //    Balance = item.Remaining,
        //    //    Date = item.InvoiceDate.Date,
        //    //    Info = "فاتورة مشتريات",
        //    //    InvoiceId = item.Id
        //    //})
        //    // .Concat(returns.Select(item => new SupplierBalanceViewModel
        //    // {
        //    //     Deptor = 0,
        //    //     Creditor = item.Remaining,
        //    //     Balance = -item.Remaining,
        //    //     Date = item.InvoiceDate.Date,
        //    //     Info = "فاتورة مرتجعات",
        //    //     InvoiceId = item.Id
        //    // }))
        //    // .Concat(payments.Select(item => new SupplierBalanceViewModel
        //    // {
        //    //     Deptor = 0,
        //    //     Creditor = item.Paid,
        //    //     Balance = -item.Paid,
        //    //     Date = item.Date.Date,
        //    //     Info = "سندات قبض",
        //    //     InvoiceId = item.Id
        //    // }))
        //    // .OrderByDescending(a => a.Date)
        //    // .ToList();

        //    var balances = new List<SupplierBalanceViewModel>();


        //    foreach (var item in purchases)
        //    {
        //        lastOverallBalance += item.Remaining;
        //        //currentBalance += item.Remaining;
        //        balances.Add(new SupplierBalanceViewModel
        //        {
        //            Deptor = item.Remaining,
        //            Creditor = 0,
        //            Balance = lastOverallBalance,
        //            Date = item.InvoiceDate.Date,
        //            Info = "فاتورة مشتريات",
        //            InvoiceId = item.Id
        //        });
        //    }

        //    // Iterate over returns
        //    foreach (var item in returns)
        //    {
        //        lastOverallBalance -= item.Remaining;
        //        //currentBalance -= item.Remaining;
        //        balances.Add(new SupplierBalanceViewModel
        //        {
        //            Deptor = 0,
        //            Creditor = item.Remaining,
        //            Balance = lastOverallBalance,
        //            Date = item.InvoiceDate.Date,
        //            Info = "فاتورة مرتجعات",
        //            InvoiceId = item.Id
        //        });
        //    }

        //    // Iterate over payments
        //    foreach (var item in payments)
        //    {
        //        lastOverallBalance -= item.Paid;
        //        //currentBalance -= item.Paid;
        //        balances.Add(new SupplierBalanceViewModel
        //        {
        //            Deptor = 0,
        //            Creditor = item.Paid,
        //            Balance = lastOverallBalance,
        //            Date = item.Date.Date,
        //            Info = "سندات قبض",
        //            InvoiceId = item.Id
        //        });
        //    }

        //    // Sort the balances list by date descending
        //    balances = balances.OrderBy(a => a.Date).ToList();









        //    decimal totalDeptor = balances.Sum(b => b.Deptor);
        //    decimal totalCreditor = balances.Sum(b => b.Creditor);
        //    decimal overallBalance = lastOverallBalance;


        //    decimal allTotalDeptor = totalDeptor + lastTotalDeptor;
        //    decimal allTotalCreditor = totalCreditor + lastTotalCreditor;
        //    decimal balance = allTotalDeptor - allTotalCreditor;

        //    SupplierTotalViewModel supplierTotalViewModel = new SupplierTotalViewModel()
        //    {
        //        Name = sup.Name,
        //        Date = DateTime.Now,
        //        balances = balances,
        //        TotalDeptor = totalDeptor,
        //        TotalCreditor = totalCreditor,
        //        Total = overallBalance,
        //        LastTotalCreditor = lastTotalCreditor,
        //        LastTotalDeptor = lastTotalDeptor,
        //        LastTotal = lastOverallBalance,

        //        AllTotalCreditor = allTotalCreditor,
        //        AllTotalDeptor = allTotalDeptor,
        //        Balance = balance
        //    };


        //    return PartialView("_AllSupplierBalancesPartial", supplierTotalViewModel);
        //}
        #endregion


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
            decimal currentBalance = lastOverallBalance;

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

            var balances = new List<SupplierBalanceViewModel>();

            var allTransactions = new List<(DateTime Date, decimal Amount, string Type)>();

            allTransactions.AddRange(purchases.Select(item => (item.InvoiceDate.Date, item.Remaining, "Purchase")));
            allTransactions.AddRange(returns.Select(item => (item.InvoiceDate.Date, -item.Remaining, "Return")));
            allTransactions.AddRange(payments.Select(item => (item.Date.Date, -item.Paid, "Payment")));

            allTransactions = allTransactions.OrderBy(t => t.Date).ToList();

            foreach (var transaction in allTransactions)
            {
                currentBalance += transaction.Amount;

                string info;
                if (transaction.Type == "Purchase")
                {
                    info = "فاتورة مشتريات";
                }
                else if (transaction.Type == "Return")
                {
                    info = "فاتورة مرتجعات";
                }
                else
                {
                    info = "سندات قبض";
                }

                balances.Add(new SupplierBalanceViewModel
                {
                    Deptor = transaction.Amount > 0 ? transaction.Amount : 0,
                    Creditor = transaction.Amount < 0 ? -transaction.Amount : 0,
                    Balance = currentBalance,
                    Date = transaction.Date,
                    Info = info,
                    InvoiceId = 0 // You may need to adjust this based on your data model
                });
            }

            decimal totalDeptor = balances.Sum(b => b.Deptor);
            decimal totalCreditor = balances.Sum(b => b.Creditor);
            decimal overallBalance = currentBalance;

            decimal allTotalDeptor = totalDeptor + lastTotalDeptor;
            decimal allTotalCreditor = totalCreditor + lastTotalCreditor;
            decimal balance = allTotalDeptor - allTotalCreditor;

            SupplierTotalViewModel supplierTotalViewModel = new SupplierTotalViewModel()
            {
                Name = sup.Name,
                Date = DateTime.Now,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                balances = balances,
                TotalDeptor = totalDeptor,
                TotalCreditor = totalCreditor,
                Total = overallBalance,
                LastTotalCreditor = lastTotalCreditor,
                LastTotalDeptor = lastTotalDeptor,
                LastTotal = lastOverallBalance,
                AllTotalCreditor = allTotalCreditor,
                AllTotalDeptor = allTotalDeptor,
                Balance = balance
            };

            var viewAsPdf = new ViewAsPdf
            {
                Model = supplierTotalViewModel,
                FileName = $"تقرير كشف حساب المورد {sup.Name} بين تاريخين.pdf"
            };
            return viewAsPdf;
        }




        // Supplier Balance between 2 dates
        public IActionResult SearchInSuppliersBalance()
        {
            ViewBag.Suppliers = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");

            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        public IActionResult AllSuppliersBalanceByDates(DateTime startDate, DateTime endDate)
        {
            var suppliers = _unitOfWork.SupplierRepository.GetAll();

            var supplierViewModels = new List<SuppliersTotalViewModel>();

            foreach (var supplier in suppliers)
            {
                var purchases = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date >= startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Purchases)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var returns = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date >= startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Returns)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var payments = _unitOfWork.PaymentInvoiceSupplierRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.Date.Date >= startDate.Date
                                && a.Date.Date <= endDate.Date
                                && a.SupplierId == supplier.Id)
                    .OrderByDescending(a => a.Date)
                    .ToList();


                var lastPurchases = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date < startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Purchases)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var lastReturns = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date < startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Returns)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var lastPayments = _unitOfWork.PaymentInvoiceSupplierRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.Date.Date < startDate.Date
                                && a.Date.Date <= endDate.Date
                                && a.SupplierId == supplier.Id)
                    .OrderByDescending(a => a.Date)
                    .ToList();


                var lastBalances = lastPurchases.Select(item => new SupplierBalanceViewModel
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

                var supplierViewModel = new SuppliersTotalViewModel()
                {
                    Name = supplier.Name,
                    Date = DateTime.Now,
                    TotalDeptor = totalDeptor,
                    TotalCreditor = totalCreditor,
                    Total = overallBalance,

                    LastTotalCreditor = lastTotalCreditor,
                    LastTotalDeptor = lastTotalDeptor,
                    LastTotal = lastOverallBalance,

                    AllTotal = lastOverallBalance + overallBalance
                };

                supplierViewModels.Add(supplierViewModel);
            }
            var model = new SupliersByDatesViewModel()
            {
                Suppliers = supplierViewModels,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = supplierViewModels.Sum(a => a.AllTotal)
            };


            return PartialView("_AllSuppliersBalancesPartial", model);
        }
       

        public IActionResult SuppliersBalanceReport( DateTime startDate, DateTime endDate)
        {

            var suppliers = _unitOfWork.SupplierRepository.GetAll();

            var supplierViewModels = new List<SuppliersTotalViewModel>();

            foreach (var supplier in suppliers)
            {
                var purchases = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date >= startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Purchases)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var returns = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date >= startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Returns)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var payments = _unitOfWork.PaymentInvoiceSupplierRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.Date.Date >= startDate.Date
                                && a.Date.Date <= endDate.Date
                                && a.SupplierId == supplier.Id)
                    .OrderByDescending(a => a.Date)
                    .ToList();


                var lastPurchases = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date < startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Purchases)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var lastReturns = _unitOfWork.InvoiceRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.InvoiceDate.Date < startDate.Date
                                && a.InvoiceDate.Date <= endDate.Date
                                && a.SupplierId == supplier.Id
                                && a.InvoiceType == InvoiceType.Returns)
                    .OrderByDescending(a => a.InvoiceDate)
                    .ToList();

                var lastPayments = _unitOfWork.PaymentInvoiceSupplierRepository
                    .GetAll(a => a.Include(a => a.Supplier))
                    .Where(a => a.Date.Date < startDate.Date
                                && a.Date.Date <= endDate.Date
                                && a.SupplierId == supplier.Id)
                    .OrderByDescending(a => a.Date)
                    .ToList();


                var lastBalances = lastPurchases.Select(item => new SupplierBalanceViewModel
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

                // Step 3: Construct the view model for each supplier
                var supplierViewModel = new SuppliersTotalViewModel()
                {
                    Name = supplier.Name,
                    Date = DateTime.Now,
                    TotalDeptor = totalDeptor,
                    TotalCreditor = totalCreditor,
                    Total = overallBalance,

                    LastTotalCreditor = lastTotalCreditor,
                    LastTotalDeptor = lastTotalDeptor,
                    LastTotal = lastOverallBalance,
                    AllTotal = lastOverallBalance + overallBalance
                };

                supplierViewModels.Add(supplierViewModel);
            }
            var model = new SupliersByDatesViewModel()
            {
                Suppliers = supplierViewModels,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = supplierViewModels.Sum(a => a.AllTotal)
            };


            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
                FileName = $"تقرير كشف حساب الموردين بين تاريخين.pdf"
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
            var model = new ScroungeByDatesViewModel()
            {
                Scrounges = scrounge,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = scrounge.Sum(a => a.Amount)
            };

            return PartialView("_AllScroungesByDatesPartial", model);
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

            var model = new ScroungeByDatesViewModel()
            {
                Scrounges = scrounge,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = scrounge.Sum(a => a.Amount)
            };

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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

            var model = new SalaryByDatesViewModel()
            {
                Salaries = salary,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = salary.Sum(a => a.SalaryAmount)
            };


            return PartialView("_AllSalaryByDatesPartial", model);
        }

        public IActionResult SalaryReport(DateTime startDate, DateTime endDate)
        {

            var salary = _unitOfWork.SalaryRepository
              .GetAll(a => a.Include(a => a.User).Include(a => a.Employee))
              .Where(a => a.SalaryDate.Date >= startDate.Date && a.SalaryDate.Date <= endDate.Date)
              .OrderByDescending(a => a.SalaryDate).ToList();

            var model = new SalaryByDatesViewModel()
            {
                Salaries = salary,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = salary.Sum(a => a.SalaryAmount)
            };

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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

            var model = new InvoicesByDatesViewModel()
            {
                Invoices = invoices,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = invoices.Sum(a => a.TotalPrice)
            };



            return PartialView("_AllInvoicesPartial", model);
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

            var model = new InvoicesByDatesViewModel()
            {
                Invoices = invoices,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = invoices.Sum(a => a.TotalPrice)
            };
            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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

            var model = new TayarsByDatesViewModel()
            {
                Tayars = orders,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = orders.Sum(a => a.TotalPrice)
            };

            return PartialView("_AllTayarOrdersPartial",model);
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

            var model = new TayarsByDatesViewModel()
            {
                Tayars = orders,
                FromDate = startDate.Date,
                ToDate = endDate.Date,
                PrintDate = DateTime.Now,
                Total = orders.Sum(a => a.TotalPrice)
            };


            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
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
