using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class AccountSettingService : IAccountSettingService
{
    private readonly IAccountSettingRepository _repository;

    public AccountSettingService(IAccountSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountSettingViewModel> GetAccountSettingAsync()
    {
        return await _repository.GetAccountSettingAsync();
    }

    public async Task<ResultMessage> UpdateAccountSettingAsync(AccountSettingViewModel model)
    {
        return await _repository.UpdateAccountSettingAsync(model);
    }
}
