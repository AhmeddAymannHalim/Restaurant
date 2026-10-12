using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class EmployeeAccountRepository : IEmployeeAccountRepository
{
    private const string CustomerRole = "User";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeeAccountRepository(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PaginatedResult<ApplicationUser>> GetPagedAccountsAsync(PageRequest request)
    {
        var query = _userManager.Users.AsNoTracking().Where(a => a.UserType != CustomerRole);

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Email.ToLower().Contains(search)
                || a.UserName.ToLower().Contains(search)
                || a.FirstName.ToLower().Contains(search)
                || a.LastName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.FirstName)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<ApplicationUser>.Create(data, total, request);
    }

    public async Task<ApplicationUser> GetAccountByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser> GetAccountByEmployeeIdAsync(int employeeId)
    {
        var employee = await GetEmployeeByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        return await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Code == employee.Code);
    }

    public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
    {
        return await _context.Employees.AsNoTracking().FirstOrDefaultAsync(a => a.EmployeeId == employeeId);
    }

    public async Task<List<Employee>> GetEmployeesWithoutAccountAsync()
    {
        var usedCodes = _userManager.Users.Where(a => a.Code != null).Select(a => a.Code);

        return await _context.Employees.AsNoTracking()
            .Where(a => !usedCodes.Contains(a.Code))
            .OrderBy(a => a.FirstName)
            .ToListAsync();
    }

    public async Task<ResultMessage> UpsertAccountAsync(ApplicationUser entity, int employeeId, string password)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            return await CreateAccountAsync(entity, employeeId, password);
        }

        return await UpdateAccountAsync(entity, password);
    }

    private async Task<ResultMessage> CreateAccountAsync(ApplicationUser entity, int employeeId, string password)
    {
        var employee = await GetEmployeeByIdAsync(employeeId);
        if (employee is null)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        var user = new ApplicationUser
        {
            UserName = entity.UserName,
            Email = entity.Email,
            EmailConfirmed = true,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Country = employee.Country,
            City = employee.City,
            Street = employee.Street,
            Age = employee.Age,
            Image = employee.Image,
            PhoneNumber = employee.PhoneNumber,
            MobileNumber = employee.MobileNumber,
            UserType = employee.UserType,
            Code = employee.Code
        };

        if (!await _roleManager.RoleExistsAsync(user.UserType))
        {
            await _roleManager.CreateAsync(new IdentityRole(user.UserType));
        }

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return ToResultMessage(created);
        }

        return ToResultMessage(await _userManager.AddToRoleAsync(user, user.UserType));
    }

    private async Task<ResultMessage> UpdateAccountAsync(ApplicationUser entity, string password)
    {
        var existing = await _userManager.FindByIdAsync(entity.Id);
        if (existing is null)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        existing.UserName = entity.UserName;
        existing.Email = entity.Email;

        var updated = await _userManager.UpdateAsync(existing);
        if (!updated.Succeeded || string.IsNullOrEmpty(password))
        {
            return ToResultMessage(updated);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(existing);

        return ToResultMessage(await _userManager.ResetPasswordAsync(existing, token, password));
    }

    private static ResultMessage ToResultMessage(IdentityResult result)
    {
        return new ResultMessage
        {
            Success = result.Succeeded,
            Message = result.Succeeded ? null : result.Errors.First().Description
        };
    }
}
