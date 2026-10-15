using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IBillRepository
{
    Task<PaginatedResult<BillListItemViewModel>> GetPagedBillsAsync(PageRequest request);
    Task<BillViewModel> GetBillByIdAsync(int id);
    Task<ResultMessage> PostBillAsync(int id);
    Task<ResultMessage> PostPendingAsync();
}
