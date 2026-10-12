using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IRoleService
{
    Task<PaginatedResult<RoleViewModel>> GetPagedRolesAsync(PageRequest request);
    Task<RoleViewModel> GetRoleByIdAsync(string id);
    Task<ResultMessage> UpsertRoleAsync(RoleViewModel model);
    Task<ResultMessage> DeleteRoleAsync(string id);
}
