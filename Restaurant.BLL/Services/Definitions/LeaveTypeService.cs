using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeaveTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<LeaveTypeViewModel>> GetPagedLeaveTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.LeaveTypes.GetPagedLeaveTypesAsync(request);

        return PaginatedResult<LeaveTypeViewModel>.Create(_mapper.Map<List<LeaveTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<LeaveTypeViewModel> GetLeaveTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.LeaveTypes.GetLeaveTypeByIdAsync(id);

        return _mapper.Map<LeaveTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertLeaveTypeAsync(LeaveTypeViewModel model)
    {
        var entity = _mapper.Map<LeaveType>(model);

        return await _unitOfWork.LeaveTypes.UpsertLeaveTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteLeaveTypeAsync(int id)
    {
        return await _unitOfWork.LeaveTypes.DeleteLeaveTypeAsync(id);
    }

    public async Task<string> NewLeaveTypeCodeAsync()
    {
        return await _unitOfWork.LeaveTypes.NewLeaveTypeCodeAsync();
    }
}
