using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAccountSettingRepository
{
    Task<AccountSettingViewModel> GetAccountSettingAsync();
    Task<ResultMessage> UpdateAccountSettingAsync(AccountSettingViewModel model);
}
