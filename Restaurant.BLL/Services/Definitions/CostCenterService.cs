using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CostCenterService : ICostCenterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CostCenterService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CostCenterViewModel>> GetPagedCostCentersAsync(PageRequest request)
    {
        var result = await _unitOfWork.CostCenters.GetPagedCostCentersAsync(request);

        return PaginatedResult<CostCenterViewModel>.Create(_mapper.Map<List<CostCenterViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<CostCenterViewModel> GetCostCenterByIdAsync(int id)
    {
        var entity = await _unitOfWork.CostCenters.GetCostCenterByIdAsync(id);

        return _mapper.Map<CostCenterViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertCostCenterAsync(CostCenterViewModel model)
    {
        var entity = _mapper.Map<CostCenter>(model);

        return await _unitOfWork.CostCenters.UpsertCostCenterAsync(entity);
    }

    public async Task<ResultMessage> DeleteCostCenterAsync(int id)
    {
        return await _unitOfWork.CostCenters.DeleteCostCenterAsync(id);
    }

    public async Task<string> NewCostCenterCodeAsync()
    {
        return await _unitOfWork.CostCenters.NewCostCenterCodeAsync();
    }
}
