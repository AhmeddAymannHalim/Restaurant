using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICompanyRepository
{
    Task<PaginatedResult<Company>> GetPagedCompaniesAsync(PageRequest request);
    Task<Company> GetCompanyByIdAsync(int id);
    Task<ResultMessage> UpsertCompanyAsync(Company entity);
    Task<string> NewCompanyCodeAsync();
    Task<ResultMessage> DeleteCompanyAsync(int id);
}
