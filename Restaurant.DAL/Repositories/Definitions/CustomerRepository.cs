using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Customer>> GetPagedCustomersAsync(PageRequest request)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search) || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Customer>.Create(data, total, request);
    }

    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<decimal?> GetCustomerBalanceAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => (decimal?)a.Balance)
            .FirstOrDefaultAsync();
    }

    public async Task<ResultMessage> UpsertCustomerAsync(Customer entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.Customers.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                _context.Customers.Update(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }

    public async Task<ResultMessage> DeleteCustomerAsync(int id)
    {
        try
        {
            var entity = await _context.Customers.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Customers.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }
}
