using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BranchViewModel>> GetPagedBranchesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Branches.GetPagedBranchesAsync(request);

        return PaginatedResult<BranchViewModel>.Create(_mapper.Map<List<BranchViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<BranchViewModel> GetBranchByIdAsync(int id)
    {
        var entity = await _unitOfWork.Branches.GetBranchByIdAsync(id);

        return _mapper.Map<BranchViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertBranchAsync(BranchViewModel model)
    {
        var entity = _mapper.Map<Branch>(model);

        return await _unitOfWork.Branches.UpsertBranchAsync(entity);
    }

    public async Task<ResultMessage> DeleteBranchAsync(int id)
    {
        return await _unitOfWork.Branches.DeleteBranchAsync(id);
    }

    public async Task<string> NewBranchCodeAsync()
    {
        return await _unitOfWork.Branches.NewBranchCodeAsync();
    }
}
