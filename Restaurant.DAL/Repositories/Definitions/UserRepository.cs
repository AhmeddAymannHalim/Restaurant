using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class UserRepository : IUserRepository
{
    private const string AdminRole = "Admin";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PaginatedResult<ApplicationUser>> GetPagedUsersAsync(PageRequest request)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Email.ToLower().Contains(search)
                || a.UserName.ToLower().Contains(search)
                || a.FirstName.ToLower().Contains(search)
                || a.LastName.ToLower().Contains(search)
                || a.UserType.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.UserType)
            .ThenBy(a => a.FirstName)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<ApplicationUser>.Create(data, total, request);
    }

    public async Task<ApplicationUser> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public bool IsEmailExist(string id, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var normalized = email.Trim().ToUpperInvariant();

        return _userManager.Users.Any(a => a.NormalizedEmail == normalized && a.Id != id);
    }

    public bool IsUserNameExist(string id, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        var normalized = userName.Trim().ToUpperInvariant();

        return _userManager.Users.Any(a => a.NormalizedUserName == normalized && a.Id != id);
    }

    public async Task<ResultMessage> UpsertUserAsync(ApplicationUser entity, string password)
    {
        if (!await _roleManager.RoleExistsAsync(entity.UserType))
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RoleNotFound"
            };
        }

        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.EmailConfirmed = true;
            var created = await _userManager.CreateAsync(entity, password);
            if (!created.Succeeded)
            {
                return ToResultMessage(created);
            }

            return ToResultMessage(await _userManager.AddToRoleAsync(entity, entity.UserType));
        }

        var existing = await _userManager.FindByIdAsync(entity.Id);
        if (existing is null)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        if (existing.UserType == AdminRole && entity.UserType != AdminRole && await IsLastAdminAsync())
        {
            return new ResultMessage
            {
                Success = false,
                Message = "LastAdminCannotChange"
            };
        }

        var oldRoles = await _userManager.GetRolesAsync(existing);

        existing.UserName = entity.UserName;
        existing.FirstName = entity.FirstName;
        existing.LastName = entity.LastName;
        existing.Email = entity.Email;
        existing.PhoneNumber = entity.PhoneNumber;
        existing.UserType = entity.UserType;

        var updated = await _userManager.UpdateAsync(existing);
        if (!updated.Succeeded)
        {
            return ToResultMessage(updated);
        }

        await _userManager.RemoveFromRolesAsync(existing, oldRoles);

        return ToResultMessage(await _userManager.AddToRoleAsync(existing, existing.UserType));
    }

    public async Task<ResultMessage> DeleteUserAsync(string id)
    {
        var entity = await _userManager.FindByIdAsync(id);
        if (entity is null)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        if (entity.UserType == AdminRole && await IsLastAdminAsync())
        {
            return new ResultMessage
            {
                Success = false,
                Message = "LastAdminCannotChange"
            };
        }

        return ToResultMessage(await _userManager.DeleteAsync(entity));
    }

    private async Task<bool> IsLastAdminAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync(AdminRole);

        return admins.Count <= 1;
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
