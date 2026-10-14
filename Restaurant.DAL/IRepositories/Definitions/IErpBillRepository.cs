using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IErpBillRepository
{
    Task<PaginatedResult<ErpBillListItemViewModel>> GetPagedBillsAsync(PageRequest request);
    Task<ErpBillViewModel> GetBillByIdAsync(int id);
    Task<ResultMessage> PostBillAsync(int id);
    Task<ResultMessage> PostPendingAsync();
}
