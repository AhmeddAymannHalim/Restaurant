using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class InsuranceCategoryService : IInsuranceCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InsuranceCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<InsuranceCategoryViewModel>> GetPagedInsuranceCategoriesAsync(PageRequest request)
    {
        var result = await _unitOfWork.InsuranceCategories.GetPagedInsuranceCategoriesAsync(request);

        return PaginatedResult<InsuranceCategoryViewModel>.Create(_mapper.Map<List<InsuranceCategoryViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<InsuranceCategoryViewModel> GetInsuranceCategoryByIdAsync(int id)
    {
        var entity = await _unitOfWork.InsuranceCategories.GetInsuranceCategoryByIdAsync(id);

        return _mapper.Map<InsuranceCategoryViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertInsuranceCategoryAsync(InsuranceCategoryViewModel model)
    {
        var entity = _mapper.Map<InsuranceCategory>(model);

        return await _unitOfWork.InsuranceCategories.UpsertInsuranceCategoryAsync(entity);
    }

    public async Task<ResultMessage> DeleteInsuranceCategoryAsync(int id)
    {
        return await _unitOfWork.InsuranceCategories.DeleteInsuranceCategoryAsync(id);
    }

    public async Task<string> NewInsuranceCategoryCodeAsync()
    {
        return await _unitOfWork.InsuranceCategories.NewInsuranceCategoryCodeAsync();
    }
}
