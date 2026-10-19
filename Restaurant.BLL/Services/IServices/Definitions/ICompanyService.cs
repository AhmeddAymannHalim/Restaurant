using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICompanyService
{
    Task<PaginatedResult<CompanyViewModel>> GetPagedCompaniesAsync(PageRequest request);
    Task<CompanyViewModel> GetCompanyByIdAsync(int id);
    Task<ResultMessage> UpsertCompanyAsync(CompanyViewModel model);
    Task<ResultMessage> DeleteCompanyAsync(int id);
    Task<string> NewCompanyCodeAsync();
}
