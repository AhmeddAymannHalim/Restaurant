using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class StoreConvertService : IStoreConvertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StoreConvertService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<StoreConvertViewModel>> GetPagedConvertsAsync(PageRequest request)
    {
        var result = await _unitOfWork.StoreConverts.GetPagedConvertsAsync(request);

        return PaginatedResult<StoreConvertViewModel>.Create(_mapper.Map<List<StoreConvertViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<StoreConvertViewModel> GetConvertByIdAsync(int id)
    {
        return _mapper.Map<StoreConvertViewModel>(await _unitOfWork.StoreConverts.GetConvertByIdAsync(id));
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _unitOfWork.StoreConverts.GetNextCodeAsync();
    }

    public async Task<ResultMessage> CreateConvertAsync(StoreConvertViewModel model)
    {
        return await _unitOfWork.StoreConverts.CreateConvertAsync(_mapper.Map<StoreConvert>(model));
    }

    public async Task<ResultMessage> DeleteConvertAsync(int id)
    {
        return await _unitOfWork.StoreConverts.DeleteConvertAsync(id);
    }
}
