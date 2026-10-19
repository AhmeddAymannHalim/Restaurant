using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class InsuranceCompanyService : IInsuranceCompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InsuranceCompanyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<InsuranceCompanyViewModel>> GetPagedInsuranceCompaniesAsync(PageRequest request)
    {
        var result = await _unitOfWork.InsuranceCompanies.GetPagedInsuranceCompaniesAsync(request);

        return PaginatedResult<InsuranceCompanyViewModel>.Create(_mapper.Map<List<InsuranceCompanyViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<InsuranceCompanyViewModel> GetInsuranceCompanyByIdAsync(int id)
    {
        var entity = await _unitOfWork.InsuranceCompanies.GetInsuranceCompanyByIdAsync(id);

        return _mapper.Map<InsuranceCompanyViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertInsuranceCompanyAsync(InsuranceCompanyViewModel model)
    {
        var entity = _mapper.Map<InsuranceCompany>(model);

        return await _unitOfWork.InsuranceCompanies.UpsertInsuranceCompanyAsync(entity);
    }

    public async Task<ResultMessage> DeleteInsuranceCompanyAsync(int id)
    {
        return await _unitOfWork.InsuranceCompanies.DeleteInsuranceCompanyAsync(id);
    }

    public async Task<string> NewInsuranceCompanyCodeAsync()
    {
        return await _unitOfWork.InsuranceCompanies.NewInsuranceCompanyCodeAsync();
    }
}
