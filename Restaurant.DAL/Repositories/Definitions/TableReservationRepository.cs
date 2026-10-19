using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class TableReservationRepository : ITableReservationRepository
{
    // a table is taken for 2 hours around the reservation time
    private const int SlotMinutes = 120;

    private readonly ApplicationDbContext _context;

    public TableReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<TableReservation>> GetPagedReservationsAsync(PageRequest request, ReservationStatus? status)
    {
        var query = _context.TableReservations.AsNoTracking().Include(a => a.Table).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.CustomerName.ToLower().Contains(search)
                || a.PhoneNumber.Contains(search)
                || a.Table.ArName.ToLower().Contains(search)
                || a.Table.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Status == ReservationStatus.Pending ? 0 : 1)
            .ThenBy(a => a.ReservationDate)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<TableReservation>.Create(data, total, request);
    }

    public async Task<TableReservation> GetReservationByIdAsync(int id)
    {
        return await _context.TableReservations.AsNoTracking().Include(a => a.Table).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> UpsertReservationAsync(TableReservation entity)
    {
        try
        {
            entity.CustomerName = entity.CustomerName?.Trim();
            entity.PhoneNumber = entity.PhoneNumber?.Trim();
            entity.Email = string.IsNullOrWhiteSpace(entity.Email) ? null : entity.Email.Trim();
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes) ? null : entity.Notes.Trim();

            if (entity.ReservationDate == default)
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.DateRequired" };
            }

            if (string.IsNullOrWhiteSpace(entity.CustomerName))
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.NameRequired" };
            }

            var digits = (entity.PhoneNumber ?? string.Empty).Count(char.IsDigit);
            if (digits < 7 || digits > 15)
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.PhoneInvalid" };
            }

            if (entity.Email != null && !entity.Email.Contains('@'))
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.EmailInvalid" };
            }

            var table = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.TableId);
            if (table is null)
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.TableRequired" };
            }

            if (entity.GuestCount < 1)
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.GuestsRequired" };
            }

            if (entity.GuestCount > table.NumOfPersons)
            {
                return new ResultMessage { Success = false, Message = "Reservation.Error.TooManyGuests" };
            }

            // the same table can not have another open reservation around this time
            if (entity.Status == ReservationStatus.Pending || entity.Status == ReservationStatus.Confirmed)
            {
                var from = entity.ReservationDate.AddMinutes(-SlotMinutes);
                var to = entity.ReservationDate.AddMinutes(SlotMinutes);
                var tableTaken = await _context.TableReservations.AsNoTracking().AnyAsync(a => a.TableId == entity.TableId
                    && a.Id != entity.Id
                    && (a.Status == ReservationStatus.Pending || a.Status == ReservationStatus.Confirmed)
                    && a.ReservationDate > from
                    && a.ReservationDate < to);
                if (tableTaken)
                {
                    return new ResultMessage { Success = false, Message = "Reservation.Error.TableTaken" };
                }
            }

            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                await _context.TableReservations.AddAsync(entity);
            }
            else
            {
                var existing = await _context.TableReservations.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                entity.CreatedAt = existing.CreatedAt;
                entity.CustomerId = existing.CustomerId;
                if (entity.Status != ReservationStatus.Rejected)
                {
                    entity.RejectionReason = null;
                }

                _context.TableReservations.Update(entity);
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> SetStatusAsync(int id, ReservationStatus status, string reason)
    {
        try
        {
            var entity = await _context.TableReservations.FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (status == ReservationStatus.Confirmed && entity.Status != ReservationStatus.Confirmed)
            {
                var from = entity.ReservationDate.AddMinutes(-SlotMinutes);
                var to = entity.ReservationDate.AddMinutes(SlotMinutes);
                var tableTaken = await _context.TableReservations.AsNoTracking().AnyAsync(a => a.TableId == entity.TableId
                    && a.Id != entity.Id
                    && (a.Status == ReservationStatus.Pending || a.Status == ReservationStatus.Confirmed)
                    && a.ReservationDate > from
                    && a.ReservationDate < to);
                if (tableTaken)
                {
                    return new ResultMessage { Success = false, Message = "Reservation.Error.TableTaken" };
                }
            }

            entity.Status = status;
            entity.RejectionReason = status == ReservationStatus.Rejected && !string.IsNullOrWhiteSpace(reason)
                ? reason.Trim()
                : null;
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> ArriveAsync(int id)
    {
        try
        {
            var entity = await _context.TableReservations.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            entity.Status = ReservationStatus.Seated;
            entity.RejectionReason = null;
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true, Data = entity.TableId.ToString() };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> DeleteReservationAsync(int id)
    {
        try
        {
            var entity = await _context.TableReservations.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.TableReservations.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }
}
