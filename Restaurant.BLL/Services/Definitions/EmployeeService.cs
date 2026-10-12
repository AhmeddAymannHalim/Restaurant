using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<EmployeeViewModel>> GetPagedEmployeesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Employees.GetPagedEmployeesAsync(request);

        return PaginatedResult<EmployeeViewModel>.Create(_mapper.Map<List<EmployeeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<EmployeeViewModel> GetEmployeeByIdAsync(int id)
    {
        var entity = await _unitOfWork.Employees.GetEmployeeByIdAsync(id);

        return _mapper.Map<EmployeeViewModel>(entity);
    }

    public async Task<string> NewEmployeeCodeAsync()
    {
        return await _unitOfWork.Employees.NewEmployeeCodeAsync();
    }

    public async Task<ResultMessage> UpsertEmployeeAsync(EmployeeViewModel model)
    {
        var entity = _mapper.Map<Employee>(model);

        return await _unitOfWork.Employees.UpsertEmployeeAsync(entity, model.ImageFile);
    }

    public async Task<ResultMessage> DeleteEmployeeAsync(int id)
    {
        return await _unitOfWork.Employees.DeleteEmployeeAsync(id);
    }
}
