using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class CustomerPaymentRepository : ICustomerPaymentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public CustomerPaymentRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<PaymentInvoiceCustomer>> GetPagedPaymentsAsync(PageRequest request)
    {
        var query = _context.PaymentInvoiceCustomer.AsNoTracking().Include(a => a.Customer).ThenInclude(a => a.Account).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ProcessCode.ToLower().Contains(search)
                || a.Description.ToLower().Contains(search)
                || a.Customer.Account.ArName.ToLower().Contains(search)
                || a.Customer.Account.EnName.ToLower().Contains(search));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Date < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<PaymentInvoiceCustomer>.Create(data, total, request);
    }

    public async Task<PaymentInvoiceCustomer> GetPaymentByIdAsync(int id)
    {
        return await _context.PaymentInvoiceCustomer.AsNoTracking()
            .Include(a => a.Customer).ThenInclude(a => a.Account)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var last = await _context.PaymentInvoiceCustomer.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.ProcessCode)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
    }

    public async Task<decimal> GetBalanceAsync(int partnerId)
    {
        var account = await _context.Customers.AsNoTracking()
            .Where(a => a.Id == partnerId)
            .Select(a => a.Account)
            .FirstOrDefaultAsync();

        return account?.BalanceValue ?? 0;
    }

    public async Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceCustomer entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var partner = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == entity.CustomerId);
            if (partner is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            PaymentInvoiceCustomer payment;
            if (entity.Id == 0)
            {
                payment = entity;
                payment.ProcessCode = await GetNextCodeAsync();
                payment.Date = DateTime.Now;
                await _context.PaymentInvoiceCustomer.AddAsync(payment);
            }
            else
            {
                payment = await _context.PaymentInvoiceCustomer.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (payment is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                var before = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.CustomerId);
                before.Account.Adjust(payment.Paid);

                payment.CustomerId = entity.CustomerId;
                payment.Paid = entity.Paid;
                payment.Description = entity.Description;
            }

            partner.Account.Adjust(-payment.Paid);
            await _context.SaveChangesAsync();

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.CustomerPaymentVoucher, payment.Id);
            var posting = await _journals.PostSourceEntryAsync(JournalSourceType.CustomerPaymentVoucher,
                payment.Id,
                payment.Date,
                $"Customer receipt {payment.ProcessCode}",
                new[]
                {
                    new JournalLineRequest(AccountCodes.Cash, payment.Paid, 0),
                    new JournalLineRequest(AccountCodes.AccountsReceivable, 0, payment.Paid)
                },
                saveChanges: false);
            if (!posting.Success)
            {
                return posting;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage
            {
                Success = true,
                Data = payment.ProcessCode
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeletePaymentAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var payment = await _context.PaymentInvoiceCustomer.FirstOrDefaultAsync(a => a.Id == id);
            if (payment is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.PaymentRefunds.AnyAsync(a => a.OriginalPaymentId == id))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeletePaymentWithRefunds"
                };
            }

            var partner = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.CustomerId);
            partner.Account.Adjust(payment.Paid);

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.CustomerPaymentVoucher, payment.Id);

            _context.PaymentInvoiceCustomer.Remove(payment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<IEnumerable<PaymentRefund>> GetRefundsForPaymentAsync(int paymentId)
    {
        return await _context.PaymentRefunds.AsNoTracking()
            .Include(a => a.OriginalPayment)
            .Include(a => a.RequestedByUser)
            .Include(a => a.ApprovedByUser)
            .Where(a => a.OriginalPaymentId == paymentId)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    private async Task<decimal> GetRemainingRefundableAsync(int paymentId, int excludeRefundId = 0)
    {
        var payment = await _context.PaymentInvoiceCustomer.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == paymentId);
        if (payment is null)
        {
            return 0;
        }

        var approved = await _context.PaymentRefunds.AsNoTracking()
            .Where(a => a.OriginalPaymentId == paymentId
                && a.Status == RefundStatus.Approved
                && a.Id != excludeRefundId)
            .SumAsync(a => a.Amount);

        return payment.Paid - approved;
    }

    public async Task<ResultMessage> RequestRefundAsync(PaymentRefund entity, string requestedByUserId)
    {
        try
        {
            var payment = await _context.PaymentInvoiceCustomer.FirstOrDefaultAsync(a => a.Id == entity.OriginalPaymentId);
            if (payment is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            if (entity.Amount <= 0)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundAmountMustBePositive"
                };
            }

            var pendingAndApproved = await _context.PaymentRefunds.AsNoTracking()
                .Where(a => a.OriginalPaymentId == entity.OriginalPaymentId && a.Status != RefundStatus.Rejected)
                .SumAsync(a => a.Amount);

            if (entity.Amount > payment.Paid - pendingAndApproved)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundExceedsRemainingAmount"
                };
            }

            var refund = new PaymentRefund
            {
                OriginalPaymentId = entity.OriginalPaymentId,
                Amount = entity.Amount,
                Reason = entity.Reason,
                RequestedByUserId = requestedByUserId,
                Status = RefundStatus.Pending
            };

            await _context.PaymentRefunds.AddAsync(refund);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId)
    {
        try
        {
            var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(a => a.Id == refundId && a.OriginalPaymentId != null);
            if (refund is null)
            {
                return new ResultMessage { Success = true };
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundAlreadyProcessed"
                };
            }

            var remaining = await GetRemainingRefundableAsync(refund.OriginalPaymentId.Value, refund.Id);
            if (refund.Amount > remaining)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundExceedsRemainingAmount"
                };
            }

            var payment = await _context.PaymentInvoiceCustomer.FindAsync(refund.OriginalPaymentId.Value);
            var customer = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.CustomerId);
            customer.Account.Adjust(refund.Amount);

            refund.Status = RefundStatus.Approved;
            refund.ApprovedByUserId = approvedByUserId;
            refund.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId)
    {
        try
        {
            var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(a => a.Id == refundId && a.OriginalPaymentId != null);
            if (refund is null)
            {
                return new ResultMessage { Success = true };
            }

            if (refund.Status != RefundStatus.Pending)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundAlreadyProcessed"
                };
            }

            refund.Status = RefundStatus.Rejected;
            refund.ApprovedByUserId = approvedByUserId;
            refund.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
