using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISellTypeRepository
{
    Task<PaginatedResult<SellType>> GetPagedSellTypesAsync(PageRequest request);
    Task<SellType> GetSellTypeByIdAsync(int id);
    Task<ResultMessage> UpsertSellTypeAsync(SellType entity);
    Task<ResultMessage> DeleteSellTypeAsync(int id);
}
