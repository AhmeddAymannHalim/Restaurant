using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CompanyViewModel>> GetPagedCompaniesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Companies.GetPagedCompaniesAsync(request);

        return PaginatedResult<CompanyViewModel>.Create(_mapper.Map<List<CompanyViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<CompanyViewModel> GetCompanyByIdAsync(int id)
    {
        var entity = await _unitOfWork.Companies.GetCompanyByIdAsync(id);

        return _mapper.Map<CompanyViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertCompanyAsync(CompanyViewModel model)
    {
        var entity = _mapper.Map<Company>(model);

        return await _unitOfWork.Companies.UpsertCompanyAsync(entity);
    }

    public async Task<ResultMessage> DeleteCompanyAsync(int id)
    {
        return await _unitOfWork.Companies.DeleteCompanyAsync(id);
    }
}
