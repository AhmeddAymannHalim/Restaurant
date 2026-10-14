using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IInsuranceCompanyRepository
{
    Task<PaginatedResult<InsuranceCompany>> GetPagedInsuranceCompaniesAsync(PageRequest request);
    Task<InsuranceCompany> GetInsuranceCompanyByIdAsync(int id);
    Task<ResultMessage> UpsertInsuranceCompanyAsync(InsuranceCompany entity);
    Task<ResultMessage> DeleteInsuranceCompanyAsync(int id);
}
