using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class TableReservationRepository : ITableReservationRepository
{
    private const int SlotMinutes = 120;
    private const int MinimumLeadMinutes = 30;
    private const int MaximumDaysAhead = 60;

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

    public async Task<List<TableSlot>> GetTableSlotsAsync(DateTime when, int guests)
    {
        var reserved = await ReservedTableIdsAsync(when, null);
        var tables = await _context.Tables.AsNoTracking().OrderBy(a => a.NumOfPersons).ThenBy(a => a.Id).ToListAsync();

        var slots = new List<TableSlot>();
        foreach (var table in tables)
        {
            var slot = new TableSlot();
            slot.Table = table;
            slot.IsReserved = reserved.Contains(table.Id);
            slot.TooSmall = guests > table.NumOfPersons;
            slots.Add(slot);
        }

        return slots;
    }

    public async Task<ResultMessage> BookAsync(TableReservation entity, string customerId)
    {
        try
        {
            entity.Id = 0;
            entity.CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId;
            entity.Status = ReservationStatus.Pending;
            entity.RejectionReason = null;
            entity.CreatedAt = DateTime.Now;

            var error = await ValidateAsync(entity, true);
            if (error != null)
            {
                return new ResultMessage { Success = false, Message = error };
            }

            await _context.TableReservations.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true, Data = entity.Id.ToString() };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> UpsertReservationAsync(TableReservation entity)
    {
        try
        {
            var error = await ValidateAsync(entity, false);
            if (error != null)
            {
                return new ResultMessage { Success = false, Message = error };
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
                var reserved = await ReservedTableIdsAsync(entity.ReservationDate, entity.Id);
                if (reserved.Contains(entity.TableId))
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

    private async Task<List<int>> ReservedTableIdsAsync(DateTime when, int? excludeId)
    {
        var from = when.AddMinutes(-SlotMinutes);
        var to = when.AddMinutes(SlotMinutes);

        return await _context.TableReservations
            .AsNoTracking()
            .Where(a => (a.Status == ReservationStatus.Pending || a.Status == ReservationStatus.Confirmed)
                && a.ReservationDate > from
                && a.ReservationDate < to
                && (excludeId == null || a.Id != excludeId))
            .Select(a => a.TableId)
            .Distinct()
            .ToListAsync();
    }

    private async Task<string> ValidateAsync(TableReservation entity, bool publicBooking)
    {
        entity.CustomerName = entity.CustomerName?.Trim();
        entity.PhoneNumber = entity.PhoneNumber?.Trim();
        entity.Email = string.IsNullOrWhiteSpace(entity.Email) ? null : entity.Email.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(entity.Notes) ? null : entity.Notes.Trim();

        if (entity.ReservationDate == default)
        {
            return "Reservation.Error.DateRequired";
        }

        if (string.IsNullOrWhiteSpace(entity.CustomerName))
        {
            return "Reservation.Error.NameRequired";
        }

        var digits = (entity.PhoneNumber ?? string.Empty).Count(char.IsDigit);
        if (digits < 7 || digits > 15)
        {
            return "Reservation.Error.PhoneInvalid";
        }

        if (entity.Email != null && !entity.Email.Contains('@'))
        {
            return "Reservation.Error.EmailInvalid";
        }

        var table = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.TableId);
        if (table is null)
        {
            return "Reservation.Error.TableRequired";
        }

        if (entity.GuestCount < 1)
        {
            return "Reservation.Error.GuestsRequired";
        }

        if (entity.GuestCount > table.NumOfPersons)
        {
            return "Reservation.Error.TooManyGuests";
        }

        if (publicBooking)
        {
            if (entity.ReservationDate < DateTime.Now.AddMinutes(MinimumLeadMinutes))
            {
                return "Reservation.Error.TooSoon";
            }

            if (entity.ReservationDate > DateTime.Now.AddDays(MaximumDaysAhead))
            {
                return "Reservation.Error.TooFar";
            }
        }

        if (entity.Status == ReservationStatus.Pending || entity.Status == ReservationStatus.Confirmed)
        {
            var reserved = await ReservedTableIdsAsync(entity.ReservationDate, entity.Id == 0 ? null : entity.Id);
            if (reserved.Contains(entity.TableId))
            {
                return "Reservation.Error.TableTaken";
            }
        }

        return null;
    }
}
