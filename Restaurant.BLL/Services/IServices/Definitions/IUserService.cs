using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IUserService
{
    Task<PaginatedResult<UserViewModel>> GetPagedUsersAsync(PageRequest request);
    Task<UserViewModel> GetUserByIdAsync(string id);
    Task<ResultMessage> UpsertUserAsync(UserViewModel model);
    Task<ResultMessage> DeleteUserAsync(string id);
}
