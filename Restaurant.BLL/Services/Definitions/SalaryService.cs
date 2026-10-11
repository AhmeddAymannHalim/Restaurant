using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalaryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SalaryViewModel>> GetPagedSalariesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Salaries.GetPagedSalariesAsync(request);

        return PaginatedResult<SalaryViewModel>.Create(_mapper.Map<List<SalaryViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<SalaryViewModel> GetSalaryByIdAsync(int id)
    {
        var entity = await _unitOfWork.Salaries.GetSalaryByIdAsync(id);

        return _mapper.Map<SalaryViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertSalaryAsync(SalaryViewModel model, string userId)
    {
        var entity = _mapper.Map<Salary>(model);

        return await _unitOfWork.Salaries.UpsertSalaryAsync(entity, userId);
    }

    public async Task<ResultMessage> DeleteSalaryAsync(int id)
    {
        return await _unitOfWork.Salaries.DeleteSalaryAsync(id);
    }

    public async Task<string> NewSalaryCodeAsync()
    {
        return await _unitOfWork.Salaries.NewSalaryCodeAsync();
    }

    public async Task<EmployeeExpenseViewModel> GetEmployeeExpensesAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.Salaries.GetEmployeeExpensesAsync(employeeId, startDate, endDate);
    }
}
