using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IDepartmentRepository
{
    Task<PaginatedResult<Department>> GetPagedDepartmentsAsync(PageRequest request);
    Task<Department> GetDepartmentByIdAsync(int id);
    Task<ResultMessage> UpsertDepartmentAsync(Department entity);
    Task<string> NewDepartmentCodeAsync();
    Task<ResultMessage> DeleteDepartmentAsync(int id);
}
