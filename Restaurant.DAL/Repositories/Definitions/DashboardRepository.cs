using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class DashboardRepository : IDashboardRepository
{
    private const int ListLimit = 12;

    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(a => a.Id == userId)
            .Select(a => new
            {
                a.FirstName,
                a.LastName,
                a.Image
            })
            .FirstOrDefaultAsync();

        var shift = await _context.Orders
            .AsNoTracking()
            .Where(a => !a.FinishShift)
            .Select(a => new
            {
                a.Type,
                a.Cash,
                a.Complete,
                TotalPrice = a.Bill.TotalBillAmountAfterDiscountAndExtra
            })
            .ToListAsync();

        var unpaid = shift.Where(a => !a.Cash).ToList();

        var model = new DashboardViewModel
        {
            UserName = user == null ? string.Empty : (user.FirstName + " " + user.LastName).Trim(),
            UserImage = user?.Image,
            ShiftOrders = shift.Count,
            ShiftSales = shift.Where(a => a.Cash).Sum(a => a.TotalPrice),
            InKitchen = await _context.Orders.AsNoTracking().CountAsync(a => !a.Complete),
            AwaitingPayment = unpaid.Count,
            AwaitingPaymentAmount = unpaid.Sum(a => a.TotalPrice),
            WaitingReservations = await _context.TableReservations.AsNoTracking().CountAsync(a => a.Status == ReservationStatus.Pending)
        };

        var inKitchenByType = await _context.Orders
            .AsNoTracking()
            .Where(a => !a.Complete)
            .GroupBy(a => a.Type)
            .Select(a => new
            {
                Type = a.Key,
                Count = a.Count()
            })
            .ToListAsync();

        var types = new[] { OrderType.Hall, OrderType.TakeAway, OrderType.Delivery };
        model.ByType = types.Select(type => new DashboardTypeRow
        {
            Type = type,
            InKitchen = inKitchenByType.Where(a => a.Type == type).Sum(a => a.Count),
            Unpaid = unpaid.Count(a => a.Type == type),
            Paid = shift.Count(a => a.Cash && a.Type == type)
        }).ToList();

        model.Kitchen = await _context.Orders
            .AsNoTracking()
            .Where(a => !a.Complete)
            .OrderBy(a => a.Bill.Date)
            .Take(ListLimit)
            .Select(a => new DashboardKitchenItem
            {
                Id = a.Id,
                OrderCode = a.OrderCode,
                Type = a.Type,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                OrderDate = a.Bill.Date,
                Items = a.Lines.Select(i => new DashboardLine
                {
                    ArName = i.MenuItem.ArName,
                    EnName = i.MenuItem.EnName,
                    Quantity = i.BillDetail.Quantity ?? 0
                }).ToList()
            })
            .ToListAsync();

        model.Unpaid = await _context.Orders
            .AsNoTracking()
            .Where(a => !a.Cash && !a.FinishShift)
            .OrderBy(a => a.Bill.Date)
            .Take(ListLimit)
            .Select(a => new DashboardUnpaidItem
            {
                Id = a.Id,
                OrderCode = a.OrderCode,
                Type = a.Type,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                TotalPrice = a.Bill.TotalBillAmountAfterDiscountAndExtra,
                OrderDate = a.Bill.Date
            })
            .ToListAsync();

        // reservations that are waiting from 2 hours ago until 2 days ahead
        var now = DateTime.Now;
        var reservationsFrom = now.AddHours(-2);
        var reservationsUntil = now.Date.AddDays(2);
        model.Reservations = await _context.TableReservations
            .AsNoTracking()
            .Where(a => (a.Status == ReservationStatus.Pending || a.Status == ReservationStatus.Confirmed)
                && a.ReservationDate >= reservationsFrom
                && a.ReservationDate < reservationsUntil)
            .OrderBy(a => a.ReservationDate)
            .Take(20)
            .Select(a => new DashboardReservation
            {
                Id = a.Id,
                TableId = a.TableId,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                CustomerName = a.CustomerName,
                PhoneNumber = a.PhoneNumber,
                GuestCount = a.GuestCount,
                ReservationDate = a.ReservationDate,
                Notes = a.Notes,
                Status = a.Status
            })
            .ToListAsync();

        var openHallOrders = await _context.Orders
            .AsNoTracking()
            .Where(a => a.Type == OrderType.Hall && a.TableId != null && !a.Cash && !a.FinishShift)
            .OrderBy(a => a.Id)
            .Select(a => new { TableId = a.TableId.Value, OrderId = a.Id })
            .ToListAsync();

        // a reservation holds its table from 30 minutes before until 3 hours after
        var holdFrom = now.AddMinutes(-30);
        var holdTo = now.AddHours(3);

        var tables = await _context.Tables.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        foreach (var table in tables)
        {
            var card = new DashboardTable
            {
                Id = table.Id,
                ArName = table.ArName,
                EnName = table.EnName,
                Seats = table.NumOfPersons,
                State = DashboardTableState.Free
            };

            if (table.IsPending)
            {
                // an open order means guests are sitting there right now
                card.State = DashboardTableState.Occupied;
                card.OrderId = openHallOrders.LastOrDefault(a => a.TableId == table.Id)?.OrderId;
            }
            else
            {
                var booking = model.Reservations.FirstOrDefault(a => a.TableId == table.Id
                    && a.ReservationDate >= holdFrom
                    && a.ReservationDate < holdTo);
                if (booking != null)
                {
                    card.State = DashboardTableState.Reserved;
                    card.ReservationId = booking.Id;
                    card.BookingStatus = booking.Status;
                    card.CustomerName = booking.CustomerName;
                    card.PhoneNumber = booking.PhoneNumber;
                    card.ReservedAt = booking.ReservationDate;
                    card.Guests = booking.GuestCount;
                }
            }

            model.Tables.Add(card);
        }

        return model;
    }
}
