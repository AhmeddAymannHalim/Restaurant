using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ScroungeService : IScroungeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ScroungeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ScroungeViewModel>> GetPagedScroungesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Scrounges.GetPagedScroungesAsync(request);

        return PaginatedResult<ScroungeViewModel>.Create(_mapper.Map<List<ScroungeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ScroungeViewModel> GetScroungeByIdAsync(int id)
    {
        var entity = await _unitOfWork.Scrounges.GetScroungeByIdAsync(id);

        return _mapper.Map<ScroungeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertScroungeAsync(ScroungeViewModel model, string userId)
    {
        var entity = _mapper.Map<Scrounge>(model);

        return await _unitOfWork.Scrounges.UpsertScroungeAsync(entity, userId);
    }

    public async Task<ResultMessage> DeleteScroungeAsync(int id)
    {
        return await _unitOfWork.Scrounges.DeleteScroungeAsync(id);
    }
}
