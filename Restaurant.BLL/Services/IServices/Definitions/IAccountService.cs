using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IAccountService
{
    Task<ResultMessage<AccountToken>> RegisterAsync(RegisterViewModel model);
    Task<ResultMessage<AccountIdentity>> SignInAsync(SignInViewModel model);
    Task<AccountToken> GetConfirmationTokenAsync(string email);
    Task<ResultMessage> ConfirmEmailAsync(string userId, string token);
    Task<AccountToken> GetPasswordResetTokenAsync(string email);
    Task<ResultMessage> ResetPasswordAsync(ResetPasswordViewModel model);
    Task<ResultMessage> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    Task<ProfileViewModel> GetProfileAsync(string userId);
}
