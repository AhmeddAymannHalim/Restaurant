using Shared.Helpers;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContext;
    private readonly IAccountProvisionRepository _accountProvision;
    private readonly IPhoneRepository _phones;
    private readonly ILocationRepository _location;

    public SupplierRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
        _accountProvision = new AccountProvisionRepository(context);
        _phones = new PhoneRepository(context);
        _location = new LocationRepository(context);
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
        var supplier = await _context.Suppliers
            .Include(a => a.Account)
            .Include(a => a.Phones)
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (supplier is null)
        {
            return null;
        }

        supplier.InvoiceCount = await CountInvoicesAsync(id);

        return supplier;
    }

    public async Task<int> CountInvoicesAsync(int id)
    {
        var accountId = await _context.Suppliers.AsNoTracking().Where(a => a.Id == id).Select(a => a.AccountId).FirstOrDefaultAsync();

        return accountId.HasValue
            ? await _context.BillMasters.CountAsync(a => a.CustomerOrSupplierAccountId == accountId
                                                        && a.AmountPaided < a.TotalBillAmountAfterDiscountAndExtra): 0;
    }

    public async Task<ResultMessage> UpsertSupplierAsync(Supplier entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var form = entity.Account;
            if (form.DebitBalance < 0 || form.CreditBalance < 0)
            {
                return new ResultMessage { Success = false, Message = "NegativeAmount" };
            }

            entity.Account = null;
            var phones = _phones.CleanNumbers(entity.PhoneNumber1, entity.Phones);
            entity.Phones = new List<SupplierPhone>();
            entity.PhoneNumber2 = phones.ElementAtOrDefault(0);
            var location = await _location.ResolveAsync(entity.TownId);
            entity.GovId = location.GovId;
            entity.CountryId = location.CountryId;
            Account openingAccount = null;
            decimal openingDelta = 0;

            if (entity.Id == 0)
            {
                var account = await _accountProvision.CreateAsync(form.ArName,
                    form.EnName,
                    AccountType.Supplier,
                    form.CreditBalance - form.DebitBalance);

                entity.Account = account;
                openingAccount = account;
                openingDelta = form.CreditBalance - form.DebitBalance;
                entity.LastBalance = openingDelta;
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";

                _phones.SyncSupplierPhones(entity.Phones, phones);
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
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";

                existing.Account.ArName = form.ArName;
                existing.Account.EnName = form.EnName;
                openingAccount = existing.Account;
                openingDelta = (form.CreditBalance - form.DebitBalance) - (existing.Account.CreditBalance - existing.Account.DebitBalance);
                existing.Account.Adjust(openingDelta);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _phones.SyncSupplierPhones(existing.Phones, phones);
            }

            await _context.SaveChangesAsync();
            var posting = await new JournalEntryRepository(_context).PostOpeningBalanceAsync(openingAccount, openingDelta);
            if (!posting.Success)
            {
                return posting;
            }

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

            if (entity.Account is not null && entity.Account.DebitBalance != entity.Account.CreditBalance)
            {
                return new ResultMessage { Success = false, Message = "LockedByBalance" };
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
