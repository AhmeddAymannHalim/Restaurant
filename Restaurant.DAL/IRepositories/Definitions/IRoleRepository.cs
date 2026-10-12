using Microsoft.AspNetCore.Identity;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IRoleRepository
{
    Task<PaginatedResult<IdentityRole>> GetPagedRolesAsync(PageRequest request);
    Task<IdentityRole> GetRoleByIdAsync(string id);
    bool IsNameExist(string id, string name);
    Task<ResultMessage> UpsertRoleAsync(IdentityRole entity);
    Task<ResultMessage> DeleteRoleAsync(string id);
}
