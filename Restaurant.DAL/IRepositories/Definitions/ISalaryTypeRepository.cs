using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISalaryTypeRepository
{
    Task<PaginatedResult<SalaryType>> GetPagedSalaryTypesAsync(PageRequest request);
    Task<SalaryType> GetSalaryTypeByIdAsync(int id);
    Task<ResultMessage> UpsertSalaryTypeAsync(SalaryType entity);
    Task<ResultMessage> DeleteSalaryTypeAsync(int id);
}
