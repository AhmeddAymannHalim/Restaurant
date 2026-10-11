using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ChefService : IChefService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChefService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ChefViewModel>> GetPagedChefsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Chefs.GetPagedChefsAsync(request);

        return PaginatedResult<ChefViewModel>.Create(_mapper.Map<List<ChefViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<ChefViewModel> GetChefByIdAsync(int id)
    {
        var entity = await _unitOfWork.Chefs.GetChefByIdAsync(id);

        return _mapper.Map<ChefViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertChefAsync(ChefViewModel model)
    {
        var entity = _mapper.Map<ChefItem>(model);

        return await _unitOfWork.Chefs.UpsertChefAsync(entity, model.ImageFile);
    }

    public async Task<ResultMessage> DeleteChefAsync(int id)
    {
        return await _unitOfWork.Chefs.DeleteChefAsync(id);
    }
}
