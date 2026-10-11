using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ExpenseViewModel>> GetPagedExpensesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Expenses.GetPagedExpensesAsync(request);

        return PaginatedResult<ExpenseViewModel>.Create(_mapper.Map<List<ExpenseViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<ExpenseViewModel> GetExpenseByIdAsync(int id)
    {
        var entity = await _unitOfWork.Expenses.GetExpenseByIdAsync(id);

        return _mapper.Map<ExpenseViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertExpenseAsync(ExpenseViewModel model)
    {
        var entity = _mapper.Map<Expense>(model);

        return await _unitOfWork.Expenses.UpsertExpenseAsync(entity);
    }

    public async Task<ResultMessage> DeleteExpenseAsync(int id)
    {
        return await _unitOfWork.Expenses.DeleteExpenseAsync(id);
    }
}
