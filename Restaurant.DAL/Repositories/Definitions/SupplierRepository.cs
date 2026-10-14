using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Supplier>> GetPagedSuppliersAsync(PageRequest request)
    {
        var query = _context.Suppliers.AsNoTracking()
            .Include(a => a.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Account.ArName.ToLower().Contains(search)
                                  || a.Account.EnName.ToLower().Contains(search)
                                  || a.Account.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Supplier>.Create(data, total, request);
    }

    public async Task<Supplier> GetSupplierByIdAsync(int id)
    {
        return await _context.Suppliers
            .Include(a => a.Account)
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> UpsertSupplierAsync(Supplier entity)
    {
        try
        {
            var form = entity.Account;
            entity.Account = null;
            (entity.GovId, entity.CountryId) = await LocationResolver.ResolveAsync(_context, entity.TownId);

            if (entity.Id == 0)
            {
                var account = await AccountProvisioner.CreateAsync(
                    _context,
                    form.ArName,
                    form.EnName,
                    AccountType.Supplier,
                    AccountCodes.AccountsPayable,
                    form.CreditBalance);

                entity.Account = account;
                entity.LastBalance = form.CreditBalance;
                entity.CreatedAt = DateTime.Now;

                await _context.Suppliers.AddAsync(entity);
            }
            else
            {
                var existing = await _context.Suppliers
                    .Include(a => a.Account)
                    .FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.AccountId = existing.AccountId;
                entity.LastBalance = existing.LastBalance;
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;

                existing.Account.ArName = form.ArName;
                existing.Account.EnName = form.EnName;
                existing.Account.Adjust(form.CreditBalance - existing.Account.BalanceValue);
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

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

    public async Task<ResultMessage> DeleteSupplierAsync(int id)
    {
        try
        {
            var entity = await _context.Suppliers
                .Include(a => a.Account)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Suppliers.Remove(entity);
            if (entity.Account is not null)
            {
                _context.Accounts.Remove(entity.Account);
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
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
}
