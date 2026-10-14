using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class SupplierPaymentRepository : ISupplierPaymentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public SupplierPaymentRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<PaymentInvoiceSupplier>> GetPagedPaymentsAsync(PageRequest request)
    {
        var query = _context.PaymentInvoiceSupplier.AsNoTracking().Include(a => a.Supplier).ThenInclude(a => a.Account).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ProcessCode.ToLower().Contains(search)
                || a.Description.ToLower().Contains(search)
                || a.Supplier.Account.ArName.ToLower().Contains(search)
                || a.Supplier.Account.EnName.ToLower().Contains(search));
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

        return PaginatedResult<PaymentInvoiceSupplier>.Create(data, total, request);
    }

    public async Task<PaymentInvoiceSupplier> GetPaymentByIdAsync(int id)
    {
        return await _context.PaymentInvoiceSupplier.AsNoTracking()
            .Include(a => a.Supplier).ThenInclude(a => a.Account)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var last = await _context.PaymentInvoiceSupplier.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.ProcessCode)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
    }

    public async Task<decimal> GetBalanceAsync(int partnerId)
    {
        var account = await _context.Suppliers.AsNoTracking()
            .Where(a => a.Id == partnerId)
            .Select(a => a.Account)
            .FirstOrDefaultAsync();

        return account?.BalanceValue ?? 0;
    }

    public async Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceSupplier entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var partner = await _context.Suppliers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == entity.SupplierId);
            if (partner is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            PaymentInvoiceSupplier payment;
            if (entity.Id == 0)
            {
                payment = entity;
                payment.ProcessCode = await GetNextCodeAsync();
                payment.Date = DateTime.Now;
                await _context.PaymentInvoiceSupplier.AddAsync(payment);
            }
            else
            {
                payment = await _context.PaymentInvoiceSupplier.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (payment is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                var before = await _context.Suppliers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.SupplierId);
                before.Account.Adjust(payment.Paid);

                payment.SupplierId = entity.SupplierId;
                payment.Paid = entity.Paid;
                payment.Description = entity.Description;
            }

            partner.Account.Adjust(-payment.Paid);
            await _context.SaveChangesAsync();

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.SupplierPaymentVoucher, payment.Id);
            var posting = await _journals.PostSourceEntryAsync(JournalSourceType.SupplierPaymentVoucher,
                payment.Id,
                payment.Date,
                $"Supplier payment {payment.ProcessCode}",
                new[]
                {
                    new JournalLineRequest(AccountCodes.AccountsPayable, payment.Paid, 0),
                    new JournalLineRequest(AccountCodes.Cash, 0, payment.Paid)
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
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeletePaymentAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var payment = await _context.PaymentInvoiceSupplier.FirstOrDefaultAsync(a => a.Id == id);
            if (payment is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.PaymentRefunds.AnyAsync(a => a.SupplierPaymentId == id))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeletePaymentWithRefunds"
                };
            }

            var partner = await _context.Suppliers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.SupplierId);
            partner.Account.Adjust(payment.Paid);

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.SupplierPaymentVoucher, payment.Id);

            _context.PaymentInvoiceSupplier.Remove(payment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<IEnumerable<PaymentRefund>> GetRefundsForPaymentAsync(int paymentId)
    {
        return await _context.PaymentRefunds.AsNoTracking()
            .Include(a => a.SupplierPayment)
            .Include(a => a.RequestedByUser)
            .Include(a => a.ApprovedByUser)
            .Where(a => a.SupplierPaymentId == paymentId)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    private async Task<decimal> GetRemainingRefundableAsync(int paymentId, int excludeRefundId = 0)
    {
        var payment = await _context.PaymentInvoiceSupplier.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == paymentId);
        if (payment is null)
        {
            return 0;
        }

        var approved = await _context.PaymentRefunds.AsNoTracking()
            .Where(a => a.SupplierPaymentId == paymentId
                && a.Status == RefundStatus.Approved
                && a.Id != excludeRefundId)
            .SumAsync(a => a.Amount);

        return payment.Paid - approved;
    }

    public async Task<ResultMessage> RequestRefundAsync(PaymentRefund entity, string requestedByUserId)
    {
        try
        {
            var payment = await _context.PaymentInvoiceSupplier.FirstOrDefaultAsync(a => a.Id == entity.SupplierPaymentId);
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
                .Where(a => a.SupplierPaymentId == entity.SupplierPaymentId && a.Status != RefundStatus.Rejected)
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
                SupplierPaymentId = entity.SupplierPaymentId,
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
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId)
    {
        try
        {
            var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(a => a.Id == refundId && a.SupplierPaymentId != null);
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

            var remaining = await GetRemainingRefundableAsync(refund.SupplierPaymentId.Value, refund.Id);
            if (refund.Amount > remaining)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RefundExceedsRemainingAmount"
                };
            }

            var payment = await _context.PaymentInvoiceSupplier.FindAsync(refund.SupplierPaymentId.Value);
            var supplier = await _context.Suppliers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == payment.SupplierId);
            supplier.Account.Adjust(refund.Amount);

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
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId)
    {
        try
        {
            var refund = await _context.PaymentRefunds.FirstOrDefaultAsync(a => a.Id == refundId && a.SupplierPaymentId != null);
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
                Message = ex.Message
            };
        }
    }
}
