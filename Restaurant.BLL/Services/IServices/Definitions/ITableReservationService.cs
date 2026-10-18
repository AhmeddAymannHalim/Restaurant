using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ITableReservationService
{
    Task<PaginatedResult<TableReservationViewModel>> GetPagedReservationsAsync(PageRequest request, ReservationStatus? status);
    Task<TableReservationViewModel> GetReservationByIdAsync(int id);
    Task<List<TableSlotViewModel>> GetTableSlotsAsync(DateTime when, int guests);
    Task<ResultMessage> BookAsync(ReservationCreateViewModel model, string customerId);
    Task<ResultMessage> UpsertReservationAsync(TableReservationViewModel model);
    Task<ResultMessage> SetStatusAsync(int id, ReservationStatus status, string reason);
    Task<ResultMessage> ArriveAsync(int id);
    Task<ResultMessage> DeleteReservationAsync(int id);
}
