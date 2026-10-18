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
            WaitingOnline = await _context.OnlineOrders.AsNoTracking().CountAsync(a => a.IsPending),
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

        var types = new[] { OrderType.Hall, OrderType.TakeAway, OrderType.Delivery, OrderType.Delivery_Online };
        model.ByType = types.Select(type => new DashboardTypeRow
        {
            Type = type,
            InKitchen = inKitchenByType.Where(a => a.Type == type).Sum(a => a.Count),
            Unpaid = unpaid.Count(a => a.Type == type),
            Paid = shift.Count(a => a.Cash && a.Type == type)
        }).ToList();

        model.OnlineRequests = await _context.OnlineOrders
            .AsNoTracking()
            .Where(a => a.IsPending)
            .OrderBy(a => a.OrderDate)
            .Take(ListLimit)
            .Select(a => new DashboardOnlineItem
            {
                Id = a.Id,
                CustomerName = a.CustomerName,
                PhoneNumber = a.PhoneNumber,
                Address = a.Address,
                TableId = a.TableId,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                TotalPrice = a.TotalPrice,
                OrderDate = a.OrderDate,
                Items = a.OnlineOrderMenuItems.Select(i => new DashboardLine
                {
                    ArName = i.MenuItem.ArName,
                    EnName = i.MenuItem.EnName,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();

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

        await FillTablesAsync(model);

        return model;
    }

    // ---------- table floor ----------
    // Each table is Free, Reserved or Occupied. To change how long a booking holds a table, edit the two numbers below.

    private const int HoldBeforeMinutes = 30;
    private const int HoldAfterHours = 3;

    private async Task FillTablesAsync(DashboardViewModel model)
    {
        model.Reservations = await GetUpcomingReservationsAsync();

        var openOrderByTable = await GetOpenOrderByTableAsync();
        var tables = await _context.Tables.AsNoTracking().OrderBy(a => a.Id).ToListAsync();

        foreach (var table in tables)
        {
            model.Tables.Add(BuildTableCard(table, model.Reservations, openOrderByTable));
        }
    }

    // Bookings that are not finished yet (waiting or confirmed), from 2 hours ago until the end of tomorrow.
    private async Task<List<DashboardReservation>> GetUpcomingReservationsAsync()
    {
        var now = DateTime.Now;
        var from = now.AddHours(-2);
        var until = now.Date.AddDays(2);

        return await _context.TableReservations
            .AsNoTracking()
            .Where(a => (a.Status == ReservationStatus.Pending || a.Status == ReservationStatus.Confirmed)
                && a.ReservationDate >= from
                && a.ReservationDate < until)
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
    }

    // Table id -> id of the unpaid hall order that is open on it.
    private async Task<Dictionary<int, int>> GetOpenOrderByTableAsync()
    {
        var openOrders = await _context.Orders
            .AsNoTracking()
            .Where(a => a.Type == OrderType.Hall && a.TableId != null && !a.Cash && !a.FinishShift)
            .OrderBy(a => a.Id)
            .Select(a => new { TableId = a.TableId.Value, OrderId = a.Id })
            .ToListAsync();

        var orderByTable = new Dictionary<int, int>();
        foreach (var order in openOrders)
        {
            orderByTable[order.TableId] = order.OrderId;
        }

        return orderByTable;
    }

    private DashboardTable BuildTableCard(Table table, List<DashboardReservation> reservations, Dictionary<int, int> openOrderByTable)
    {
        var card = new DashboardTable
        {
            Id = table.Id,
            ArName = table.ArName,
            EnName = table.EnName,
            Seats = table.NumOfPersons,
            State = DashboardTableState.Free
        };

        // An open order means guests are sitting there right now.
        if (table.IsPending)
        {
            card.State = DashboardTableState.Occupied;
            if (openOrderByTable.TryGetValue(table.Id, out var orderId))
            {
                card.OrderId = orderId;
            }

            return card;
        }

        var booking = FindBookingHoldingTable(table.Id, reservations);
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

        return card;
    }

    // The earliest booking of this table that falls inside the holding window, or null.
    private DashboardReservation FindBookingHoldingTable(int tableId, List<DashboardReservation> reservations)
    {
        var holdFrom = DateTime.Now.AddMinutes(-HoldBeforeMinutes);
        var holdTo = DateTime.Now.AddHours(HoldAfterHours);

        foreach (var booking in reservations)
        {
            var isThisTable = booking.TableId == tableId;
            var isInWindow = booking.ReservationDate >= holdFrom && booking.ReservationDate < holdTo;
            if (isThisTable && isInWindow)
            {
                return booking;
            }
        }

        return null;
    }
}
