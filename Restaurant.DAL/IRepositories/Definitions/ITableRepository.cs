using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ITableRepository
{
    Task<PaginatedResult<Restaurant.DAL.Models.Table>> GetPagedTablesAsync(PageRequest request);
    Task<Restaurant.DAL.Models.Table> GetTableByIdAsync(int id);
    Task<ResultMessage> UpsertTableAsync(Restaurant.DAL.Models.Table entity);
    Task<ResultMessage> DeleteTableAsync(int id);
}
