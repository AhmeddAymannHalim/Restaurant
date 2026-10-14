using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

/// <summary>Data access shared by every simple lookup table (SellType, Department, Currency, ...).</summary>
public interface ILookupCrudRepository<T> where T : BaseEntity
{
    Task<PaginatedResult<T>> GetPagedAsync(PageRequest request);
    Task<T> GetByIdAsync(int id);
    Task<string> NewCodeAsync();
    Task<IEnumerable<LookupItem>> GetOptionsAsync(int? excludeId);
    Task<ResultMessage> UpsertAsync(T entity);
    Task<ResultMessage> DeleteAsync(int id);
}
