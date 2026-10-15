using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
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
            .Include(a => a.Phones)
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> UpsertSupplierAsync(Supplier entity)
    {
        try
        {
            var form = entity.Account;
            entity.Account = null;
            var phones = PhoneRows.Clean(entity.PhoneNumber1, entity.Phones);
            entity.Phones = new List<SupplierPhone>();
            entity.PhoneNumber2 = phones.ElementAtOrDefault(0);
            (entity.GovId, entity.CountryId) = await LocationResolver.ResolveAsync(_context, entity.TownId);
            Account openingAccount = null;
            decimal openingDelta = 0;

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
                openingAccount = account;
                openingDelta = form.CreditBalance;
                entity.LastBalance = form.CreditBalance;
                entity.CreatedAt = DateTime.Now;

                PhoneRows.Sync(_context, entity.Phones, phones);
                await _context.Suppliers.AddAsync(entity);
            }
            else
            {
                var existing = await _context.Suppliers
                    .Include(a => a.Account)
                    .Include(a => a.Phones)
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
                openingAccount = existing.Account;
                openingDelta = form.CreditBalance - existing.Account.BalanceValue;
                existing.Account.Adjust(openingDelta);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                PhoneRows.Sync(_context, existing.Phones, phones);
            }

            await _context.SaveChangesAsync();
            await new JournalEntryRepository(_context).PostOpeningBalanceAsync(openingAccount, openingDelta);

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
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
