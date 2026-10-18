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

    public async Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId)
    {
        return await _unitOfWork.Orders.GetShiftDashboardAsync(userId);
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
        var settings = await _unitOfWork.Orders.GetSettingsAsync();
        var menu = _mapper.Map<List<CashierMenuItemViewModel>>(await _unitOfWork.Orders.GetCashierMenuAsync());

        return new OrderViewModel
        {
            OrderCode = await _unitOfWork.Orders.GetNextCodeAsync(),
            CustomerName = string.Empty,
            TaxPercent = settings.OrderTax,
            DefaultService = settings.OrderService,
            Service = settings.OrderService,
            Menu = menu
        };
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

    public async Task<OrderViewModel> GetNewOrderFromOnlineAsync(int onlineOrderId)
    {
        var online = await _unitOfWork.Orders.GetOnlineOrderAsync(onlineOrderId);
        if (online is null)
        {
            return null;
        }

        var model = await GetNewOrderAsync();
        model.OnlineOrderId = online.Id;
        model.Type = online.TableId.HasValue ? OrderType.Hall : OrderType.Delivery_Online;
        model.TableId = online.TableId;
        model.CustomerName = online.CustomerName;
        model.Address = online.Address;
        model.PhoneNumber = online.PhoneNumber;
        model.DeliveryZoneId = online.DeliveryZoneId;
        model.DeliveryFee = online.DeliveryFee;
        model.DeliveryZoneArName = online.DeliveryZone?.ArName;
        model.DeliveryZoneEnName = online.DeliveryZone?.EnName;
        model.Items = _mapper.Map<List<OrderItemViewModel>>(online.OnlineOrderMenuItems);

        return model;
    }

    public async Task<IEnumerable<LookupItem>> GetPendingTablesAsync(int exceptTableId)
    {
        return _mapper.Map<List<LookupItem>>(await _unitOfWork.Orders.GetPendingTablesAsync(exceptTableId));
    }

    public async Task<ResultMessage> CreateOrderAsync(OrderViewModel model, string userId)
    {
        var order = new Order
        {
            Type = model.Type,
            Cash = model.Cash,
            PaymentMethod = model.PaymentMethod,
            OnAccount = model.OnAccount,
            CustomerName = model.CustomerName,
            CustomerId = model.CustomerId,
            TableId = model.TableId,
            EmployeeId = model.EmployeeId,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            DeliveryZoneId = model.DeliveryZoneId,
            Bill = new BillMaster { BillDiscountValue = model.Discount },
            Lines = model.Items.Select(a => new OrderMenuItem
            {
                MenuItemId = a.MenuItemId,
                BillDetail = new BillDetail { Quantity = a.Quantity }
            }).ToList()
        };

        return await _unitOfWork.Orders.CreateOrderAsync(order, model.OnlineOrderId, userId);
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

    public async Task<IEnumerable<IncompleteCashOrderViewModel>> GetIncompleteCashOrdersAsync(string userId)
    {
        return _mapper.Map<List<IncompleteCashOrderViewModel>>(await _unitOfWork.Orders.GetIncompleteCashOrdersAsync(userId));
    }

    public async Task<ResultMessage> FinishShiftAsync(string userId, decimal countedCash)
    {
        return await _unitOfWork.Orders.FinishShiftAsync(userId, countedCash);
    }

    public async Task<IEnumerable<OrderViewModel>> GetKitchenOrdersAsync()
    {
        return _mapper.Map<List<OrderViewModel>>(await _unitOfWork.Orders.GetKitchenOrdersAsync());
    }

    public async Task<ResultMessage> CompleteOrderAsync(int id)
    {
        return await _unitOfWork.Orders.CompleteOrderAsync(id);
    }

    public async Task<bool> VerifyShiftPasswordAsync(string password)
    {
        return await _unitOfWork.Orders.VerifyShiftPasswordAsync(password);
    }

    public async Task<IEnumerable<PendingOnlineOrderViewModel>> GetPendingOnlineOrdersAsync()
    {
        return _mapper.Map<List<PendingOnlineOrderViewModel>>(await _unitOfWork.Orders.GetPendingOnlineOrdersAsync());
    }

    public async Task<ResultMessage> CancelOnlineOrderAsync(int id)
    {
        return await _unitOfWork.Orders.CancelOnlineOrderAsync(id);
    }

    public async Task<ResultMessage> RejectOnlineOrderAsync(int id, string reason)
    {
        return await _unitOfWork.Orders.RejectOnlineOrderAsync(id, reason);
    }
}
