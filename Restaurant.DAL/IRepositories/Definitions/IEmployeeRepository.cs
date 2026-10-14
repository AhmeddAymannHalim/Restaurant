using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IEmployeeRepository
{
    Task<PaginatedResult<Employee>> GetPagedEmployeesAsync(PageRequest request);
    Task<Employee> GetEmployeeByIdAsync(int id);
    Task<string> NewEmployeeCodeAsync();
    Task<ResultMessage> UpsertEmployeeAsync(Employee entity, IFormFile imageFile, IList<IFormFile> documentFiles);
    Task<ResultMessage> DeleteEmployeeAsync(int id);
}
