using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ClassificationService : IClassificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClassificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ClassificationViewModel>> GetPagedClassificationsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Classifications.GetPagedClassificationsAsync(request);

        return PaginatedResult<ClassificationViewModel>.Create(_mapper.Map<List<ClassificationViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ClassificationViewModel> GetClassificationByIdAsync(int id)
    {
        var entity = await _unitOfWork.Classifications.GetClassificationByIdAsync(id);

        return _mapper.Map<ClassificationViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertClassificationAsync(ClassificationViewModel model)
    {
        var entity = _mapper.Map<Classification>(model);

        return await _unitOfWork.Classifications.UpsertClassificationAsync(entity);
    }

    public async Task<ResultMessage> DeleteClassificationAsync(int id)
    {
        return await _unitOfWork.Classifications.DeleteClassificationAsync(id);
    }

    public async Task<string> NewClassificationCodeAsync()
    {
        return await _unitOfWork.Classifications.NewClassificationCodeAsync();
    }
}
