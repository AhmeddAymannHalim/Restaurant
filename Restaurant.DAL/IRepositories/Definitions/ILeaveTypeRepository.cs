using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ILeaveTypeRepository
{
    Task<PaginatedResult<LeaveType>> GetPagedLeaveTypesAsync(PageRequest request);
    Task<LeaveType> GetLeaveTypeByIdAsync(int id);
    Task<ResultMessage> UpsertLeaveTypeAsync(LeaveType entity);
    Task<string> NewLeaveTypeCodeAsync();
    Task<ResultMessage> DeleteLeaveTypeAsync(int id);
}
