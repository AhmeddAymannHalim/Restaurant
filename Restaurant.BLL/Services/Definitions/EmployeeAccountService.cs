using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class EmployeeAccountService : IEmployeeAccountService
{
    private readonly IEmployeeAccountRepository _repository;
    private readonly IMapper _mapper;

    public EmployeeAccountService(IEmployeeAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<EmployeeAccountViewModel>> GetPagedAccountsAsync(PageRequest request)
    {
        var result = await _repository.GetPagedAccountsAsync(request);

        return PaginatedResult<EmployeeAccountViewModel>.Create(_mapper.Map<List<EmployeeAccountViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<EmployeeAccountViewModel> GetAccountByIdAsync(string id)
    {
        var entity = await _repository.GetAccountByIdAsync(id);

        return _mapper.Map<EmployeeAccountViewModel>(entity);
    }

    public async Task<EmployeeAccountViewModel> GetAccountByEmployeeIdAsync(int employeeId)
    {
        var entity = await _repository.GetAccountByEmployeeIdAsync(employeeId);
        if (entity is not null)
        {
            return _mapper.Map<EmployeeAccountViewModel>(entity);
        }

        var employee = await _repository.GetEmployeeByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        return new EmployeeAccountViewModel
        {
            EmployeeId = employee.EmployeeId,
            Code = employee.Code,
            FirstName = employee.FirstName,
            LastName = employee.LastName
        };
    }

    public async Task<IEnumerable<LookupItem>> GetEmployeesWithoutAccountAsync()
    {
        var employees = await _repository.GetEmployeesWithoutAccountAsync();

        return employees.Select(a => new LookupItem
        {
            Id = a.EmployeeId,
            ArName = a.FullName,
            EnName = a.FullName
        });
    }

    public async Task<ResultMessage> UpsertAccountAsync(EmployeeAccountViewModel model)
    {
        var entity = _mapper.Map<ApplicationUser>(model);

        return await _repository.UpsertAccountAsync(entity, model.EmployeeId, model.Password);
    }
}
