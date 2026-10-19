using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleRepository(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<PaginatedResult<IdentityRole>> GetPagedRolesAsync(PageRequest request)
    {
        var query = _roleManager.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Name)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<IdentityRole>.Create(data, total, request);
    }

    public async Task<IdentityRole> GetRoleByIdAsync(string id)
    {
        return await _roleManager.FindByIdAsync(id);
    }

    public bool IsNameExist(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalized = name.Trim().ToUpperInvariant();

        return _roleManager.Roles.Any(a => a.NormalizedName == normalized && a.Id != id);
    }

    public async Task<ResultMessage> UpsertRoleAsync(IdentityRole entity)
    {
        IdentityResult result;

        if (string.IsNullOrEmpty(entity.Id))
        {
            result = await _roleManager.CreateAsync(new IdentityRole(entity.Name));
        }
        else
        {
            var existing = await _roleManager.FindByIdAsync(entity.Id);
            if (existing is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            if ((existing.Name == "Admin" || existing.Name == "Manager" || existing.Name == "كاشير" || existing.Name == "User"))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "SystemRoleLocked"
                };
            }

            existing.Name = entity.Name;
            result = await _roleManager.UpdateAsync(existing);
        }

        return new ResultMessage { Success = result.Succeeded, Message = result.Succeeded ? null : "Identity." + result.Errors.First().Code };
    }

    public async Task<ResultMessage> DeleteRoleAsync(string id)
    {
        var entity = await _roleManager.FindByIdAsync(id);
        if (entity is null)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "RecordNotFound"
            };
        }

        if ((entity.Name == "Admin" || entity.Name == "Manager" || entity.Name == "كاشير" || entity.Name == "User"))
        {
            return new ResultMessage
            {
                Success = false,
                Message = "SystemRoleLocked"
            };
        }

        var deleted = await _roleManager.DeleteAsync(entity);

        return new ResultMessage { Success = deleted.Succeeded, Message = deleted.Succeeded ? null : "Identity." + deleted.Errors.First().Code };
    }
}
