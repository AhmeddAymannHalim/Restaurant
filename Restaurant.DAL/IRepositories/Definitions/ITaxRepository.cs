using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ITaxRepository
{
    Task<PaginatedResult<Tax>> GetPagedTaxesAsync(PageRequest request);
    Task<Tax> GetTaxByIdAsync(int id);
    Task<ResultMessage> UpsertTaxAsync(Tax entity);
    Task<ResultMessage> DeleteTaxAsync(int id);
}
