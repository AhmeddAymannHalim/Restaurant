using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IDepositService
{
    Task<PaginatedResult<DepositViewModel>> GetPagedDepositsAsync(PageRequest request);
    Task<DepositViewModel> GetDepositByIdAsync(int id);
    Task<ResultMessage> UpsertDepositAsync(DepositViewModel model, string userId);
    Task<ResultMessage> DeleteDepositAsync(int id);
    Task<string> NewDepositCodeAsync();
}
