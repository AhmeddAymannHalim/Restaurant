using PaymentMethod = Shared.Enums.PaymentMethod;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId)
    {
        var open = await _context.Orders.AsNoTracking()
            .Where(a => !a.FinishShift)
            .Select(a => new
            {
                a.Type,
                a.Cash,
                a.OnAccount,
                a.PaymentMethod,
                a.SellerId,
                a.ReceiverId,
                a.TotalPrice
            })
            .ToListAsync();

        static List<ShiftRowViewModel> Rows(IEnumerable<OrderType> types, IEnumerable<(OrderType Type, decimal Total)> orders)
        {
            return types.Select(type => new ShiftRowViewModel
            {
                Type = type,
                Count = orders.Count(a => a.Type == type),
                Amount = orders.Where(a => a.Type == type).Sum(a => a.Total)
            }).ToList();
        }

        var paidTypes = new[] { OrderType.Hall, OrderType.Delivery, OrderType.Delivery_Online, OrderType.TakeAway };
        var unpaidTypes = new[] { OrderType.Hall, OrderType.Delivery, OrderType.Delivery_Online };

        var collected = open.Where(a => a.Cash
            && !a.OnAccount
            && a.SellerId == userId
            && a.ReceiverId == userId).Select(a => (a.Type, a.TotalPrice));
        var pendingMine = open.Where(a => !a.Cash && a.SellerId == userId).Select(a => (a.Type, a.TotalPrice));
        var pendingOthers = open.Where(a => !a.Cash && a.SellerId != userId).Select(a => (a.Type, a.TotalPrice));
        var fromOthers = open.Where(a => a.Cash
            && !a.OnAccount
            && a.SellerId != userId
            && a.ReceiverId == userId).ToList();
        // Visa / InstaPay / wallets never enter the cash drawer, so they are taken out of the expected cash.
        var electronic = open.Where(a => a.Cash
            && !a.OnAccount
            && a.PaymentMethod != null
            && a.PaymentMethod != PaymentMethod.Cash
            && a.ReceiverId == userId).ToList();
        var onAccount = open.Where(a => a.OnAccount
            && (a.SellerId == userId || a.ReceiverId == userId)).ToList();

        var lastReport = await _context.ShiftCloseReports.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Id)
            .Select(a => new ShiftCloseReportViewModel
            {
                ClosedAt = a.ClosedAt,
                ExpectedCash = a.ExpectedCash,
                CountedCash = a.CountedCash,
                Variance = a.Variance,
                HadIncompleteOrders = a.HadIncompleteOrders
            })
            .FirstOrDefaultAsync();

        return new ShiftDashboardViewModel
        {
            Collected = Rows(paidTypes, collected),
            PendingMine = Rows(unpaidTypes, pendingMine),
            PendingOthers = Rows(unpaidTypes, pendingOthers),
            CollectedFromOthers = new ShiftRowViewModel
            {
                Count = fromOthers.Count,
                Amount = fromOthers.Sum(a => a.TotalPrice)
            },
            Electronic = new ShiftRowViewModel
            {
                Count = electronic.Count,
                Amount = electronic.Sum(a => a.TotalPrice)
            },
            OnAccount = new ShiftRowViewModel
            {
                Count = onAccount.Count,
                Amount = onAccount.Sum(a => a.TotalPrice)
            },
            LastShiftReport = lastReport
        };
    }

    public async Task<PaginatedResult<Order>> GetPagedOrdersAsync(PageRequest request, string userId)
    {
        var query = _context.Orders.AsNoTracking()
            .Include(a => a.Seller)
            .Include(a => a.Receiver)
            .Include(a => a.Table)
            .Include(a => a.Employee)
            .Include(a => a.OrderMenuItems)
            .AsQueryable();

        query = request.Status switch
        {
            "pending" => query.Where(a => !a.Cash),
            "history" => query.Where(a => a.Cash && a.FinishShift),
            _ => query.Where(a => a.Cash && !a.FinishShift && a.SellerId == userId)
        };

        if (Enum.TryParse<OrderType>(request.Type, out var type))
        {
            query = query.Where(a => a.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.OrderCode.ToLower().Contains(search) || a.CustomerName.ToLower().Contains(search));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.OrderDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.OrderDate < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.OrderDate)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(data, total, request);
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Seller)
            .Include(a => a.Receiver)
            .Include(a => a.Table)
            .Include(a => a.Employee)
            .Include(a => a.DeliveryZone)
            .Include(a => a.OrderMenuItems).ThenInclude(a => a.MenuItem)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var last = await _context.Orders.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.OrderCode)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
    }

    public async Task<IEnumerable<MenuItem>> GetPosMenuAsync()
    {
        return await _context.MenuItems.AsNoTracking()
            .Include(a => a.MenuSection)
            .Include(a => a.Components).ThenInclude(a => a.Product)
            .OrderBy(a => a.MenuSection.ArName)
            .ThenBy(a => a.ArName)
            .ToListAsync();
    }

    public async Task<ApplicationSetting> GetSettingsAsync()
    {
        return await _context.ApplicationSettings.AsNoTracking().OrderByDescending(a => a.Id).FirstOrDefaultAsync()
            ?? new ApplicationSetting();
    }

    public async Task<IEnumerable<Table>> GetPendingTablesAsync(int exceptTableId)
    {
        return await _context.Tables.AsNoTracking()
            .Where(a => a.IsPending && a.Id != exceptTableId)
            .OrderBy(a => a.ArName)
            .ToListAsync();
    }

    public async Task<ResultMessage> CreateOrderAsync(Order order, int? onlineOrderId, string userId)
    {
        try
        {
            var settings = await GetSettingsAsync();

            var wanted = order.OrderMenuItems
                .GroupBy(a => a.MenuItemId)
                .Select(g => new
                    {
                        MenuItemId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                .ToList();
            if (wanted.Count == 0)
            {
                return Fail("AtLeastOneItem");
            }

            var ids = wanted.Select(a => a.MenuItemId).ToList();
            var menu = await _context.MenuItems
                .Include(a => a.Components).ThenInclude(a => a.Product)
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();
            if (menu.Count != ids.Count)
            {
                return Fail("RecordNotFound");
            }

            if (menu.Any(a => !a.IsAvailable))
            {
                return Fail("MenuItemNotAvailable");
            }

            Customer customer = null;
            if (order.OnAccount)
            {
                customer = order.CustomerId.HasValue ? await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value) : null;
                if (customer is null)
                {
                    return Fail("CustomerRequired");
                }
            }

            Table table = null;
            Order mergeInto = null;
            var cash = order.Cash || order.OnAccount;
            switch (order.Type)
            {
                case OrderType.TakeAway:
                    cash = true;
                    order.TableId = null;
                    order.EmployeeId = null;
                    break;

                case OrderType.Hall:
                    order.EmployeeId = null;
                    if (cash)
                    {
                        order.TableId = null;
                    }
                    else
                    {
                        table = order.TableId.HasValue ? await _context.Tables.FindAsync(order.TableId.Value) : null;
                        if (table is null)
                        {
                            return Fail("TableRequired");
                        }

                        if (table.IsPending)
                        {
                            if (!onlineOrderId.HasValue)
                            {
                                return Fail("TableBusy");
                            }

                            mergeInto = await _context.Orders
                                .Include(a => a.OrderMenuItems)
                                .FirstOrDefaultAsync(a => a.TableId == table.Id
                                    && !a.Cash
                                    && a.Type == OrderType.Hall);
                            if (mergeInto is null)
                            {
                                return Fail("TableBusy");
                            }

                            table = null;
                        }
                    }
                    break;

                case OrderType.Delivery:
                case OrderType.Delivery_Online:
                    order.TableId = null;
                    cash = true;
                    if (order.Type == OrderType.Delivery_Online && !onlineOrderId.HasValue)
                    {
                        return Fail("RecordNotFound");
                    }

                    if (!order.EmployeeId.HasValue
                        || !await _context.Employees.AnyAsync(a => a.Id == order.EmployeeId))
                    {
                        return Fail("DeliveryDriverRequired");
                    }
                    break;
            }

            var lines = wanted.Select(w =>
            {
                var dish = menu.First(a => a.Id == w.MenuItemId);
                return new OrderMenuItem
                {
                    MenuItemId = dish.Id,
                    Quantity = w.Quantity,
                    PriceForUnit = dish.Price,
                    UnitCost = dish.TotalCost,
                    Price = Math.Round(w.Quantity * dish.Price, 2)
                };
            }).ToList();

            var needs = new Dictionary<(int StoreId, int ProductId), decimal>();
            foreach (var line in lines)
            {
                var dish = menu.First(a => a.Id == line.MenuItemId);
                foreach (var component in dish.Components)
                {
                    var key = (dish.StoreId, component.ProductId);
                    var used = line.Quantity * component.Quantity;
                    needs[key] = needs.GetValueOrDefault(key) + used;

                    line.Ingredients.Add(new OrderIngredient
                    {
                        StoreId = dish.StoreId,
                        ProductId = component.ProductId,
                        Quantity = used
                    });
                }
            }

            var movements = new List<StoreProductMovement>();
            foreach (var need in needs)
            {
                var stock = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == need.Key.StoreId
                    && a.ProductId == need.Key.ProductId);
                if (stock is null || stock.Quantity < need.Value)
                {
                    return Fail("InsufficientStock");
                }

                stock.Quantity -= need.Value;
                movements.Add(new StoreProductMovement
                {
                    StoreId = need.Key.StoreId,
                    ProductId = need.Key.ProductId,
                    QuantityDelta = -need.Value,
                    Source = StockMovementSource.Order
                });
            }

            if (mergeInto is not null)
            {
                foreach (var line in lines)
                {
                    mergeInto.OrderMenuItems.Add(line);
                }

                Recalculate(mergeInto, settings);

                var waiting = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == onlineOrderId.Value);
                if (waiting is not null && waiting.Status == OnlineOrderStatus.Rejected)
                {
                    return Fail("OnlineOrderRejected");
                }
                if (waiting is null || !waiting.IsPending)
                {
                    return Fail("RecordNotFound");
                }

                waiting.IsPending = false;
                waiting.Status = OnlineOrderStatus.Accepted;
                mergeInto.Complete = false;

                foreach (var movement in movements)
                {
                    movement.ReferenceCode = mergeInto.OrderCode;
                }
                await _context.StoreProductMovements.AddRangeAsync(movements);

                await _context.SaveChangesAsync();

                return new ResultMessage
                {
                    Success = true,
                    Data = mergeInto.Id.ToString()
                };
            }

            int? deliveryZoneId = null;
            decimal deliveryFee = 0;
            if (order.Type is OrderType.Delivery or OrderType.Delivery_Online)
            {
                if (onlineOrderId.HasValue)
                {
                    var source = await _context.OnlineOrders.AsNoTracking().FirstOrDefaultAsync(a => a.Id == onlineOrderId.Value);
                    deliveryZoneId = source?.DeliveryZoneId;
                    deliveryFee = source?.DeliveryFee ?? 0;
                }
                else
                {
                    var zones = await _context.DeliveryZones.AsNoTracking().ToListAsync();
                    if (zones.Count > 0)
                    {
                        var zone = zones.FirstOrDefault(a => a.Id == order.DeliveryZoneId);
                        if (zone is null)
                        {
                            return Fail("DeliveryZoneRequired");
                        }

                        deliveryZoneId = zone.Id;
                        deliveryFee = zone.Fee;
                    }
                }
            }

            var entity = new Order
            {
                OrderCode = await GetNextCodeAsync(),
                OrderDate = DateTime.Now,
                Type = order.Type,
                SellerId = userId,
                Cash = cash,
                PaymentMethod = cash && !order.OnAccount ? order.PaymentMethod ?? PaymentMethod.Cash : null,
                OnAccount = order.OnAccount,
                ReceiverId = cash ? userId : null,
                Complete = false,
                FinishShift = false,
                CustomerName = string.IsNullOrWhiteSpace(order.CustomerName) ? "عميل نقدي" : order.CustomerName.Trim(),
                CustomerId = order.CustomerId,
                TableId = order.TableId,
                EmployeeId = order.EmployeeId,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                DeliveryZoneId = deliveryZoneId,
                DeliveryFee = deliveryFee,
                Service = order.Type == OrderType.TakeAway ? 0 : settings.OrderService,
                Discount = Math.Max(0, order.Discount),
                OrderMenuItems = lines
            };
            Recalculate(entity, settings);

            if (customer is not null)
            {
                customer.Account.Adjust(entity.TotalPrice);
            }

            if (table is not null)
            {
                table.IsPending = true;
            }

            if (onlineOrderId.HasValue)
            {
                var online = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == onlineOrderId.Value);
                if (online is not null && online.Status == OnlineOrderStatus.Rejected)
                {
                    return Fail("OnlineOrderRejected");
                }
                if (online is null || !online.IsPending)
                {
                    return Fail("RecordNotFound");
                }

                online.IsPending = false;
                online.Status = OnlineOrderStatus.Accepted;
            }

            foreach (var movement in movements)
            {
                movement.ReferenceCode = entity.OrderCode;
            }
            await _context.StoreProductMovements.AddRangeAsync(movements);

            await _context.Orders.AddAsync(entity);
            await _context.SaveChangesAsync();

            if (entity.Cash)
            {
                await ErpOrderMirror.SyncAsync(_context, entity);
            }

            return new ResultMessage
            {
                Success = true,
                Data = entity.Id.ToString()
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail("ConcurrentUpdate");
        }
        catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Orders_OrderCode"))
        {
            return Fail("ConcurrentUpdate");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return Fail("RecordNotFound");
            }

            if (order.Cash)
            {
                return new ResultMessage
                {
                    Success = true,
                    Data = order.Id.ToString()
                };
            }

            await FreeTableAsync(order);
            order.Cash = true;
            order.PaymentMethod = method;
            order.ReceiverId = userId;
            order.OrderDate = DateTime.Now;

            await _context.SaveChangesAsync();
            await ErpOrderMirror.SyncAsync(_context, order);

            return new ResultMessage
            {
                Success = true,
                Data = order.Id.ToString()
            };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResultMessage> MoveItemAsync(int orderMenuItemId, int targetTableId)
    {
        try
        {
            var line = await _context.OrderMenuItems.FirstOrDefaultAsync(a => a.Id == orderMenuItemId);
            if (line is null)
            {
                return Fail("RecordNotFound");
            }

            var from = await LoadUnpaidHallOrderAsync(line.OrderId);
            var to = await _context.Orders.Include(a => a.OrderMenuItems)
                .FirstOrDefaultAsync(a => a.TableId == targetTableId && !a.Cash && a.Type == OrderType.Hall);
            if (from is null || to is null || from.Id == to.Id)
            {
                return Fail("TargetTableHasNoOrder");
            }

            var settings = await GetSettingsAsync();

            from.OrderMenuItems.Remove(line);
            to.OrderMenuItems.Add(line);

            if (from.OrderMenuItems.Count == 0)
            {
                await FreeTableAsync(from);
                _context.Orders.Remove(from);
            }
            else
            {
                Recalculate(from, settings);
            }

            Recalculate(to, settings);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResultMessage> MergeOrderAsync(int id, int targetTableId)
    {
        try
        {
            var from = await LoadUnpaidHallOrderAsync(id);
            var to = await _context.Orders.Include(a => a.OrderMenuItems)
                .FirstOrDefaultAsync(a => a.TableId == targetTableId && !a.Cash && a.Type == OrderType.Hall);
            if (from is null || to is null || from.Id == to.Id)
            {
                return Fail("TargetTableHasNoOrder");
            }

            var settings = await GetSettingsAsync();

            foreach (var line in from.OrderMenuItems.ToList())
            {
                from.OrderMenuItems.Remove(line);
                to.OrderMenuItems.Add(line);
            }

            to.Service += from.Service;
            to.Discount += from.Discount;
            Recalculate(to, settings);

            await FreeTableAsync(from);
            _context.Orders.Remove(from);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResultMessage> DeleteOrderAsync(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(a => a.OrderMenuItems).ThenInclude(a => a.Ingredients)
                .Include(a => a.OrderMenuItems).ThenInclude(a => a.MenuItem).ThenInclude(a => a.Components)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = true };
            }

            if (order.FinishShift)
            {
                return Fail("ShiftClosed");
            }

            foreach (var line in order.OrderMenuItems)
            {
                var ingredients = line.Ingredients
                    .Select(a => (a.StoreId, a.ProductId, a.Quantity))
                    .ToList();

                if (ingredients.Count == 0)
                {
                    ingredients = line.MenuItem.Components
                        .Select(a => (line.MenuItem.StoreId, a.ProductId, line.Quantity * a.Quantity))
                        .ToList();
                }

                foreach (var (storeId, productId, quantity) in ingredients)
                {
                    var stock = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == storeId
                        && a.ProductId == productId);
                    if (stock is not null)
                    {
                        stock.Quantity += quantity;
                        await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                        {
                            StoreId = storeId,
                            ProductId = productId,
                            QuantityDelta = quantity,
                            Source = StockMovementSource.OrderCancellation,
                            ReferenceCode = order.OrderCode
                        });
                    }
                }
            }

            if (order.OnAccount && order.CustomerId.HasValue)
            {
                var customer = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value);
                if (customer is not null)
                {
                    customer.Account.Adjust(-order.TotalPrice);
                }
            }

            await FreeTableAsync(order);
            await ErpOrderMirror.RemoveAsync(_context, order);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail("ConcurrentUpdate");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<IEnumerable<Order>> GetIncompleteCashOrdersAsync(string userId)
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Table)
            .Where(a => a.Cash && !a.FinishShift && a.ReceiverId == userId && !a.Complete)
            .OrderBy(a => a.OrderDate)
            .ToListAsync();
    }

    public async Task<ResultMessage> FinishShiftAsync(string userId, decimal countedCash)
    {
        try
        {
            if (countedCash < 0)
            {
                return Fail("CountedCashInvalid");
            }

            var hadIncompleteOrders = await _context.Orders.AnyAsync(a => a.Cash && !a.FinishShift && a.ReceiverId == userId && !a.Complete);
            var expectedCash = (await GetShiftDashboardAsync(userId)).MoneyInDrawer;

            var orders = await _context.Orders.Where(a => a.Cash && !a.FinishShift && a.ReceiverId == userId).ToListAsync();
            foreach (var order in orders)
            {
                order.FinishShift = true;
            }

            await _context.ShiftCloseReports.AddAsync(new ShiftCloseReport
            {
                UserId = userId,
                ExpectedCash = expectedCash,
                CountedCash = countedCash,
                Variance = countedCash - expectedCash,
                HadIncompleteOrders = hadIncompleteOrders
            });

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<IEnumerable<Order>> GetKitchenOrdersAsync()
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Table)
            .Include(a => a.OrderMenuItems).ThenInclude(a => a.MenuItem)
            .Where(a => !a.Complete)
            .OrderBy(a => a.OrderDate)
            .ToListAsync();
    }

    public async Task<ResultMessage> CompleteOrderAsync(int id)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return Fail("RecordNotFound");
            }

            order.Complete = true;
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<bool> VerifyShiftPasswordAsync(string password)
    {
        var setting = await _context.ApplicationSettings.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
        if (setting is null || !ShiftPassword.Verify(setting.Password, password, out var needsRehash))
        {
            return false;
        }

        if (needsRehash)
        {
            setting.Password = ShiftPassword.Hash(password);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<IEnumerable<OnlineOrder>> GetPendingOnlineOrdersAsync()
    {
        return await _context.OnlineOrders.AsNoTracking()
            .Include(a => a.Table)
            .Include(a => a.DeliveryZone)
            .Include(a => a.OnlineOrderMenuItems).ThenInclude(a => a.MenuItem)
            .Where(a => a.IsPending)
            .OrderBy(a => a.OrderDate)
            .ToListAsync();
    }

    public async Task<OnlineOrder> GetOnlineOrderAsync(int id)
    {
        return await _context.OnlineOrders.AsNoTracking()
            .Include(a => a.Table)
            .Include(a => a.DeliveryZone)
            .Include(a => a.OnlineOrderMenuItems).ThenInclude(a => a.MenuItem)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsPending);
    }

    public async Task<ResultMessage> CancelOnlineOrderAsync(int id)
    {
        try
        {
            var online = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == id && a.IsPending);
            if (online is not null)
            {
                _context.OnlineOrders.Remove(online);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ResultMessage> RejectOnlineOrderAsync(int id, string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Fail("RejectionReasonRequired");
            }

            var online = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == id && a.IsPending);
            if (online is not null)
            {
                online.IsPending = false;
                online.Status = OnlineOrderStatus.Rejected;
                online.RejectionReason = reason.Trim();
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static void Recalculate(Order order, ApplicationSetting settings)
    {
        var subTotal = order.OrderMenuItems.Sum(a => a.Price);
        order.Tax = Math.Round(subTotal * settings.OrderTax / 100, 2);

        var gross = subTotal + order.Tax + order.Service + order.DeliveryFee;
        order.Discount = Math.Min(Math.Max(0, order.Discount), gross);
        order.TotalPrice = gross - order.Discount;
    }

    private async Task<Order> LoadUnpaidHallOrderAsync(int id)
    {
        return await _context.Orders.Include(a => a.OrderMenuItems)
            .FirstOrDefaultAsync(a => a.Id == id && !a.Cash && a.Type == OrderType.Hall);
    }

    private async Task FreeTableAsync(Order order)
    {
        if (order.Type == OrderType.Hall && order.TableId.HasValue)
        {
            var table = await _context.Tables.FindAsync(order.TableId.Value);
            if (table is not null)
            {
                table.IsPending = false;
            }
        }
    }

    private static ResultMessage Fail(string message)
    {
        return new ResultMessage
        {
            Success = false,
            Message = message
        };
    }
}
