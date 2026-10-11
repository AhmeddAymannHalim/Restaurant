using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class DepositService : IDepositService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepositService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<DepositViewModel>> GetPagedDepositsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Deposits.GetPagedDepositsAsync(request);

        return PaginatedResult<DepositViewModel>.Create(_mapper.Map<List<DepositViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<DepositViewModel> GetDepositByIdAsync(int id)
    {
        var entity = await _unitOfWork.Deposits.GetDepositByIdAsync(id);

        return _mapper.Map<DepositViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertDepositAsync(DepositViewModel model, string userId)
    {
        var entity = _mapper.Map<Deposit>(model);

        return await _unitOfWork.Deposits.UpsertDepositAsync(entity, userId);
    }

    public async Task<ResultMessage> DeleteDepositAsync(int id)
    {
        return await _unitOfWork.Deposits.DeleteDepositAsync(id);
    }

    public async Task<string> NewDepositCodeAsync()
    {
        return await _unitOfWork.Deposits.NewDepositCodeAsync();
    }
}
