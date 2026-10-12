using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
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
                a.TotalPrice
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
            AwaitingPaymentAmount = unpaid.Sum(a => a.TotalPrice)
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
                MobileNumber = a.MobileNumber,
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
            .OrderBy(a => a.OrderDate)
            .Take(ListLimit)
            .Select(a => new DashboardKitchenItem
            {
                Id = a.Id,
                OrderCode = a.OrderCode,
                Type = a.Type,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                OrderDate = a.OrderDate,
                Items = a.OrderMenuItems.Select(i => new DashboardLine
                {
                    ArName = i.MenuItem.ArName,
                    EnName = i.MenuItem.EnName,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();

        model.Unpaid = await _context.Orders
            .AsNoTracking()
            .Where(a => !a.Cash && !a.FinishShift)
            .OrderBy(a => a.OrderDate)
            .Take(ListLimit)
            .Select(a => new DashboardUnpaidItem
            {
                Id = a.Id,
                OrderCode = a.OrderCode,
                Type = a.Type,
                TableArName = a.Table.ArName,
                TableEnName = a.Table.EnName,
                TotalPrice = a.TotalPrice,
                OrderDate = a.OrderDate
            })
            .ToListAsync();

        return model;
    }
}
