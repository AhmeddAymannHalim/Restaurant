using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class StockMovementService : IStockMovementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockMovementService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<StockMovementViewModel>> GetPagedAsync(PageRequest request, int? storeId, int? productId)
    {
        var result = await _unitOfWork.StockMovements.GetPagedAsync(request, storeId, productId);

        return PaginatedResult<StockMovementViewModel>.Create(_mapper.Map<List<StockMovementViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }
}
