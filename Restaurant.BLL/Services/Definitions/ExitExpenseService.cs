using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ExitExpenseService : IExitExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExitExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ExitExpenseViewModel>> GetPagedExitExpensesAsync(PageRequest request)
    {
        var result = await _unitOfWork.ExitExpenses.GetPagedExitExpensesAsync(request);

        return PaginatedResult<ExitExpenseViewModel>.Create(_mapper.Map<List<ExitExpenseViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ExitExpenseViewModel> GetExitExpenseByIdAsync(int id)
    {
        var entity = await _unitOfWork.ExitExpenses.GetExitExpenseByIdAsync(id);

        return _mapper.Map<ExitExpenseViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertExitExpenseAsync(ExitExpenseViewModel model, string userId)
    {
        var entity = _mapper.Map<ExitExpense>(model);

        return await _unitOfWork.ExitExpenses.UpsertExitExpenseAsync(entity, userId);
    }

    public async Task<ResultMessage> DeleteExitExpenseAsync(int id)
    {
        return await _unitOfWork.ExitExpenses.DeleteExitExpenseAsync(id);
    }
}
