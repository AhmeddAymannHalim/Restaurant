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
    private readonly IBillSupportRepository _billSupport;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
        _billSupport = new BillSupportRepository(context);
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
            {
                var deposits = await _context.Deposits
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.CreatedTime >= from && a.CreatedTime < end)
                    .SumAsync(a => (decimal?)a.Amount) ?? 0;

                var sales = await _context.Orders
                    .AsNoTracking()
                    .Where(a => a.Cash && a.Bill.Date >= from && a.Bill.Date < end)
                    .Where(a => !a.OnAccount)
                    .SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

                var supplierReturns = await _context.BillMasters
                    .AsNoTracking()
                    .Where(a => a.BillSetting.BillType == BillType.PurchaseReturn && a.Date >= from && a.Date < end)
                    .SumAsync(a => (decimal?)a.InitialPaidAmount) ?? 0;

                var customerPayments = await _context.PaymentInvoiceCustomer
                    .AsNoTracking()
                    .Where(a => a.Date >= from && a.Date < end)
                    .SumAsync(a => (decimal?)a.Paid) ?? 0;

                var purchases = await _context.BillMasters
                    .AsNoTracking()
                    .Where(a => a.BillSetting.BillType == BillType.Purchases && a.Date >= from && a.Date < end)
                    .SumAsync(a => (decimal?)a.InitialPaidAmount) ?? 0;

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
                    new List<object> { "Deposits", deposits },
                    new List<object> { "Sales", sales },
                    new List<object> { "SupplierReturns", supplierReturns },
                    new List<object> { "CustomerPayments", customerPayments },
                    new List<object> { "Purchases", purchases },
                    new List<object> { "Expenses", expenses },
                    new List<object> { "EmployeeAdvances", advances },
                    new List<object> { "Salaries", salaries },
                    new List<object> { "SupplierPayments", supplierPayments }
                };

                var totalIn = deposits + sales + supplierReturns + customerPayments;
                var totalOut = purchases + expenses + advances + salaries + supplierPayments;

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "TotalIn", Value = totalIn },
                    new ReportTotal { Label = "TotalOut", Value = totalOut },
                    new ReportTotal { Label = "MoneySafeBalance", Value = totalIn - totalOut }
                };

                break;
            }

            case "Profit":
            {
                var lines = await _context.OrderMenuItems
                    .AsNoTracking()
                    .Where(a => a.Order.Cash
                        && a.Order.Bill.Date >= from
                        && a.Order.Bill.Date < end)
                    .Select(a => new
                    {
                        Price = a.BillDetail.TotalAmount ?? 0,
                        Cost = (a.BillDetail.UnitCostAtPosting ?? 0) * (a.BillDetail.Quantity ?? 0)
                    })
                    .ToListAsync();

                var profitOrders = await _context.Orders
                    .AsNoTracking()
                    .Where(a => a.Cash && a.Bill.Date >= from && a.Bill.Date < end)
                    .Select(a => new
                    {
                        Discount = a.Bill.BillDiscountValue,
                        a.Service
                    })
                    .ToListAsync();

                var itemsSales = lines.Sum(a => a.Price);
                var costOfGoods = lines.Sum(a => a.Cost);
                var discounts = profitOrders.Sum(a => a.Discount);
                var service = profitOrders.Sum(a => a.Service);
                var grossProfit = itemsSales - costOfGoods - discounts + service;

                var profitExpenses = await _context.ExitExpenses
                    .AsNoTracking()
                    .Where(a => a.Date >= from && a.Date < end)
                    .SumAsync(a => (decimal?)a.Price) ?? 0;

                var profitSalaries = await _context.Salaries
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
                    new List<object> { "ItemsSales", itemsSales },
                    new List<object> { "CostOfGoods", costOfGoods },
                    new List<object> { "Discounts", discounts },
                    new List<object> { "Service", service },
                    new List<object> { "GrossProfit", grossProfit },
                    new List<object> { "Expenses", profitExpenses },
                    new List<object> { "Salaries", profitSalaries }
                };

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "NetProfit", Value = grossProfit - profitExpenses - profitSalaries }
                };

                break;
            }

            case "Orders":
            case "Drivers":
            {
                int? driverId = request.Report == "Drivers" ? request.PartnerId ?? 0 : (int?)null;

                var query = _context.Orders
                    .AsNoTracking()
                    .Where(a => a.Cash && a.Bill.Date >= from && a.Bill.Date < end)
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
                        .Where(a => a.Job.ArName == "طيار")
                        .Select(a => new LookupItem
                        {
                            Id = a.Id,
                            ArName = a.ArName,
                            EnName = a.EnName
                        })
                        .ToListAsync();
                }

                var orders = await query
                    .Include(a => a.Bill)
                    .OrderByDescending(a => a.Bill.Date)
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
                    new ReportTotal { Label = "Count", Value = orders.Count },
                    new ReportTotal { Label = "TotalPrice", Value = orders.Sum(a => a.TotalPrice) }
                };

                break;
            }

            case "Invoices":
            {
                var billTypes = _billSupport.GetInvoiceBillTypes();
                var query = _context.BillMasters
                    .AsNoTracking()
                    .Where(a => billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id) && a.Date >= from && a.Date < end);

                if (Enum.TryParse<InvoiceType>(request.Type, out var invoiceType))
                {
                    var billType = _billSupport.GetInvoiceType(invoiceType).BillType;
                    query = query.Where(a => a.BillSetting.BillType == billType);
                }

                var isArabic = LanguageHelper.IsArabic();

                var invoices = await query
                    .OrderByDescending(a => a.Date)
                    .Select(a => new
                    {
                        a.Code,
                        a.Date,
                        a.BillSetting.BillType,
                        Partner = isArabic ? a.CustomerOrSupplierAccount.ArName : a.CustomerOrSupplierAccount.EnName,
                        Store = isArabic ? a.Store.ArName : a.Store.EnName,
                        Total = a.TotalBillAmountAfterDiscountAndExtra,
                        Paid = a.AmountPaided,
                        Remaining = a.CasherRemainingAmount
                    })
                    .ToListAsync();

                report.UsesType = true;
                report.Types = new List<string>
                {
                    "Purchases",
                    "Returns",
                    "Sales",
                    "SalesReturn"
                };

                report.Columns = new List<string>
                {
                    "InvoiceCode",
                    "InvoiceDate",
                    "InvoiceType",
                    "Partner",
                    "Store",
                    "NetTotal",
                    "Paid",
                    "Remaining"
                };

                foreach (var invoice in invoices)
                {
                    report.Rows.Add(new List<object>
                    {
                        invoice.Code,
                        invoice.Date,
                        _billSupport.TypeOf(invoice.BillType),
                        invoice.Partner,
                        invoice.Store,
                        invoice.Total,
                        invoice.Paid,
                        invoice.Remaining
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "Count", Value = invoices.Count },
                    new ReportTotal { Label = "NetTotal", Value = invoices.Sum(a => a.Total) },
                    new ReportTotal { Label = "Paid", Value = invoices.Sum(a => a.Paid) },
                    new ReportTotal { Label = "Remaining", Value = invoices.Sum(a => a.Remaining) }
                };

                break;
            }

            case "SupplierStatement":
            {
                var supplierId = request.PartnerId ?? 0;

                report.UsesPartner = true;
                report.PartnerLabel = "Supplier";
                report.Partners = await _context.Suppliers
                    .AsNoTracking()
                    .Include(a => a.Account)
                    .OrderBy(a => a.Account.ArName)
                    .Select(a => new LookupItem
                    {
                        Id = a.Id,
                        ArName = a.Account.ArName,
                        EnName = a.Account.EnName
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
                    .Include(a => a.Account)
                    .FirstOrDefaultAsync(a => a.Id == supplierId);

                if (supplier is null)
                {
                    break;
                }

                var statementAccountId = await _context.Suppliers.AsNoTracking()
                    .Where(a => a.Id == supplierId)
                    .Select(a => a.AccountId)
                    .FirstOrDefaultAsync();

                var beforeInvoiceRows = await _context.BillMasters
                    .AsNoTracking()
                    .Where(a => statementAccountId != null && a.CustomerOrSupplierAccountId == statementAccountId
                        && (a.BillSetting.BillType == BillType.Purchases || a.BillSetting.BillType == BillType.PurchaseReturn)
                        && a.IsEntryPosted
                        && !_context.Orders.Any(p => p.Id == a.Id))
                    .Where(a => a.Date < from)
                    .Select(a => new { a.BillSetting.BillType, Remaining = a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount })
                    .ToListAsync();

                decimal beforeInvoiceDebit = 0;
                decimal beforeInvoiceCredit = 0;
                foreach (var invoice in beforeInvoiceRows)
                {
                    if (invoice.BillType == BillType.Purchases)
                    {
                        beforeInvoiceDebit += invoice.Remaining;
                    }
                    else
                    {
                        beforeInvoiceCredit += invoice.Remaining;
                    }
                }

                var beforePaymentTotal = await _context.PaymentInvoiceSupplier
                    .AsNoTracking()
                    .Where(a => a.SupplierId == supplierId)
                    .Where(a => a.Date < from)
                    .SumAsync(a => (decimal?)a.Paid) ?? 0;

                var opening = (supplier.LastBalance ?? 0) + beforeInvoiceDebit - beforeInvoiceCredit - beforePaymentTotal;

                var inPeriodInvoiceRows = await _context.BillMasters
                    .AsNoTracking()
                    .Where(a => statementAccountId != null && a.CustomerOrSupplierAccountId == statementAccountId
                        && (a.BillSetting.BillType == BillType.Purchases || a.BillSetting.BillType == BillType.PurchaseReturn)
                        && a.IsEntryPosted
                        && !_context.Orders.Any(p => p.Id == a.Id))
                    .Where(a => a.Date >= from && a.Date < end)
                    .Select(a => new { a.Date, a.Code, a.BillSetting.BillType, Remaining = a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount })
                    .ToListAsync();

                var invoiceMovements = inPeriodInvoiceRows.Select(invoice =>
                {
                    string invoiceInfo;
                    decimal invoiceDebit = 0;
                    decimal invoiceCredit = 0;
                    if (invoice.BillType == BillType.Purchases)
                    {
                        invoiceInfo = "Purchases";
                        invoiceDebit = invoice.Remaining;
                    }
                    else
                    {
                        invoiceInfo = "Returns";
                        invoiceCredit = invoice.Remaining;
                    }

                    return new
                    {
                        Date = invoice.Date,
                        Info = invoiceInfo,
                        Code = invoice.Code,
                        Debit = invoiceDebit,
                        Credit = invoiceCredit
                    };
                }).ToList();

                var inPeriodPaymentRows = await _context.PaymentInvoiceSupplier
                    .AsNoTracking()
                    .Where(a => a.SupplierId == supplierId)
                    .Where(a => a.Date >= from && a.Date < end)
                    .ToListAsync();

                var paymentMovements = inPeriodPaymentRows.Select(payment => new
                {
                    Date = payment.Date,
                    Info = "SupplierPayments",
                    Code = payment.ProcessCode,
                    Debit = 0m,
                    Credit = payment.Paid
                }).ToList();

                var movements = invoiceMovements
                    .Concat(paymentMovements)
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
                    new ReportTotal { Label = "OpeningBalance", Value = opening },
                    new ReportTotal { Label = "Debit", Value = movements.Sum(a => a.Debit) },
                    new ReportTotal { Label = "Credit", Value = movements.Sum(a => a.Credit) },
                    new ReportTotal { Label = "ClosingBalance", Value = balance }
                };

                break;
            }

            case "CustomerStatement":
            {
                var customerId = request.PartnerId ?? 0;

                report.UsesPartner = true;
                report.PartnerLabel = "Customer";
                report.Partners = await _context.Customers
                    .AsNoTracking()
                    .Include(a => a.Account)
                    .OrderBy(a => a.Account.ArName)
                    .Select(a => new LookupItem
                    {
                        Id = a.Id,
                        ArName = a.Account.ArName,
                        EnName = a.Account.EnName
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
                    .Include(a => a.Account)
                    .FirstOrDefaultAsync(a => a.Id == customerId);

                if (customer is null)
                {
                    break;
                }

                var custOrders = await _context.Orders
                    .AsNoTracking()
                    .Where(a => a.OnAccount && a.CustomerId == customerId && a.Bill.Date >= from)
                    .Select(a => new
                    {
                        Date = a.Bill.Date,
                        Info = "Orders",
                        Code = a.OrderCode,
                        Debit = a.Bill.TotalBillAmountAfterDiscountAndExtra,
                        Credit = 0m
                    })
                    .ToListAsync();

                // back office sales invoices move the balance too: what was not paid at once
                var custInvoices = await _context.BillMasters
                    .AsNoTracking()
                    .Where(a => customer.AccountId != null && a.CustomerOrSupplierAccountId == customer.AccountId
                        && a.IsEntryPosted
                        && (a.BillSetting.BillType == BillType.Sales || a.BillSetting.BillType == BillType.SalesReturn)
                        && !_context.Orders.Any(p => p.Id == a.Id)
                        && a.Date >= from)
                    .Select(a => new
                    {
                        Date = a.Date,
                        Info = a.BillSetting.BillType == BillType.Sales ? "Sales" : "SalesReturn",
                        Code = a.Code,
                        Debit = a.BillSetting.BillType == BillType.Sales ? a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount : 0m,
                        Credit = a.BillSetting.BillType == BillType.SalesReturn ? a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount : 0m
                    })
                    .ToListAsync();

                var custPayments = await _context.PaymentInvoiceCustomer
                    .AsNoTracking()
                    .Where(a => a.CustomerId == customerId && a.Date >= from)
                    .Select(a => new
                    {
                        Date = a.Date,
                        Info = "CustomerPayments",
                        Code = a.ProcessCode,
                        Debit = 0m,
                        Credit = a.Paid
                    })
                    .ToListAsync();

                var custRefunds = await _context.PaymentRefunds
                    .AsNoTracking()
                    .Where(a => a.OriginalPayment.CustomerId == customerId
                        && a.Status == RefundStatus.Approved
                        && a.ApprovedAt >= from)
                    .Select(a => new
                    {
                        Date = a.ApprovedAt.Value,
                        Info = "PaymentRefund",
                        Code = a.OriginalPayment.ProcessCode,
                        Debit = a.Amount,
                        Credit = 0m
                    })
                    .ToListAsync();

                var all = custOrders.Concat(custInvoices).Concat(custPayments).Concat(custRefunds).ToList();
                var custOpening = customer.Account.BalanceValue - all.Sum(a => a.Debit - a.Credit);
                var custInPeriod = all
                    .Where(a => a.Date < end)
                    .OrderBy(a => a.Date)
                    .ToList();

                var custBalance = custOpening;

                report.Rows.Add(new List<object>
                {
                    from,
                    "OpeningBalance",
                    null,
                    null,
                    custOpening
                });

                foreach (var movement in custInPeriod)
                {
                    custBalance += movement.Debit - movement.Credit;

                    report.Rows.Add(new List<object>
                    {
                        movement.Date,
                        movement.Info + " " + movement.Code,
                        movement.Debit == 0 ? null : movement.Debit,
                        movement.Credit == 0 ? null : movement.Credit,
                        custBalance
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "OpeningBalance", Value = custOpening },
                    new ReportTotal { Label = "Debit", Value = custInPeriod.Sum(a => a.Debit) },
                    new ReportTotal { Label = "Credit", Value = custInPeriod.Sum(a => a.Credit) },
                    new ReportTotal { Label = "ClosingBalance", Value = custBalance }
                };

                break;
            }

            case "SuppliersBalances":
            {
                var isArabic = LanguageHelper.IsArabic();

                var suppliers = await _context.Suppliers
                    .AsNoTracking()
                    .Include(a => a.Account)
                    .OrderBy(a => a.Account.ArName)
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
                    var balanceAccountId = await _context.Suppliers.AsNoTracking()
                        .Where(a => a.Id == supplier.Id)
                        .Select(a => a.AccountId)
                        .FirstOrDefaultAsync();

                    var beforeInvoiceRows = await _context.BillMasters
                        .AsNoTracking()
                        .Where(a => balanceAccountId != null && a.CustomerOrSupplierAccountId == balanceAccountId
                            && (a.BillSetting.BillType == BillType.Purchases || a.BillSetting.BillType == BillType.PurchaseReturn)
                            && a.IsEntryPosted
                            && !_context.Orders.Any(p => p.Id == a.Id))
                        .Where(a => a.Date < from)
                        .Select(a => new { a.BillSetting.BillType, Remaining = a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount })
                        .ToListAsync();

                    decimal beforeInvoiceDebit = 0;
                    decimal beforeInvoiceCredit = 0;
                    foreach (var invoice in beforeInvoiceRows)
                    {
                        if (invoice.BillType == BillType.Purchases)
                        {
                            beforeInvoiceDebit += invoice.Remaining;
                        }
                        else
                        {
                            beforeInvoiceCredit += invoice.Remaining;
                        }
                    }

                    var beforePaymentTotal = await _context.PaymentInvoiceSupplier
                        .AsNoTracking()
                        .Where(a => a.SupplierId == supplier.Id)
                        .Where(a => a.Date < from)
                        .SumAsync(a => (decimal?)a.Paid) ?? 0;

                    var inPeriodInvoiceRows = await _context.BillMasters
                        .AsNoTracking()
                        .Where(a => balanceAccountId != null && a.CustomerOrSupplierAccountId == balanceAccountId
                            && (a.BillSetting.BillType == BillType.Purchases || a.BillSetting.BillType == BillType.PurchaseReturn)
                            && a.IsEntryPosted
                            && !_context.Orders.Any(p => p.Id == a.Id))
                        .Where(a => a.Date >= from && a.Date < end)
                        .Select(a => new { a.BillSetting.BillType, Remaining = a.TotalBillAmountAfterDiscountAndExtra - a.InitialPaidAmount })
                        .ToListAsync();

                    decimal inPeriodInvoiceDebit = 0;
                    decimal inPeriodInvoiceCredit = 0;
                    foreach (var invoice in inPeriodInvoiceRows)
                    {
                        if (invoice.BillType == BillType.Purchases)
                        {
                            inPeriodInvoiceDebit += invoice.Remaining;
                        }
                        else
                        {
                            inPeriodInvoiceCredit += invoice.Remaining;
                        }
                    }

                    var inPeriodPaymentTotal = await _context.PaymentInvoiceSupplier
                        .AsNoTracking()
                        .Where(a => a.SupplierId == supplier.Id)
                        .Where(a => a.Date >= from && a.Date < end)
                        .SumAsync(a => (decimal?)a.Paid) ?? 0;

                    var opening = (supplier.LastBalance ?? 0) + beforeInvoiceDebit - beforeInvoiceCredit - beforePaymentTotal;
                    var debit = inPeriodInvoiceDebit;
                    var credit = inPeriodInvoiceCredit + inPeriodPaymentTotal;

                    report.Rows.Add(new List<object>
                    {
                        isArabic ? supplier.Account.ArName : supplier.Account.EnName,
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
                    new ReportTotal { Label = "OpeningBalance", Value = totalOpening },
                    new ReportTotal { Label = "Debit", Value = totalDebit },
                    new ReportTotal { Label = "Credit", Value = totalCredit },
                    new ReportTotal { Label = "ClosingBalance", Value = totalOpening + totalDebit - totalCredit }
                };

                break;
            }

            case "SupplierPayments":
            {
                var isArabic = LanguageHelper.IsArabic();

                var payments = await _context.PaymentInvoiceSupplier
                    .AsNoTracking()
                    .Include(a => a.Supplier).ThenInclude(a => a.Account)
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
                        isArabic ? payment.Supplier.Account.ArName : payment.Supplier.Account.EnName,
                        payment.Description,
                        payment.Paid
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "Count", Value = payments.Count },
                    new ReportTotal { Label = "Paid", Value = payments.Sum(a => a.Paid) }
                };

                break;
            }

            case "CustomerPayments":
            {
                var isArabic = LanguageHelper.IsArabic();

                var payments = await _context.PaymentInvoiceCustomer
                    .AsNoTracking()
                    .Include(a => a.Customer).ThenInclude(a => a.Account)
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
                        isArabic ? payment.Customer.Account.ArName : payment.Customer.Account.EnName,
                        payment.Description,
                        payment.Paid
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "Count", Value = payments.Count },
                    new ReportTotal { Label = "Paid", Value = payments.Sum(a => a.Paid) }
                };

                break;
            }

            case "Deposits":
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
                    new ReportTotal { Label = "Count", Value = deposits.Count },
                    new ReportTotal { Label = "Amount", Value = deposits.Sum(a => a.Amount) }
                };

                break;
            }

            case "Expenses":
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
                    new ReportTotal { Label = "Count", Value = expenses.Count },
                    new ReportTotal { Label = "Price", Value = expenses.Sum(a => a.Price) }
                };

                break;
            }

            case "Salaries":
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
                        (LanguageHelper.IsArabic() ? salary.Employee.ArName : salary.Employee.EnName),
                        salary.SalaryAmount
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "Count", Value = salaries.Count },
                    new ReportTotal { Label = "SalaryAmount", Value = salaries.Sum(a => a.SalaryAmount) }
                };

                break;
            }

            case "Scrounges":
            {
                var query = _context.Scrounges
                    .AsNoTracking()
                    .Include(a => a.Employee)
                    .Where(a => a.Date >= from && a.Date < end);

                if (Enum.TryParse<MoneyState>(request.Type, out var state))
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
                        (LanguageHelper.IsArabic() ? scrounge.Employee.ArName : scrounge.Employee.EnName),
                        scrounge.State,
                        scrounge.Info,
                        scrounge.Amount
                    });
                }

                report.Totals = new List<ReportTotal>
                {
                    new ReportTotal { Label = "Count", Value = scrounges.Count },
                    new ReportTotal { Label = "Amount", Value = scrounges.Sum(a => a.Amount) }
                };

                break;
            }
        }

        return report;
    }

}
