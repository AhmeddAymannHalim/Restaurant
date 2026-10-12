using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class OnlineOrderService : IOnlineOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OnlineOrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OnlineMenuItemViewModel>> GetMenuAsync()
    {
        return _mapper.Map<List<OnlineMenuItemViewModel>>(await _unitOfWork.OnlineOrders.GetMenuAsync());
    }

    public async Task<ResultMessage> CreateOrderAsync(OnlineOrderCreateViewModel model, string customerId)
    {
        return await _unitOfWork.OnlineOrders.CreateOrderAsync(_mapper.Map<OnlineOrder>(model), customerId);
    }

    public async Task<IEnumerable<PendingOnlineOrderViewModel>> GetCustomerOrdersAsync(string customerId)
    {
        return _mapper.Map<List<PendingOnlineOrderViewModel>>(await _unitOfWork.OnlineOrders.GetCustomerOrdersAsync(customerId));
    }

    public async Task<PendingOnlineOrderViewModel> GetOrderAsync(int id)
    {
        return _mapper.Map<PendingOnlineOrderViewModel>(await _unitOfWork.OnlineOrders.GetOrderAsync(id));
    }

    public async Task<ResultMessage> CancelOrderAsync(int id, string userId, bool isStaff)
    {
        return await _unitOfWork.OnlineOrders.CancelOrderAsync(id, userId, isStaff);
    }
}
