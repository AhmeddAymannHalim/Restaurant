using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IUnitPriceRepository
{
    Task<PaginatedResult<UnitPrice>> GetPagedUnitPricesAsync(PageRequest request);
    Task<UnitPrice> GetUnitPriceByIdAsync(int id);
    Task<ResultMessage> UpsertUnitPriceAsync(UnitPrice entity);
    Task<ResultMessage> DeleteUnitPriceAsync(int id);
}
