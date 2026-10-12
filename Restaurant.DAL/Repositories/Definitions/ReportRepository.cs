using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportViewModel> BuildAsync(ReportRequest request)
    {
        var from = (request.FromDate ?? DefaultDateRange.Start).Date;
        var to = (request.ToDate ?? DefaultDateRange.End).Date;

        if (to < from)
        {
            to = from;
        }

        var report = new ReportViewModel
        {
            Report = request.Report,
            FromDate = from,
            ToDate = to,
            PartnerId = request.PartnerId,
            Type = request.Type
        };

        var end = to.AddDays(1);

        switch (request.Report)
        {
            case "MoneySafe":
                await BuildMoneySafeAsync(report, from, end);
                break;

            case "Profit":
                await BuildProfitAsync(report, from, end);
                break;

            case "Orders":
                await BuildOrdersAsync(report, from, end, null);
                break;

            case "Drivers":
                await BuildOrdersAsync(report, from, end, request.PartnerId ?? 0);
                break;

            case "Invoices":
                await BuildInvoicesAsync(report, from, end, request.Type);
                break;

            case "SupplierStatement":
                await BuildSupplierStatementAsync(report, from, end, request.PartnerId ?? 0);
                break;

            case "CustomerStatement":
                await BuildCustomerStatementAsync(report, from, end, request.PartnerId ?? 0);
                break;

            case "SuppliersBalances":
                await BuildSuppliersBalancesAsync(report, from, end);
                break;

            case "SupplierPayments":
                await BuildSupplierPaymentsAsync(report, from, end);
                break;

            case "CustomerPayments":
                await BuildCustomerPaymentsAsync(report, from, end);
                break;

            case "Deposits":
                await BuildDepositsAsync(report, from, end);
                break;

            case "Expenses":
                await BuildExpensesAsync(report, from, end);
                break;

            case "Salaries":
                await BuildSalariesAsync(report, from, end);
                break;

            case "Scrounges":
                await BuildScroungesAsync(report, from, end, request.Type);
                break;
        }

        return report;
    }

    private async Task BuildMoneySafeAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var deposits = await _context.Deposits
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.CreatedTime >= from && a.CreatedTime < end)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var sales = await ClosedOrders(from, end)
            .Where(a => !a.OnAccount)
            .SumAsync(a => (decimal?)a.TotalPrice) ?? 0;

        var supplierReturns = await _context.Invoices
            .AsNoTracking()
            .Where(a => a.InvoiceType == InvoiceType.Returns && a.InvoiceDate >= from && a.InvoiceDate < end)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        var customerPayments = await _context.PaymentInvoiceCustomer
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date < end)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        var purchases = await _context.Invoices
            .AsNoTracking()
            .Where(a => a.InvoiceType == InvoiceType.Purchases && a.InvoiceDate >= from && a.InvoiceDate < end)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        var expenses = await _context.ExitExpenses
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date < end)
            .SumAsync(a => (decimal?)a.Price) ?? 0;

        var advances = await _context.Scrounges
            .AsNoTracking()
            .Where(a => a.State == MoneyState.Dept && a.Date >= from && a.Date < end)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var salaries = await _context.Salaries
            .AsNoTracking()
            .Where(a => a.SalaryDate >= from && a.SalaryDate < end)
            .SumAsync(a => (decimal?)a.SalaryAmount) ?? 0;

        var supplierPayments = await _context.PaymentInvoiceSupplier
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date < end)
            .SumAsync(a => (decimal?)a.Paid) ?? 0;

        report.Columns = new List<string>
        {
            "Item",
            "Amount"
        };

        report.Rows = new List<List<object>>
        {
            Row("Deposits", deposits),
            Row("Sales", sales),
            Row("SupplierReturns", supplierReturns),
            Row("CustomerPayments", customerPayments),
            Row("Purchases", purchases),
            Row("Expenses", expenses),
            Row("EmployeeAdvances", advances),
            Row("Salaries", salaries),
            Row("SupplierPayments", supplierPayments)
        };

        var totalIn = deposits + sales + supplierReturns + customerPayments;
        var totalOut = purchases + expenses + advances + salaries + supplierPayments;

        report.Totals = new List<ReportTotal>
        {
            Total("TotalIn", totalIn),
            Total("TotalOut", totalOut),
            Total("MoneySafeBalance", totalIn - totalOut)
        };
    }

    private async Task BuildProfitAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var lines = await _context.OrderMenuItems
            .AsNoTracking()
            .Where(a => a.Order.Cash
                && a.Order.FinishShift
                && a.Order.OrderDate >= from
                && a.Order.OrderDate < end)
            .Select(a => new
            {
                a.Price,
                Cost = a.UnitCost * a.Quantity
            })
            .ToListAsync();

        var orders = await ClosedOrders(from, end)
            .Select(a => new
            {
                a.Discount,
                a.Service
            })
            .ToListAsync();

        var itemsSales = lines.Sum(a => a.Price);
        var costOfGoods = lines.Sum(a => a.Cost);
        var discounts = orders.Sum(a => a.Discount);
        var service = orders.Sum(a => a.Service);
        var grossProfit = itemsSales - costOfGoods - discounts + service;

        var expenses = await _context.ExitExpenses
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date < end)
            .SumAsync(a => (decimal?)a.Price) ?? 0;

        var salaries = await _context.Salaries
            .AsNoTracking()
            .Where(a => a.SalaryDate >= from && a.SalaryDate < end)
            .SumAsync(a => (decimal?)a.SalaryAmount) ?? 0;

        report.Columns = new List<string>
        {
            "Item",
            "Amount"
        };

        report.Rows = new List<List<object>>
        {
            Row("ItemsSales", itemsSales),
            Row("CostOfGoods", costOfGoods),
            Row("Discounts", discounts),
            Row("Service", service),
            Row("GrossProfit", grossProfit),
            Row("Expenses", expenses),
            Row("Salaries", salaries)
        };

        report.Totals = new List<ReportTotal>
        {
            Total("NetProfit", grossProfit - expenses - salaries)
        };
    }

    private async Task BuildOrdersAsync(ReportViewModel report, DateTime from, DateTime end, int? driverId)
    {
        var query = ClosedOrders(from, end)
            .Include(a => a.Seller)
            .Include(a => a.Employee)
            .AsQueryable();

        if (driverId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == driverId.Value);

            report.UsesPartner = true;
            report.PartnerLabel = "DeliveryDriver";
            report.Partners = await _context.Employees
                .AsNoTracking()
                .Where(a => a.UserType == "طيار")
                .Select(a => new LookupItem
                {
                    Id = a.EmployeeId,
                    ArName = a.FirstName + " " + a.LastName,
                    EnName = a.FirstName + " " + a.LastName
                })
                .ToListAsync();
        }

        var orders = await query
            .OrderByDescending(a => a.OrderDate)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "OrderCode",
            "InvoiceDate",
            "OrderType",
            "CustomerName",
            "Seller",
            "Service",
            "Tax",
            "Discount",
            "TotalPrice",
            "PaymentStatus"
        };

        foreach (var order in orders)
        {
            report.Rows.Add(new List<object>
            {
                order.OrderCode,
                order.OrderDate,
                order.Type,
                order.CustomerName,
                order.Seller?.FullName,
                order.Service,
                order.Tax,
                order.Discount,
                order.TotalPrice,
                order.OnAccount ? "OnAccount" : "PaidStatus"
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", orders.Count),
            Total("TotalPrice", orders.Sum(a => a.TotalPrice))
        };
    }

    private async Task BuildInvoicesAsync(ReportViewModel report, DateTime from, DateTime end, string type)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(a => a.Supplier)
            .Include(a => a.Store)
            .Where(a => a.InvoiceDate >= from && a.InvoiceDate < end);

        if (Enum.TryParse<InvoiceType>(type, out var invoiceType))
        {
            query = query.Where(a => a.InvoiceType == invoiceType);
        }

        var invoices = await query
            .OrderByDescending(a => a.InvoiceDate)
            .ToListAsync();

        var isArabic = LanguageHelper.IsArabic();

        report.UsesType = true;
        report.Types = new List<string>
        {
            "Purchases",
            "Returns"
        };

        report.Columns = new List<string>
        {
            "InvoiceCode",
            "InvoiceDate",
            "InvoiceType",
            "Supplier",
            "Store",
            "NetTotal",
            "Paid",
            "Remaining"
        };

        foreach (var invoice in invoices)
        {
            report.Rows.Add(new List<object>
            {
                invoice.InvoiceCode,
                invoice.InvoiceDate,
                invoice.InvoiceType,
                isArabic ? invoice.Supplier.ArName : invoice.Supplier.EnName,
                isArabic ? invoice.Store.ArName : invoice.Store.EnName,
                invoice.TotalPrice,
                invoice.Paid,
                invoice.Remaining
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", invoices.Count),
            Total("NetTotal", invoices.Sum(a => a.TotalPrice)),
            Total("Paid", invoices.Sum(a => a.Paid)),
            Total("Remaining", invoices.Sum(a => a.Remaining))
        };
    }

    private async Task BuildSupplierStatementAsync(ReportViewModel report, DateTime from, DateTime end, int supplierId)
    {
        report.UsesPartner = true;
        report.PartnerLabel = "Supplier";
        report.Partners = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .ToListAsync();

        report.Columns = new List<string>
        {
            "InvoiceDate",
            "Description",
            "Debit",
            "Credit",
            "Balance"
        };

        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == supplierId);

        if (supplier is null)
        {
            return;
        }

        var before = await GetSupplierMovementsAsync(supplierId, null, from);
        var opening = (supplier.LastBalance ?? 0) + before.Sum(a => a.Debit - a.Credit);

        var movements = (await GetSupplierMovementsAsync(supplierId, from, end))
            .OrderBy(a => a.Date)
            .ToList();

        var balance = opening;

        report.Rows.Add(new List<object>
        {
            from,
            "OpeningBalance",
            null,
            null,
            opening
        });

        foreach (var movement in movements)
        {
            balance += movement.Debit - movement.Credit;

            report.Rows.Add(new List<object>
            {
                movement.Date,
                movement.Info + " " + movement.Code,
                movement.Debit == 0 ? null : movement.Debit,
                movement.Credit == 0 ? null : movement.Credit,
                balance
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("OpeningBalance", opening),
            Total("Debit", movements.Sum(a => a.Debit)),
            Total("Credit", movements.Sum(a => a.Credit)),
            Total("ClosingBalance", balance)
        };
    }

    private async Task BuildCustomerStatementAsync(ReportViewModel report, DateTime from, DateTime end, int customerId)
    {
        report.UsesPartner = true;
        report.PartnerLabel = "Customer";
        report.Partners = await _context.Customers
            .AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .ToListAsync();

        report.Columns = new List<string>
        {
            "InvoiceDate",
            "Description",
            "Debit",
            "Credit",
            "Balance"
        };

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == customerId);

        if (customer is null)
        {
            return;
        }

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(a => a.OnAccount && a.CustomerId == customerId && a.OrderDate >= from)
            .Select(a => new CustomerMovement
            {
                Date = a.OrderDate,
                Info = "Orders",
                Code = a.OrderCode,
                Debit = a.TotalPrice,
                Credit = 0
            })
            .ToListAsync();

        var payments = await _context.PaymentInvoiceCustomer
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId && a.Date >= from)
            .Select(a => new CustomerMovement
            {
                Date = a.Date,
                Info = "CustomerPayments",
                Code = a.ProcessCode,
                Debit = 0,
                Credit = a.Paid
            })
            .ToListAsync();

        var refunds = await _context.PaymentRefunds
            .AsNoTracking()
            .Where(a => a.OriginalPayment.CustomerId == customerId
                && a.Status == RefundStatus.Approved
                && a.ApprovedAt >= from)
            .Select(a => new CustomerMovement
            {
                Date = a.ApprovedAt.Value,
                Info = "PaymentRefund",
                Code = a.OriginalPayment.ProcessCode,
                Debit = a.Amount,
                Credit = 0
            })
            .ToListAsync();

        var all = orders.Concat(payments).Concat(refunds).ToList();
        var opening = customer.Balance - all.Sum(a => a.Debit - a.Credit);
        var inPeriod = all
            .Where(a => a.Date < end)
            .OrderBy(a => a.Date)
            .ToList();

        var balance = opening;

        report.Rows.Add(new List<object>
        {
            from,
            "OpeningBalance",
            null,
            null,
            opening
        });

        foreach (var movement in inPeriod)
        {
            balance += movement.Debit - movement.Credit;

            report.Rows.Add(new List<object>
            {
                movement.Date,
                movement.Info + " " + movement.Code,
                movement.Debit == 0 ? null : movement.Debit,
                movement.Credit == 0 ? null : movement.Credit,
                balance
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("OpeningBalance", opening),
            Total("Debit", inPeriod.Sum(a => a.Debit)),
            Total("Credit", inPeriod.Sum(a => a.Credit)),
            Total("ClosingBalance", balance)
        };
    }

    private async Task BuildSuppliersBalancesAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var isArabic = LanguageHelper.IsArabic();

        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(a => a.ArName)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "Supplier",
            "OpeningBalance",
            "Debit",
            "Credit",
            "ClosingBalance"
        };

        decimal totalOpening = 0;
        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var supplier in suppliers)
        {
            var before = await GetSupplierMovementsAsync(supplier.Id, null, from);
            var inPeriod = await GetSupplierMovementsAsync(supplier.Id, from, end);

            var opening = (supplier.LastBalance ?? 0) + before.Sum(a => a.Debit - a.Credit);
            var debit = inPeriod.Sum(a => a.Debit);
            var credit = inPeriod.Sum(a => a.Credit);

            report.Rows.Add(new List<object>
            {
                isArabic ? supplier.ArName : supplier.EnName,
                opening,
                debit,
                credit,
                opening + debit - credit
            });

            totalOpening += opening;
            totalDebit += debit;
            totalCredit += credit;
        }

        report.Totals = new List<ReportTotal>
        {
            Total("OpeningBalance", totalOpening),
            Total("Debit", totalDebit),
            Total("Credit", totalCredit),
            Total("ClosingBalance", totalOpening + totalDebit - totalCredit)
        };
    }

    private async Task<List<SupplierMovement>> GetSupplierMovementsAsync(int supplierId, DateTime? from, DateTime? until)
    {
        var invoices = _context.Invoices
            .AsNoTracking()
            .Where(a => a.SupplierId == supplierId);

        var payments = _context.PaymentInvoiceSupplier
            .AsNoTracking()
            .Where(a => a.SupplierId == supplierId);

        if (from.HasValue)
        {
            invoices = invoices.Where(a => a.InvoiceDate >= from.Value);
            payments = payments.Where(a => a.Date >= from.Value);
        }

        if (until.HasValue)
        {
            invoices = invoices.Where(a => a.InvoiceDate < until.Value);
            payments = payments.Where(a => a.Date < until.Value);
        }

        var movements = new List<SupplierMovement>();

        foreach (var invoice in await invoices.ToListAsync())
        {
            var isPurchase = invoice.InvoiceType == InvoiceType.Purchases;

            movements.Add(new SupplierMovement
            {
                Date = invoice.InvoiceDate,
                Info = isPurchase ? "Purchases" : "Returns",
                Code = invoice.InvoiceCode,
                Debit = isPurchase ? invoice.Remaining : 0,
                Credit = isPurchase ? 0 : invoice.Remaining
            });
        }

        foreach (var payment in await payments.ToListAsync())
        {
            movements.Add(new SupplierMovement
            {
                Date = payment.Date,
                Info = "SupplierPayments",
                Code = payment.ProcessCode,
                Debit = 0,
                Credit = payment.Paid
            });
        }

        return movements;
    }

    private async Task BuildSupplierPaymentsAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var isArabic = LanguageHelper.IsArabic();

        var payments = await _context.PaymentInvoiceSupplier
            .AsNoTracking()
            .Include(a => a.Supplier)
            .Where(a => a.Date >= from && a.Date < end)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "ProcessCode",
            "InvoiceDate",
            "Supplier",
            "Description",
            "Paid"
        };

        foreach (var payment in payments)
        {
            report.Rows.Add(new List<object>
            {
                payment.ProcessCode,
                payment.Date,
                isArabic ? payment.Supplier.ArName : payment.Supplier.EnName,
                payment.Description,
                payment.Paid
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", payments.Count),
            Total("Paid", payments.Sum(a => a.Paid))
        };
    }

    private async Task BuildCustomerPaymentsAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var isArabic = LanguageHelper.IsArabic();

        var payments = await _context.PaymentInvoiceCustomer
            .AsNoTracking()
            .Include(a => a.Customer)
            .Where(a => a.Date >= from && a.Date < end)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "ProcessCode",
            "InvoiceDate",
            "Customer",
            "Description",
            "Paid"
        };

        foreach (var payment in payments)
        {
            report.Rows.Add(new List<object>
            {
                payment.ProcessCode,
                payment.Date,
                isArabic ? payment.Customer.ArName : payment.Customer.EnName,
                payment.Description,
                payment.Paid
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", payments.Count),
            Total("Paid", payments.Sum(a => a.Paid))
        };
    }

    private async Task BuildDepositsAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var deposits = await _context.Deposits
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.CreatedTime >= from && a.CreatedTime < end)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "ProcessCode",
            "InvoiceDate",
            "DeposerName",
            "Description",
            "Amount"
        };

        foreach (var deposit in deposits)
        {
            report.Rows.Add(new List<object>
            {
                deposit.ProcessCode,
                deposit.CreatedTime,
                deposit.DeposerName,
                deposit.Description,
                deposit.Amount
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", deposits.Count),
            Total("Amount", deposits.Sum(a => a.Amount))
        };
    }

    private async Task BuildExpensesAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var isArabic = LanguageHelper.IsArabic();

        var expenses = await _context.ExitExpenses
            .AsNoTracking()
            .Include(a => a.Expense)
            .Include(a => a.User)
            .Where(a => a.Date >= from && a.Date < end)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "InvoiceDate",
            "Expense",
            "UserName",
            "Price"
        };

        foreach (var expense in expenses)
        {
            report.Rows.Add(new List<object>
            {
                expense.Date,
                isArabic ? expense.Expense.ArName : expense.Expense.EnName,
                expense.User?.FullName,
                expense.Price
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", expenses.Count),
            Total("Price", expenses.Sum(a => a.Price))
        };
    }

    private async Task BuildSalariesAsync(ReportViewModel report, DateTime from, DateTime end)
    {
        var salaries = await _context.Salaries
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.SalaryDate >= from && a.SalaryDate < end)
            .OrderByDescending(a => a.SalaryDate)
            .ToListAsync();

        report.Columns = new List<string>
        {
            "ProcessCode",
            "InvoiceDate",
            "Employee",
            "SalaryAmount"
        };

        foreach (var salary in salaries)
        {
            report.Rows.Add(new List<object>
            {
                salary.SalaryCode,
                salary.SalaryDate,
                salary.Employee.FullName,
                salary.SalaryAmount
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", salaries.Count),
            Total("SalaryAmount", salaries.Sum(a => a.SalaryAmount))
        };
    }

    private async Task BuildScroungesAsync(ReportViewModel report, DateTime from, DateTime end, string type)
    {
        var query = _context.Scrounges
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.Date >= from && a.Date < end);

        if (Enum.TryParse<MoneyState>(type, out var state))
        {
            query = query.Where(a => a.State == state);
        }

        var scrounges = await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        report.UsesType = true;
        report.Types = Enum.GetNames<MoneyState>().ToList();

        report.Columns = new List<string>
        {
            "InvoiceDate",
            "Employee",
            "Type",
            "Description",
            "Amount"
        };

        foreach (var scrounge in scrounges)
        {
            report.Rows.Add(new List<object>
            {
                scrounge.Date,
                scrounge.Employee.FullName,
                scrounge.State,
                scrounge.Info,
                scrounge.Amount
            });
        }

        report.Totals = new List<ReportTotal>
        {
            Total("Count", scrounges.Count),
            Total("Amount", scrounges.Sum(a => a.Amount))
        };
    }

    private IQueryable<Order> ClosedOrders(DateTime from, DateTime end)
    {
        return _context.Orders
            .AsNoTracking()
            .Where(a => a.Cash
                && a.FinishShift
                && a.OrderDate >= from
                && a.OrderDate < end);
    }

    private static List<object> Row(string label, decimal amount)
    {
        return new List<object>
        {
            label,
            amount
        };
    }

    private static ReportTotal Total(string label, decimal value)
    {
        return new ReportTotal
        {
            Label = label,
            Value = value
        };
    }

    private class CustomerMovement
    {
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public string Code { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    private class SupplierMovement
    {
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public string Code { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
