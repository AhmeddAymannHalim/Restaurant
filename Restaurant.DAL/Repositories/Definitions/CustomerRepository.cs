using Microsoft.AspNetCore.Http;
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

public class CustomerRepository : ICustomerRepository
{
    private const string ImagesFolder = "Customer Images";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IAccountProvisionRepository _accountProvision;
    private readonly IPhoneRepository _phones;
    private readonly ILocationRepository _location;

    public CustomerRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
        _accountProvision = new AccountProvisionRepository(context);
        _phones = new PhoneRepository(context);
        _location = new LocationRepository(context);
    }

    public async Task<PaginatedResult<Customer>> GetPagedCustomersAsync(PageRequest request)
    {
        var query = _context.Customers.AsNoTracking()
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

        return PaginatedResult<Customer>.Create(data, total, request);
    }

    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .Include(a => a.Account)
            .Include(a => a.Phones)
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (customer is null)
        {
            return null;
        }

        customer.InvoiceCount = await CountInvoicesAsync(id);

        return customer;
    }

    public async Task<decimal?> GetCustomerBalanceAsync(int id)
    {
        var account = await _context.Customers
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => a.Account)
            .FirstOrDefaultAsync();

        return account?.BalanceValue;
    }

    public async Task<int> CountInvoicesAsync(int id)
    {
        var accountId = await _context.Customers.AsNoTracking().Where(a => a.Id == id).Select(a => a.AccountId).FirstOrDefaultAsync();

        return accountId.HasValue
            ? await _context.BillMasters.CountAsync(a => a.CustomerOrSupplierAccountId == accountId
           && a.AmountPaided < a.TotalBillAmountAfterDiscountAndExtra) : 0;
    }

    public async Task<ResultMessage> UpsertCustomerAsync(Customer entity, IFormFile imageFile)
    {
        try
        {
            if (!DocumentSetting.IsAllowedImage(imageFile))
            {
                return new ResultMessage { Success = false, Message = "InvalidFile" };
            }

            var form = entity.Account;
            if (form.DebitBalance < 0 || form.CreditBalance < 0)
            {
                return new ResultMessage { Success = false, Message = "NegativeAmount" };
            }

            entity.Account = null;
            var phones = _phones.CleanNumbers(entity.PhoneNumber1, entity.Phones);
            entity.Phones = new List<CustomerPhone>();
            entity.PhoneNumber2 = phones.ElementAtOrDefault(0);
            var location = await _location.ResolveAsync(entity.TownId);
            entity.GovId = location.GovId;
            entity.CountryId = location.CountryId;
            string replacedImage = null;
            Account openingAccount = null;
            decimal openingDelta = 0;

            if (entity.Id == 0)
            {
                entity.Account = await _accountProvision.CreateAsync(form.ArName,
                    form.EnName,
                    AccountType.Customer,
                    form.DebitBalance - form.CreditBalance);
                openingAccount = entity.Account;
                openingDelta = form.DebitBalance - form.CreditBalance;
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
                entity.ImageUrl = imageFile is null ? null : DocumentSetting.UploadFile(imageFile, ImagesFolder);

                _phones.SyncCustomerPhones(entity.Phones, phones);
                await _context.Customers.AddAsync(entity);
            }
            else
            {
                var existing = await _context.Customers
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

                replacedImage = existing.ImageUrl;
                entity.AccountId = existing.AccountId;
                entity.ImageUrl = imageFile is null ? existing.ImageUrl : DocumentSetting.UploadFile(imageFile, ImagesFolder);
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";

                existing.Account.ArName = form.ArName;
                existing.Account.EnName = form.EnName;
                openingAccount = existing.Account;
                openingDelta = (form.DebitBalance - form.CreditBalance) - (existing.Account.DebitBalance - existing.Account.CreditBalance);
                existing.Account.Adjust(openingDelta);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _phones.SyncCustomerPhones(existing.Phones, phones);
            }

            await _context.SaveChangesAsync();
            await new JournalEntryRepository(_context).PostOpeningBalanceAsync(openingAccount, openingDelta);

            if (imageFile is not null && !string.IsNullOrEmpty(replacedImage))
            {
                DocumentSetting.DeleteFile(replacedImage);
            }

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

    public async Task<ResultMessage> DeleteCustomerAsync(int id)
    {
        try
        {
            var entity = await _context.Customers
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

            _context.Customers.Remove(entity);
            if (entity.Account is not null)
            {
                _context.Accounts.Remove(entity.Account);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                DocumentSetting.DeleteFile(entity.ImageUrl);
            }

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
