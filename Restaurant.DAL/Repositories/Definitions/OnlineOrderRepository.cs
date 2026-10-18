using Shared.Helpers;
using Restaurant.DAL.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class OnlineOrderRepository : IOnlineOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IAccountProvisionRepository _accountProvision;

    public OnlineOrderRepository(ApplicationDbContext context)
    {
        _context = context;
        _accountProvision = new AccountProvisionRepository(context);
    }

    public async Task<IEnumerable<MenuItem>> GetMenuAsync()
    {
        return await _context.MenuItems.AsNoTracking()
            .Include(a => a.MenuSection)
            .Include(a => a.Components).ThenInclude(a => a.Product)
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

            var customerName = order.CustomerName.Trim();
            var customerMobile = order.PhoneNumber.Trim();

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(a => a.PhoneNumber1 == customerMobile);
            if (existingCustomer is not null)
            {
                if (string.IsNullOrWhiteSpace(existingCustomer.DetailedAddress) && !string.IsNullOrWhiteSpace(order.Address))
                {
                    existingCustomer.DetailedAddress = order.Address;
                }
            }
            else
            {
                var nameTaken = await _context.Accounts.AnyAsync(a => a.ArName == customerName || a.EnName == customerName);
                var uniqueName = nameTaken ? $"{customerName} - {customerMobile}" : customerName;
                if (uniqueName.Length > 100)
                {
                    uniqueName = uniqueName[..100];
                }

                var newCustomerAccount = await _accountProvision.CreateAsync(uniqueName,
                    uniqueName,
                    AccountType.Customer,
                    AccountCodeRepository.AccountsReceivable);

                await _context.Customers.AddAsync(new Customer
                {
                    Account = newCustomerAccount,
                    PhoneNumber1 = customerMobile,
                    DetailedAddress = order.TableId.HasValue ? null : order.Address,
                    CreatedAt = DateTime.Now
                });
            }

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
                Message = ErrorLog.Message(ex)
            };
        }
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
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
