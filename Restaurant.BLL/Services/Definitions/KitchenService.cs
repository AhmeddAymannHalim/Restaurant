using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class KitchenService : IKitchenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public KitchenService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<KitchenViewModel>> GetPagedKitchensAsync(PageRequest request)
    {
        var result = await _unitOfWork.Kitchens.GetPagedKitchensAsync(request);

        return PaginatedResult<KitchenViewModel>.Create(_mapper.Map<List<KitchenViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<KitchenViewModel> GetKitchenByIdAsync(int id)
    {
        var entity = await _unitOfWork.Kitchens.GetKitchenByIdAsync(id);

        return _mapper.Map<KitchenViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertKitchenAsync(KitchenViewModel model)
    {
        var entity = _mapper.Map<Kitchen>(model);

        return await _unitOfWork.Kitchens.UpsertKitchenAsync(entity);
    }

    public async Task<ResultMessage> DeleteKitchenAsync(int id)
    {
        return await _unitOfWork.Kitchens.DeleteKitchenAsync(id);
    }
}
