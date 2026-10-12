using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

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
            .Include(a => a.Branche)
            .Include(a => a.SalaryType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(search)
                                  || a.LastName.ToLower().Contains(search)
                                  || a.Code.ToLower().Contains(search)
                                  || a.MobileNumber.Contains(search)
                                  || a.Job.ArName.ToLower().Contains(search)
                                  || a.Job.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.EmployeeId)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        var codes = data.Select(a => a.Code).ToList();
        var withAccount = await _context.Users
            .AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .Select(a => a.Code)
            .ToListAsync();

        foreach (var employee in data)
        {
            employee.HasAccount = withAccount.Contains(employee.Code);
        }

        return PaginatedResult<Employee>.Create(data, total, request);
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
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

    public async Task<ResultMessage> UpsertEmployeeAsync(Employee entity, IFormFile imageFile)
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

            entity.UserType = job.ArName;

            if (entity.EmployeeId == 0)
            {
                entity.Code = await NewEmployeeCodeAsync();

                if (imageFile != null)
                {
                    entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                await _context.Employees.AddAsync(entity);
                await _context.SaveChangesAsync();

                return new ResultMessage { Success = true };
            }

            var existing = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(a => a.EmployeeId == entity.EmployeeId);
            if (existing is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            entity.Code = existing.Code;
            entity.Image = existing.Image;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.Image))
                {
                    DocumentSetting.DeleteFile(existing.Image);
                }

                entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
            }

            _context.Employees.Update(entity);
            await SyncLoginAccountAsync(entity, job.ArName);
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

    private async Task SyncLoginAccountAsync(Employee employee, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(a => a.Code == employee.Code);
        if (user is null)
        {
            return;
        }

        var oldRoleName = user.UserType;

        user.FirstName = employee.FirstName;
        user.LastName = employee.LastName;
        user.Country = employee.Country;
        user.City = employee.City;
        user.Street = employee.Street;
        user.Age = employee.Age;
        user.PhoneNumber = employee.PhoneNumber;
        user.MobileNumber = employee.MobileNumber;
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
            var employee = await _context.Employees.FindAsync(id);
            if (employee is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Employees.Remove(employee);
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
