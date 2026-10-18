using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class TableReservationService : ITableReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TableReservationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TableReservationViewModel>> GetPagedReservationsAsync(PageRequest request, ReservationStatus? status)
    {
        var result = await _unitOfWork.TableReservations.GetPagedReservationsAsync(request, status);

        return PaginatedResult<TableReservationViewModel>.Create(
            _mapper.Map<List<TableReservationViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<TableReservationViewModel> GetReservationByIdAsync(int id)
    {
        return _mapper.Map<TableReservationViewModel>(await _unitOfWork.TableReservations.GetReservationByIdAsync(id));
    }

    public async Task<List<TableSlotViewModel>> GetTableSlotsAsync(DateTime when, int guests)
    {
        return _mapper.Map<List<TableSlotViewModel>>(await _unitOfWork.TableReservations.GetTableSlotsAsync(when, guests));
    }

    public async Task<ResultMessage> BookAsync(ReservationCreateViewModel model, string customerId)
    {
        return await _unitOfWork.TableReservations.BookAsync(_mapper.Map<TableReservation>(model), customerId);
    }

    public async Task<ResultMessage> UpsertReservationAsync(TableReservationViewModel model)
    {
        return await _unitOfWork.TableReservations.UpsertReservationAsync(_mapper.Map<TableReservation>(model));
    }

    public async Task<ResultMessage> SetStatusAsync(int id, ReservationStatus status, string reason)
    {
        return await _unitOfWork.TableReservations.SetStatusAsync(id, status, reason);
    }

    public async Task<ResultMessage> ArriveAsync(int id)
    {
        return await _unitOfWork.TableReservations.ArriveAsync(id);
    }

    public async Task<ResultMessage> DeleteReservationAsync(int id)
    {
        return await _unitOfWork.TableReservations.DeleteReservationAsync(id);
    }
}
