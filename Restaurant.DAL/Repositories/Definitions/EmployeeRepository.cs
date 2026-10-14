using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class EmployeeRepository : IEmployeeRepository
{
    private const string ImagesFolder = "Employee Images";

    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Employee>> GetPagedEmployeesAsync(PageRequest request)
    {
        var query = _context.Employees.AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Branch)
            .Include(a => a.Town)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search)
                                  || a.EnName.ToLower().Contains(search)
                                  || a.Code.ToLower().Contains(search)
                                  || a.PhoneNumber.Contains(search)
                                  || a.Job.ArName.ToLower().Contains(search)
                                  || a.Job.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        var codes = data.Select(a => a.Code).ToList();
        var withLogin = await _context.Users
            .AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .Select(a => a.Code)
            .ToListAsync();

        foreach (var employee in data)
        {
            employee.HasLoginAccount = withLogin.Contains(employee.Code);
        }

        return PaginatedResult<Employee>.Create(data, total, request);
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Include(a => a.Town)
            .ThenInclude(a => a.Governorate)
            .Include(a => a.Phones)
            .Include(a => a.Dependents)
            .Include(a => a.Allowances)
            .Include(a => a.FixedDeductions)
            .Include(a => a.Leaves)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> NewEmployeeCodeAsync()
    {
        var codes = await _context.Employees.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    /// <summary>A new employee always gets its Account row (mandatory object creation); the rest of the ERP data stays empty.</summary>
    public async Task<ResultMessage> UpsertEmployeeAsync(Employee entity, IFormFile imageFile, IList<IFormFile> documentFiles)
    {
        try
        {
            var job = await _context.Jobs.FindAsync(entity.JobId);
            if (job is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            (entity.GovId, entity.CountryId) = await LocationResolver.ResolveAsync(_context, entity.TownId);

            if (entity.Id == 0)
            {
                entity.Code = await NewEmployeeCodeAsync();
                entity.CreatedAt = DateTime.Now;
                entity.Phones = BuildPhones(entity);
                EmployeeChildren.PrepareNew(entity, documentFiles);

                if (imageFile != null)
                {
                    entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                entity.Account = await AccountProvisioner.CreateAsync(
                    _context,
                    entity.ArName,
                    entity.EnName,
                    AccountType.Employee,
                    AccountCodes.EmployeesPayable);

                await _context.Employees.AddAsync(entity);
                await _context.SaveChangesAsync();

                return new ResultMessage { Success = true };
            }

            var existing = await _context.Employees
                .Include(a => a.Account)
                .Include(a => a.Phones)
                .Include(a => a.Dependents)
                .Include(a => a.Allowances)
                .Include(a => a.FixedDeductions)
                .Include(a => a.Leaves)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == entity.Id);
            if (existing is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var oldImage = existing.Image;

            entity.Code = existing.Code;
            entity.Image = existing.Image;
            entity.AccountId = existing.AccountId;
            entity.UserId = existing.UserId;
            entity.State = existing.State;
            entity.CreatedAt = existing.CreatedAt;
            entity.CreatedById = existing.CreatedById;
            entity.UpdatedAt = DateTime.Now;

            if (imageFile != null)
            {
                entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
            }

            if (existing.Account is null)
            {
                entity.Account = await AccountProvisioner.CreateAsync(
                    _context,
                    entity.ArName,
                    entity.EnName,
                    AccountType.Employee,
                    AccountCodes.EmployeesPayable);
            }
            else
            {
                existing.Account.ArName = entity.ArName;
                existing.Account.EnName = entity.EnName;
            }

            var phones = BuildPhones(entity);
            entity.Phones = null;
            _context.Entry(existing).CurrentValues.SetValues(entity);
            SyncPhones(existing, phones);
            EmployeeChildren.Sync(_context, existing, entity, documentFiles);
            if (existing.Account is null && entity.Account is not null)
            {
                existing.Account = entity.Account;
            }

            await SyncLoginAccountAsync(entity, job.ArName);
            await _context.SaveChangesAsync();

            if (imageFile != null && !string.IsNullOrEmpty(oldImage))
            {
                DocumentSetting.DeleteFile(oldImage);
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

    // The main mobile plus the extra ones typed with the "+ Add" button, without blanks or repeats.
    private static List<EmployeePhone> BuildPhones(Employee entity)
    {
        var numbers = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.PhoneNumber))
        {
            numbers.Add(entity.PhoneNumber.Trim());
        }

        numbers.AddRange((entity.Phones ?? new List<EmployeePhone>())
            .Select(a => a.PhoneNumber?.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a)));

        return numbers.Distinct().Select(a => new EmployeePhone { PhoneNumber = a, CreatedAt = DateTime.Now }).ToList();
    }

    private void SyncPhones(Employee existing, List<EmployeePhone> desired)
    {
        var wanted = desired.Select(a => a.PhoneNumber).ToHashSet();

        foreach (var phone in existing.Phones.Where(a => !wanted.Contains(a.PhoneNumber)).ToList())
        {
            _context.EmployeePhones.Remove(phone);
        }

        var kept = existing.Phones.Select(a => a.PhoneNumber).ToHashSet();
        foreach (var phone in desired.Where(a => !kept.Contains(a.PhoneNumber)))
        {
            existing.Phones.Add(phone);
        }
    }

    private async Task SyncLoginAccountAsync(Employee employee, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(a => a.Code == employee.Code);
        if (user is null)
        {
            return;
        }

        var oldRoleName = user.UserType;

        user.FirstName = employee.ArName;
        user.LastName = string.Empty;
        user.TownId = employee.TownId;
        user.Street = employee.Address;
        user.PhoneNumber = employee.PhoneNumber;
        user.UserType = roleName;

        if (oldRoleName == roleName)
        {
            return;
        }

        var oldRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == oldRoleName);
        if (oldRole != null)
        {
            var oldLinks = await _context.UserRoles
                .Where(a => a.UserId == user.Id && a.RoleId == oldRole.Id)
                .ToListAsync();
            _context.UserRoles.RemoveRange(oldLinks);
        }

        var newRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == roleName);
        if (newRole is null)
        {
            newRole = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            await _context.Roles.AddAsync(newRole);
        }

        var alreadyInRole = await _context.UserRoles.AnyAsync(a => a.UserId == user.Id && a.RoleId == newRole.Id);
        if (!alreadyInRole)
        {
            await _context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newRole.Id
            });
        }
    }

    public async Task<ResultMessage> DeleteEmployeeAsync(int id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(a => a.Account)
                .Include(a => a.Phones)
                .Include(a => a.Dependents)
                .Include(a => a.Allowances)
                .Include(a => a.FixedDeductions)
                .Include(a => a.Leaves)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (employee is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.EmployeePhones.RemoveRange(employee.Phones);
            EmployeeChildren.Remove(_context, employee);
            _context.Employees.Remove(employee);
            if (employee.Account is not null)
            {
                _context.Accounts.Remove(employee.Account);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(employee.Image))
            {
                DocumentSetting.DeleteFile(employee.Image);
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
