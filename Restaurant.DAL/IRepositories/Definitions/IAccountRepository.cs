using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountRepository
{
    Task<ApplicationUser> FindByEmailAsync(string email);
    Task<List<string>> GetRolesAsync(ApplicationUser user);
    Task<ResultMessage<ApplicationUser>> RegisterCustomerAsync(ApplicationUser entity, string password);
    Task<ResultMessage<ApplicationUser>> CheckPasswordAsync(string email, string password);
    Task<string> GenerateConfirmationTokenAsync(ApplicationUser user);
    Task<ResultMessage> ConfirmEmailAsync(string userId, string token);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<ResultMessage> ResetPasswordAsync(string email, string token, string password);
    Task<ResultMessage> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<ApplicationUser> GetByIdAsync(string userId);
    Task<ResultMessage> UpdateProfileAsync(string userId, ProfileViewModel model);
}
