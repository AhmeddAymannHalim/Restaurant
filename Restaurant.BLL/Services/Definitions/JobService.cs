using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class JobService : IJobService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public JobService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<JobViewModel>> GetPagedJobsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Jobs.GetPagedJobsAsync(request);

        return PaginatedResult<JobViewModel>.Create(_mapper.Map<List<JobViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<JobViewModel> GetJobByIdAsync(int id)
    {
        var entity = await _unitOfWork.Jobs.GetJobByIdAsync(id);

        return _mapper.Map<JobViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertJobAsync(JobViewModel model)
    {
        var entity = _mapper.Map<Job>(model);

        return await _unitOfWork.Jobs.UpsertJobAsync(entity);
    }

    public async Task<ResultMessage> DeleteJobAsync(int id)
    {
        return await _unitOfWork.Jobs.DeleteJobAsync(id);
    }

    public async Task<string> NewJobCodeAsync()
    {
        return await _unitOfWork.Jobs.NewJobCodeAsync();
    }
}
