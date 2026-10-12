using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IUserRepository
{
    Task<PaginatedResult<ApplicationUser>> GetPagedUsersAsync(PageRequest request);
    Task<ApplicationUser> GetUserByIdAsync(string id);
    bool IsEmailExist(string id, string email);
    bool IsUserNameExist(string id, string userName);
    Task<ResultMessage> UpsertUserAsync(ApplicationUser entity, string password);
    Task<ResultMessage> DeleteUserAsync(string id);
}
