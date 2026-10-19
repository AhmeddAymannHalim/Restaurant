using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IInsuranceCompanyService
{
    Task<PaginatedResult<InsuranceCompanyViewModel>> GetPagedInsuranceCompaniesAsync(PageRequest request);
    Task<InsuranceCompanyViewModel> GetInsuranceCompanyByIdAsync(int id);
    Task<ResultMessage> UpsertInsuranceCompanyAsync(InsuranceCompanyViewModel model);
    Task<ResultMessage> DeleteInsuranceCompanyAsync(int id);
    Task<string> NewInsuranceCompanyCodeAsync();
}
