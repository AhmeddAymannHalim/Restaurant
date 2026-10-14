using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ILeaveTypeService
{
    Task<PaginatedResult<LeaveTypeViewModel>> GetPagedLeaveTypesAsync(PageRequest request);
    Task<LeaveTypeViewModel> GetLeaveTypeByIdAsync(int id);
    Task<ResultMessage> UpsertLeaveTypeAsync(LeaveTypeViewModel model);
    Task<ResultMessage> DeleteLeaveTypeAsync(int id);
}
