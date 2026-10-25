using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IClassificationRepository
{
    Task<PaginatedResult<Classification>> GetPagedClassificationsAsync(PageRequest request);
    Task<Classification> GetClassificationByIdAsync(int id);
    Task<ResultMessage> UpsertClassificationAsync(Classification entity);
    Task<ResultMessage> DeleteClassificationAsync(int id);
}
