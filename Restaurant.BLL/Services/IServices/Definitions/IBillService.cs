using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IBillService
{
    Task<PaginatedResult<BillListItemViewModel>> GetPagedBillsAsync(PageRequest request);
    Task<BillViewModel> GetBillByIdAsync(int id);
    Task<ResultMessage> PostBillAsync(int id);
    Task<ResultMessage> PostPendingAsync();
}
