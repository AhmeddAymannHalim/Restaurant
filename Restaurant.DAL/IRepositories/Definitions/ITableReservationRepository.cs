using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ITableReservationRepository
{
    Task<PaginatedResult<TableReservation>> GetPagedReservationsAsync(PageRequest request, ReservationStatus? status);
    Task<TableReservation> GetReservationByIdAsync(int id);
    Task<ResultMessage> UpsertReservationAsync(TableReservation entity);
    Task<ResultMessage> SetStatusAsync(int id, ReservationStatus status, string reason);
    Task<ResultMessage> ArriveAsync(int id);
    Task<ResultMessage> DeleteReservationAsync(int id);
}
