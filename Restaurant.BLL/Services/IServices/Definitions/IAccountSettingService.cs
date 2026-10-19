using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IAccountSettingService
{
    Task<AccountSettingViewModel> GetAccountSettingAsync();
    Task<ResultMessage> UpdateAccountSettingAsync(AccountSettingViewModel model);
}
