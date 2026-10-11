using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ITableService
{
    Task<PaginatedResult<TableViewModel>> GetPagedTablesAsync(PageRequest request);
    Task<TableViewModel> GetTableByIdAsync(int id);
    Task<ResultMessage> UpsertTableAsync(TableViewModel model);
    Task<ResultMessage> DeleteTableAsync(int id);
}
