using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class DeductionTypeService : IDeductionTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeductionTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<DeductionTypeViewModel>> GetPagedDeductionTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.DeductionTypes.GetPagedDeductionTypesAsync(request);

        return PaginatedResult<DeductionTypeViewModel>.Create(_mapper.Map<List<DeductionTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<DeductionTypeViewModel> GetDeductionTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.DeductionTypes.GetDeductionTypeByIdAsync(id);

        return _mapper.Map<DeductionTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertDeductionTypeAsync(DeductionTypeViewModel model)
    {
        var entity = _mapper.Map<DeductionType>(model);

        return await _unitOfWork.DeductionTypes.UpsertDeductionTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteDeductionTypeAsync(int id)
    {
        return await _unitOfWork.DeductionTypes.DeleteDeductionTypeAsync(id);
    }

    public async Task<string> NewDeductionTypeCodeAsync()
    {
        return await _unitOfWork.DeductionTypes.NewDeductionTypeCodeAsync();
    }
}
