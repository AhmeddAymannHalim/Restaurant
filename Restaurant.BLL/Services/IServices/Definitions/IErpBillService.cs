using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IErpBillService
{
    Task<PaginatedResult<ErpBillListItemViewModel>> GetPagedBillsAsync(PageRequest request);
    Task<ErpBillViewModel> GetBillByIdAsync(int id);
    Task<ResultMessage> PostBillAsync(int id);
    Task<ResultMessage> PostPendingAsync();
}
