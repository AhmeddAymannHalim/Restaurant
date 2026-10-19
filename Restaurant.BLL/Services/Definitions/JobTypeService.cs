using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class JobTypeService : IJobTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public JobTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<JobTypeViewModel>> GetPagedJobTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.JobTypes.GetPagedJobTypesAsync(request);

        return PaginatedResult<JobTypeViewModel>.Create(_mapper.Map<List<JobTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<JobTypeViewModel> GetJobTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.JobTypes.GetJobTypeByIdAsync(id);

        return _mapper.Map<JobTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertJobTypeAsync(JobTypeViewModel model)
    {
        var entity = _mapper.Map<JobType>(model);

        return await _unitOfWork.JobTypes.UpsertJobTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteJobTypeAsync(int id)
    {
        return await _unitOfWork.JobTypes.DeleteJobTypeAsync(id);
    }

    public async Task<string> NewJobTypeCodeAsync()
    {
        return await _unitOfWork.JobTypes.NewJobTypeCodeAsync();
    }
}
