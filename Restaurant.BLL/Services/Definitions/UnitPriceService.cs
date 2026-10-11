using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class UnitPriceService : IUnitPriceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UnitPriceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<UnitPriceViewModel>> GetPagedUnitPricesAsync(PageRequest request)
    {
        var result = await _unitOfWork.UnitPrices.GetPagedUnitPricesAsync(request);

        return PaginatedResult<UnitPriceViewModel>.Create(_mapper.Map<List<UnitPriceViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<UnitPriceViewModel> GetUnitPriceByIdAsync(int id)
    {
        var entity = await _unitOfWork.UnitPrices.GetUnitPriceByIdAsync(id);

        return _mapper.Map<UnitPriceViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertUnitPriceAsync(UnitPriceViewModel model)
    {
        var entity = _mapper.Map<UnitPrice>(model);

        return await _unitOfWork.UnitPrices.UpsertUnitPriceAsync(entity);
    }

    public async Task<ResultMessage> DeleteUnitPriceAsync(int id)
    {
        return await _unitOfWork.UnitPrices.DeleteUnitPriceAsync(id);
    }
}
