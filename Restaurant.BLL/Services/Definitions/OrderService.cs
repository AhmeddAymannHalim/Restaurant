using PaymentMethod = Shared.Enums.PaymentMethod;
using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<OrderViewModel>> GetPagedOrdersAsync(PageRequest request, string userId)
    {
        var result = await _unitOfWork.Orders.GetPagedOrdersAsync(request, userId);

        return PaginatedResult<OrderViewModel>.Create(_mapper.Map<List<OrderViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<OrderViewModel> GetOrderByIdAsync(int id)
    {
        return _mapper.Map<OrderViewModel>(await _unitOfWork.Orders.GetOrderByIdAsync(id));
    }

    public async Task<OrderViewModel> GetNewOrderAsync()
    {
        var model = _mapper.Map<OrderViewModel>(await _unitOfWork.Orders.GetSettingsAsync());
        model.OrderCode = await _unitOfWork.Orders.GetNextCodeAsync();
        model.Menu = _mapper.Map<List<CashierMenuItemViewModel>>(await _unitOfWork.Orders.GetCashierMenuAsync());

        return model;
    }

    public async Task<OrderViewModel> GetNewOrderFromReservationAsync(int tableId, int? reservationId)
    {
        var model = await GetNewOrderAsync();
        model.Type = OrderType.Hall;
        model.TableId = tableId;

        if (reservationId.HasValue)
        {
            var reservation = await _unitOfWork.TableReservations.GetReservationByIdAsync(reservationId.Value);
            if (reservation != null)
            {
                model.CustomerName = reservation.CustomerName;
                model.PhoneNumber = reservation.PhoneNumber;
            }
        }

        return model;
    }

    public async Task<IEnumerable<LookupItem>> GetPendingTablesAsync(int exceptTableId)
    {
        return _mapper.Map<List<LookupItem>>(await _unitOfWork.Orders.GetPendingTablesAsync(exceptTableId));
    }

    public async Task<ResultMessage> CreateOrderAsync(OrderViewModel model, string userId)
    {
        var order = _mapper.Map<Order>(model);

        return await _unitOfWork.Orders.CreateOrderAsync(order, userId);
    }

    public async Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method)
    {
        return await _unitOfWork.Orders.PayOrderAsync(id, userId, method);
    }

    public async Task<ResultMessage> MoveItemAsync(OrderMoveViewModel model)
    {
        return await _unitOfWork.Orders.MoveItemAsync(model.OrderMenuItemId, model.TargetTableId);
    }

    public async Task<ResultMessage> MergeOrderAsync(OrderMoveViewModel model)
    {
        return await _unitOfWork.Orders.MergeOrderAsync(model.Id, model.TargetTableId);
    }

    public async Task<ResultMessage> DeleteOrderAsync(int id)
    {
        return await _unitOfWork.Orders.DeleteOrderAsync(id);
    }

    public async Task<IEnumerable<OrderViewModel>> GetKitchenOrdersAsync()
    {
        return _mapper.Map<List<OrderViewModel>>(await _unitOfWork.Orders.GetKitchenOrdersAsync());
    }

    public async Task<ResultMessage> CompleteOrderAsync(int id)
    {
        return await _unitOfWork.Orders.CompleteOrderAsync(id);
    }

}
