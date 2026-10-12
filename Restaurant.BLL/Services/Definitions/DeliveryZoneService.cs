using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class DeliveryZoneService : IDeliveryZoneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeliveryZoneService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<DeliveryZoneViewModel>> GetPagedDeliveryZonesAsync(PageRequest request)
    {
        var result = await _unitOfWork.DeliveryZones.GetPagedDeliveryZonesAsync(request);

        return PaginatedResult<DeliveryZoneViewModel>.Create(_mapper.Map<List<DeliveryZoneViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<List<DeliveryZoneViewModel>> GetAllDeliveryZonesAsync()
    {
        return _mapper.Map<List<DeliveryZoneViewModel>>(await _unitOfWork.DeliveryZones.GetAllDeliveryZonesAsync());
    }

    public async Task<DeliveryZoneViewModel> GetDeliveryZoneByIdAsync(int id)
    {
        var entity = await _unitOfWork.DeliveryZones.GetDeliveryZoneByIdAsync(id);

        return _mapper.Map<DeliveryZoneViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertDeliveryZoneAsync(DeliveryZoneViewModel model)
    {
        var entity = _mapper.Map<DeliveryZone>(model);

        return await _unitOfWork.DeliveryZones.UpsertDeliveryZoneAsync(entity);
    }

    public async Task<ResultMessage> DeleteDeliveryZoneAsync(int id)
    {
        return await _unitOfWork.DeliveryZones.DeleteDeliveryZoneAsync(id);
    }
}
