using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<DepartmentViewModel>> GetPagedDepartmentsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Departments.GetPagedDepartmentsAsync(request);

        return PaginatedResult<DepartmentViewModel>.Create(_mapper.Map<List<DepartmentViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<DepartmentViewModel> GetDepartmentByIdAsync(int id)
    {
        var entity = await _unitOfWork.Departments.GetDepartmentByIdAsync(id);

        return _mapper.Map<DepartmentViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertDepartmentAsync(DepartmentViewModel model)
    {
        var entity = _mapper.Map<Department>(model);

        return await _unitOfWork.Departments.UpsertDepartmentAsync(entity);
    }

    public async Task<ResultMessage> DeleteDepartmentAsync(int id)
    {
        return await _unitOfWork.Departments.DeleteDepartmentAsync(id);
    }
}
