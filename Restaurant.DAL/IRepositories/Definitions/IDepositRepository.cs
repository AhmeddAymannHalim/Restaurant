using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IDepositRepository
{
    Task<PaginatedResult<Deposit>> GetPagedDepositsAsync(PageRequest request);
    Task<Deposit> GetDepositByIdAsync(int id);
    Task<ResultMessage> UpsertDepositAsync(Deposit entity, string userId);
    Task<ResultMessage> DeleteDepositAsync(int id);
    Task<string> NewDepositCodeAsync();
}
