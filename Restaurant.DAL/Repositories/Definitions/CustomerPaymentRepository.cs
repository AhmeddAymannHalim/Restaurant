using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class CustomerPaymentRepository : ICustomerPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerPaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PaymentInvoiceCustomer>> GetPagedPaymentsAsync(PageRequest request)
    {
        var query = _context.PaymentInvoiceCustomer.AsNoTracking().Include(a => a.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ProcessCode.ToLower().Contains(search)
                || a.Description.ToLower().Contains(search)
                || a.Customer.ArName.ToLower().Contains(search)
                || a.Customer.EnName.ToLower().Contains(search));
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
            .Include(a => a.Customer)
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
        return await _context.Customers.AsNoTracking()
            .Where(a => a.Id == partnerId)
            .Select(a => a.Balance)
            .FirstOrDefaultAsync();
    }

    public async Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceCustomer entity)
    {
        try
        {
            var partner = await _context.Customers.FindAsync(entity.CustomerId);
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

                var before = await _context.Customers.FindAsync(payment.CustomerId);
                before.Balance += payment.Paid;

                payment.CustomerId = entity.CustomerId;
                payment.Paid = entity.Paid;
                payment.Description = entity.Description;
            }

            partner.Balance -= payment.Paid;
            await _context.SaveChangesAsync();

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
        try
        {
            var payment = await _context.PaymentInvoiceCustomer.FirstOrDefaultAsync(a => a.Id == id);
            if (payment is null)
            {
                return new ResultMessage { Success = true };
            }

            var partner = await _context.Customers.FindAsync(payment.CustomerId);
            partner.Balance += payment.Paid;

            _context.PaymentInvoiceCustomer.Remove(payment);
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
