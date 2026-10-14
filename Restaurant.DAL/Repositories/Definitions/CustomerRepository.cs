using Microsoft.AspNetCore.Http;
using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class CustomerRepository : ICustomerRepository
{
    private const string ImagesFolder = "Customer Images";

    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
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
        return await _context.Customers
            .Include(a => a.Account)
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
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

    /// <summary>
    /// The incoming entity carries the ArName/EnName of the form on a transient <see cref="Customer.Account"/>;
    /// a new customer always gets its real Account row created here.
    /// </summary>
    public async Task<ResultMessage> UpsertCustomerAsync(Customer entity, IFormFile imageFile)
    {
        try
        {
            var form = entity.Account;
            entity.Account = null;
            string replacedImage = null;

            if (entity.Id == 0)
            {
                entity.Account = await AccountProvisioner.CreateAsync(
                    _context,
                    form.ArName,
                    form.EnName,
                    AccountType.Customer,
                    AccountCodes.AccountsReceivable,
                    form.DebitBalance);
                entity.CreatedAt = DateTime.Now;
                entity.ImageUrl = imageFile is null ? null : DocumentSetting.UploadFile(imageFile, ImagesFolder);

                await _context.Customers.AddAsync(entity);
            }
            else
            {
                var existing = await _context.Customers
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

                replacedImage = existing.ImageUrl;
                entity.AccountId = existing.AccountId;
                entity.ImageUrl = imageFile is null ? existing.ImageUrl : DocumentSetting.UploadFile(imageFile, ImagesFolder);
                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;

                existing.Account.ArName = form.ArName;
                existing.Account.EnName = form.EnName;
                existing.Account.Adjust(form.DebitBalance - existing.Account.BalanceValue);
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();

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
                Message = ex.Message
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
                Message = ex.Message
            };
        }
    }
}
