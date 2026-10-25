using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SellTypeService : ISellTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SellTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SellTypeViewModel>> GetPagedSellTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.SellTypes.GetPagedSellTypesAsync(request);

        return PaginatedResult<SellTypeViewModel>.Create(_mapper.Map<List<SellTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<SellTypeViewModel> GetSellTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.SellTypes.GetSellTypeByIdAsync(id);

        return _mapper.Map<SellTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertSellTypeAsync(SellTypeViewModel model)
    {
        var entity = _mapper.Map<SellType>(model);

        return await _unitOfWork.SellTypes.UpsertSellTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteSellTypeAsync(int id)
    {
        return await _unitOfWork.SellTypes.DeleteSellTypeAsync(id);
    }
}
