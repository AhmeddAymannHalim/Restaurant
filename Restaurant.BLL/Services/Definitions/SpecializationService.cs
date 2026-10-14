using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SpecializationService : ISpecializationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SpecializationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SpecializationViewModel>> GetPagedSpecializationsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Specializations.GetPagedSpecializationsAsync(request);

        return PaginatedResult<SpecializationViewModel>.Create(_mapper.Map<List<SpecializationViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<SpecializationViewModel> GetSpecializationByIdAsync(int id)
    {
        var entity = await _unitOfWork.Specializations.GetSpecializationByIdAsync(id);

        return _mapper.Map<SpecializationViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertSpecializationAsync(SpecializationViewModel model)
    {
        var entity = _mapper.Map<Specialization>(model);

        return await _unitOfWork.Specializations.UpsertSpecializationAsync(entity);
    }

    public async Task<ResultMessage> DeleteSpecializationAsync(int id)
    {
        return await _unitOfWork.Specializations.DeleteSpecializationAsync(id);
    }

    public async Task<string> NewSpecializationCodeAsync()
    {
        return await _unitOfWork.Specializations.NewSpecializationCodeAsync();
    }
}
