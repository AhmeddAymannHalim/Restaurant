using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class OnlineOrderRepository : IOnlineOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OnlineOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuItem>> GetMenuAsync()
    {
        return await _context.MenuItems.AsNoTracking()
            .Include(a => a.MenuSection)
            .Where(a => a.ShowOnWebsite)
            .OrderBy(a => a.MenuSection.ArName)
            .ThenBy(a => a.ArName)
            .ToListAsync();
    }

    public async Task<ResultMessage> CreateOrderAsync(OnlineOrder order, string customerId)
    {
        try
        {
            var wanted = order.OnlineOrderMenuItems
                .Where(a => a.Quantity > 0)
                .GroupBy(a => a.MenuItemId)
                .Select(g => new
                    {
                        MenuItemId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                .ToList();
            if (wanted.Count == 0)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "AtLeastOneItem"
                };
            }

            var ids = wanted.Select(a => a.MenuItemId).ToList();
            var menu = await _context.MenuItems.AsNoTracking()
                .Where(a => ids.Contains(a.Id) && a.ShowOnWebsite)
                .ToListAsync();
            if (menu.Count != ids.Count)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            if (order.TableId.HasValue && !await _context.Tables.AnyAsync(a => a.Id == order.TableId.Value))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var settings = await _context.ApplicationSettings
                .AsNoTracking()
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            int? zoneId = null;
            decimal deliveryFee = 0;
            if (!order.TableId.HasValue)
            {
                var zones = await _context.DeliveryZones.AsNoTracking().ToListAsync();
                if (zones.Count > 0)
                {
                    var zone = zones.FirstOrDefault(a => a.Id == order.DeliveryZoneId);
                    if (zone is null)
                    {
                        return new ResultMessage
                        {
                            Success = false,
                            Message = "DeliveryZoneRequired"
                        };
                    }

                    zoneId = zone.Id;
                    deliveryFee = zone.Fee;
                }
            }

            var lines = wanted.Select(w =>
            {
                var dish = menu.First(a => a.Id == w.MenuItemId);
                return new OnlineOrderMenuItem
                {
                    MenuItemId = dish.Id,
                    Quantity = w.Quantity,
                    PriceForUnit = dish.Price,
                    Price = Math.Round(dish.Price * w.Quantity, 2)
                };
            }).ToList();

            var subTotal = lines.Sum(a => a.Price);
            var tax = Math.Round(subTotal * (settings?.OrderTax ?? 0) / 100, 2);
            var service = settings?.OrderService ?? 0;

            var entity = new OnlineOrder
            {
                CustomerId = customerId,
                CustomerName = order.CustomerName,
                Address = order.TableId.HasValue ? null : order.Address,
                TableId = order.TableId,
                PhoneNumber = order.PhoneNumber,
                OrderDate = DateTime.Now,
                IsPending = true,
                Status = OnlineOrderStatus.Pending,
                DeliveryZoneId = zoneId,
                DeliveryFee = deliveryFee,
                Tax = tax,
                Service = service,
                TotalPrice = subTotal + tax + service + deliveryFee,
                OnlineOrderMenuItems = lines
            };

            await _context.OnlineOrders.AddAsync(entity);
            await AddToCustomerListAsync(order);
            await _context.SaveChangesAsync();

            return new ResultMessage
            {
                Success = true,
                Data = entity.Id.ToString()
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    // Every web customer becomes a restaurant customer, matched by mobile number.
    private async Task AddToCustomerListAsync(OnlineOrder order)
    {
        var name = order.CustomerName.Trim();
        var mobile = order.PhoneNumber.Trim();

        var existing = await _context.Customers
            .FirstOrDefaultAsync(a => a.PhoneNumber == mobile);
        if (existing is not null)
        {
            if (string.IsNullOrWhiteSpace(existing.Address) && !string.IsNullOrWhiteSpace(order.Address))
            {
                existing.Address = order.Address;
            }

            return;
        }

        // ArName/EnName are unique, so two different people called the same name get their number appended.
        var taken = await _context.Customers.AnyAsync(a => a.ArName == name || a.EnName == name);
        var unique = taken ? $"{name} - {mobile}" : name;
        if (unique.Length > 100)
        {
            unique = unique[..100];
        }

        await _context.Customers.AddAsync(new Customer
        {
            ArName = unique,
            EnName = unique,
            PhoneNumber = mobile,
            Address = order.TableId.HasValue ? null : order.Address
        });
    }

    public async Task<IEnumerable<OnlineOrder>> GetCustomerOrdersAsync(string customerId)
    {
        return await _context.OnlineOrders.AsNoTracking()
            .Include(a => a.Table)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.OrderDate)
            .ToListAsync();
    }

    public async Task<OnlineOrder> GetOrderAsync(int id)
    {
        return await _context.OnlineOrders.AsNoTracking()
            .Include(a => a.Table)
            .Include(a => a.DeliveryZone)
            .Include(a => a.OnlineOrderMenuItems).ThenInclude(a => a.MenuItem)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> CancelOrderAsync(int id, string userId, bool isStaff)
    {
        try
        {
            var order = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == id && a.IsPending);
            if (order is null)
            {
                return new ResultMessage { Success = true };
            }

            if (!isStaff && order.CustomerId != userId)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            _context.OnlineOrders.Remove(order);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
