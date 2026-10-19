using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StoreService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<StoreViewModel>> GetPagedStoresAsync(PageRequest request)
    {
        var result = await _unitOfWork.Stores.GetPagedStoresAsync(request);

        return PaginatedResult<StoreViewModel>.Create(_mapper.Map<List<StoreViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<StoreViewModel> GetStoreByIdAsync(int id)
    {
        var entity = await _unitOfWork.Stores.GetStoreByIdAsync(id);

        return _mapper.Map<StoreViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertStoreAsync(StoreViewModel model)
    {
        var entity = _mapper.Map<Store>(model);

        return await _unitOfWork.Stores.UpsertStoreAsync(entity);
    }

    public async Task<ResultMessage> DeleteStoreAsync(int id)
    {
        return await _unitOfWork.Stores.DeleteStoreAsync(id);
    }

    public async Task<string> NewStoreCodeAsync()
    {
        return await _unitOfWork.Stores.NewStoreCodeAsync();
    }
}
