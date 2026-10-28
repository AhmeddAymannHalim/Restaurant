using Shared.Helpers;
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
    private readonly IOrderLedgerRepository _orderLedger;
    private readonly IStockRepository _stock;
    private readonly IBillSupportRepository _billSupport;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
        _orderLedger = new OrderLedgerRepository(context);
        _stock = new StockRepository(context);
        _billSupport = new BillSupportRepository(context);
    }

    // ---------------------------------------------------------------- read

    public async Task<PaginatedResult<Order>> GetPagedOrdersAsync(PageRequest request)
    {
        // the screen has two lists only: orders still to be paid, and paid ones
        request.Status = request.Status == "pending" ? "pending" : "paid";

        var query = _context.Orders.AsNoTracking()
            .Include(a => a.Bill)
            .Include(a => a.Seller)
            .Include(a => a.Receiver)
            .Include(a => a.Table)
            .Include(a => a.Employee)
            .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
            .AsQueryable();

        query = request.Status switch
        {
            "pending" => query.Where(a => !a.Cash),
            _ => query.Where(a => a.Cash)
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
            query = query.Where(a => a.Bill.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Bill.Date < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.Bill.Date)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Order>.Create(data, total, request);
    }

    // only an unpaid order of a table can be moved or merged
    public async Task<Order> GetPendingHallOrderByIdAsync(int id)
    {
        var order = await GetOrderByIdAsync(id);
        if (order is null || order.Cash || order.TableId == null)
        {
            return null;
        }

        return order;
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Bill)
            .Include(a => a.Seller)
            .Include(a => a.Receiver)
            .Include(a => a.Table)
            .Include(a => a.Employee)
            .Include(a => a.DeliveryZone)
            .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
            .Include(a => a.Lines).ThenInclude(a => a.MenuItem)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var setting = await _orderLedger.GetSettingAsync();

        return await _billSupport.NextBillCodeAsync(setting.Id);
    }

    public async Task<ApplicationSetting> GetSettingsAsync()
    {
        return await _context.ApplicationSettings.AsNoTracking()
                                                 .OrderByDescending(a => a.Id)
                                                 .FirstOrDefaultAsync() ?? new ApplicationSetting();
    }

    public async Task<Order> GetNewOrderAsync(int? tableId, int? reservationId)
    {
        var settings = await GetSettingsAsync();
        var order = new Order
        {
            OrderCode = await GetNextCodeAsync(),
            CustomerName = null, // typed on the screen, not the default cash customer
            Bill = new BillMaster { Date = DateTime.Now },
            Service = settings.OrderService,
            TaxPercent = settings.OrderTax,
            DefaultService = settings.OrderService,
            MaxDiscount = settings.OrderDiscount,
            LoyaltyPointValue = settings.LoyaltyPointValue,
            Menu = (await GetCashierMenuAsync()).ToList()
        };

        if (tableId.HasValue)
        {
            order.Type = OrderType.Hall;
            order.TableId = tableId;
        }

        if (reservationId.HasValue)
        {
            var reservation = await _context.TableReservations.AsNoTracking().FirstOrDefaultAsync(a => a.Id == reservationId.Value);
            if (reservation != null)
            {
                order.CustomerName = reservation.CustomerName;
                order.PhoneNumber = reservation.PhoneNumber;
            }
        }

        return order;
    }

    public async Task<IEnumerable<Table>> GetPendingTablesAsync(int exceptTableId)
    {
        return await _context.Tables.AsNoTracking()
            .Where(a => a.IsPending && a.Id != exceptTableId)
            .OrderBy(a => a.ArName)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetCashierMenuAsync()
    {
        var menu = await _context.MenuItems.AsNoTracking()
            .Include(a => a.MenuSection)
            .Include(a => a.Components).ThenInclude(a => a.Product)
            .OrderBy(a => a.MenuSection.ArName)
            .ThenBy(a => a.ArName)
            .ToListAsync();

        var stock = await _stock.QuantitiesAsync();
        foreach (var dish in menu)
        {
            // a dish without a recipe has no stock limit
            if (dish.Components.Any())
            {
                var portions = int.MaxValue;
                foreach (var component in dish.Components)
                {
                    if (component.QuantityInBase <= 0)
                    {
                        continue;
                    }

                    var inStock = _stock.FindQuantity(stock, dish.StoreId, component.ProductId);
                    portions = Math.Min(portions, (int)Math.Floor(inStock / component.QuantityInBase));
                }

                dish.AvailablePortions = portions;
            }

            if (dish.AvailablePortions <= 0)
            {
                dish.IsAvailable = false;
            }
        }

        return menu;
    }

    public async Task<IEnumerable<Order>> GetKitchenOrdersAsync()
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Bill)
            .Include(a => a.Table)
            .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
            .Include(a => a.Lines).ThenInclude(a => a.MenuItem)
            .Where(a => !a.Complete)
            .OrderBy(a => a.Bill.Date)
            .ToListAsync();
    }

    // ---------------------------------------------------------------- write

    public async Task<ResultMessage> CreateOrderAsync(Order order, string userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var settings = await GetSettingsAsync();

            // 1. the dishes: the same dish may come twice, so its quantity is summed
            var menuItemIds = order.Lines.Select(a => a.MenuItemId).Distinct().ToList();
            if (menuItemIds.Count == 0)
            {
                return new ResultMessage { Success = false, Message = "AtLeastOneItem" };
            }

            // a zero or negative quantity would put ingredients back into the stock and make a negative bill
            if (order.Lines.Any(a => a.Quantity <= 0))
            {
                return new ResultMessage { Success = false, Message = "Val.GreaterThanZero" };
            }

            var menu = await _context.MenuItems
                .Include(a => a.Components).ThenInclude(a => a.Product)
                .Where(a => menuItemIds.Contains(a.Id))
                .ToListAsync();
            if (menu.Count != menuItemIds.Count)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (menu.Any(a => !a.IsAvailable))
            {
                return new ResultMessage { Success = false, Message = "MenuItemNotAvailable" };
            }

            // 2. every order has a customer
            var customer = order.CustomerId.HasValue
                ? await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value)
                : null;
            if (customer is null)
            {
                return new ResultMessage { Success = false, Message = "CustomerRequired" };
            }

            // 3. each order type decides if it is paid now and what it needs (table, driver)
            Table table = null;
            var cash = order.Cash || order.OnAccount;
            if (order.Type == OrderType.TakeAway)
            {
                cash = true;
                order.TableId = null;
                order.EmployeeId = null;
            }
            else if (order.Type == OrderType.Hall)
            {
                order.EmployeeId = null;
                table = order.TableId.HasValue ? await _context.Tables.FindAsync(order.TableId.Value) : null;
                if (table is null)
                {
                    return new ResultMessage { Success = false, Message = "TableRequired" };
                }

                if (table.IsPending)
                {
                    return new ResultMessage { Success = false, Message = "TableBusy" };
                }
            }
            else
            {
                order.TableId = null;
                cash = true;
                if (!order.EmployeeId.HasValue || !await _context.Employees.AnyAsync(a => a.Id == order.EmployeeId))
                {
                    return new ResultMessage { Success = false, Message = "DeliveryDriverRequired" };
                }

                if (string.IsNullOrWhiteSpace(order.Address) || string.IsNullOrWhiteSpace(order.PhoneNumber))
                {
                    return new ResultMessage { Success = false, Message = "Val.RequiredField" };
                }
            }

            // 4. delivery zone and fee
            int? deliveryZoneId = null;
            decimal deliveryFee = 0;
            if (order.Type is OrderType.Delivery)
            {
                var zones = await _context.DeliveryZones.AsNoTracking().ToListAsync();
                if (zones.Count > 0)
                {
                    var zone = zones.FirstOrDefault(a => a.Id == order.DeliveryZoneId);
                    if (zone is null)
                    {
                        return new ResultMessage { Success = false, Message = "DeliveryZoneRequired" };
                    }

                    deliveryZoneId = zone.Id;
                    deliveryFee = zone.Fee;
                }
            }

            // 5. the order lines, and take the ingredients out of the stock
            var lines = new List<OrderMenuItem>();
            var movements = new List<StoreProductMovement>();
            var rowNumber = 0;
            foreach (var menuItemId in menuItemIds)
            {
                var dish = menu.First(a => a.Id == menuItemId);
                var quantity = order.Lines.Where(a => a.MenuItemId == menuItemId).Sum(a => a.Quantity);
                rowNumber++;

                var line = new OrderMenuItem
                {
                    MenuItemId = dish.Id,
                    BillDetail = new BillDetail
                    {
                        RowNumber = (byte)Math.Min(rowNumber, byte.MaxValue),
                        ItemId = await _orderLedger.GetItemIdAsync(dish),
                        StoreId = dish.StoreId,
                        Quantity = quantity,
                        UnitPrice = dish.Price,
                        TransRate = 1,
                        UnitCostAtPosting = dish.TotalCost,
                        TotalAmount = Math.Round(quantity * dish.Price, 2),
                        CreatedAt = DateTime.Now
                    }
                };

                foreach (var component in dish.Components)
                {
                    var used = quantity * component.QuantityInBase;
                    if (!await _stock.AdjustAsync(dish.StoreId, component.ProductId, -used, createIfMissing: false))
                    {
                        return new ResultMessage { Success = false, Message = "InsufficientStock" };
                    }

                    line.Ingredients.Add(new OrderIngredient
                    {
                        StoreId = dish.StoreId,
                        ProductId = component.ProductId,
                        Quantity = used
                    });
                    movements.Add(new StoreProductMovement
                    {
                        StoreId = dish.StoreId,
                        ProductId = component.ProductId,
                        QuantityDelta = -used,
                        Source = StockMovementSource.Order
                    });
                }

                lines.Add(line);
            }

            // 6. loyalty points the customer spends on this order become part of its discount
            if (order.PointsRedeemed < 0)
            {
                return new ResultMessage { Success = false, Message = "PointsNegative" };
            }

            if (order.PointsRedeemed > 0 && settings.LoyaltyPointValue <= 0)
            {
                return new ResultMessage { Success = false, Message = "LoyaltyPointsOff" };
            }

            if (order.PointsRedeemed > customer.LoyaltyPointsNumber)
            {
                return new ResultMessage { Success = false, Message = "NotEnoughPoints" };
            }

            var pointsValue = Math.Round(order.PointsRedeemed * settings.LoyaltyPointValue, 2);

            // 7. the bill and the order
            var billSetting = await _orderLedger.GetSettingAsync();
            var code = await GetNextCodeAsync();
            var now = DateTime.Now;

            var bill = new BillMaster
            {
                Code = code,
                CodeWithoutSuffix = code,
                Date = now,
                CurrencyRate = 1,
                BillSettingId = billSetting.Id,
                BillDiscountValue = (order.Bill?.BillDiscountValue ?? 0) + pointsValue,
                CustomerOrSupplierPhoneNumber = order.PhoneNumber,
                CustomerOrSupplierAccountId = customer?.AccountId,
                EmployeeId = order.EmployeeId,
                StoreId = lines.First().BillDetail.StoreId,
                IsStorageEffected = true,
                CreatedAt = now,
                BillDetails = lines.Select(a => a.BillDetail).ToList()
            };
            bill.BillDailyNo = await _context.BillMasters.CountAsync(a => a.BillSettingId == billSetting.Id && a.Date.Date == now.Date) + 1;

            var entity = new Order
            {
                Bill = bill,
                OrderCode = code,
                Type = order.Type,
                SellerId = userId,
                Cash = cash,
                PaymentMethod = cash && !order.OnAccount ? order.PaymentMethod ?? PaymentMethod.Cash : null,
                OnAccount = order.OnAccount,
                ReceiverId = cash ? userId : null,
                CustomerName = string.IsNullOrWhiteSpace(order.CustomerName) ? "عميل نقدي" : order.CustomerName.Trim(),
                CustomerId = order.CustomerId,
                TableId = order.TableId,
                EmployeeId = order.EmployeeId,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                DeliveryZoneId = deliveryZoneId,
                DeliveryFee = deliveryFee,
                PointsRedeemed = order.PointsRedeemed,
                Service = order.Type == OrderType.TakeAway ? 0 : settings.OrderService,
                Lines = lines
            };
            _orderLedger.RecalculateBill(entity, settings.OrderTax);

            // 8. the discount: never more than the bill, and never more than the settings limit unless the customer tier gives more
            var requestedDiscount = (order.Bill?.BillDiscountValue ?? 0) + pointsValue;
            if (requestedDiscount > bill.BillDiscountValue + 0.01m)
            {
                return new ResultMessage { Success = false, Message = "OrderDiscountTooBig" };
            }

            if (settings.OrderDiscount > 0)
            {
                var tierPercentage = customer.CustomerTierId.HasValue
                    ? await _context.CustomerTierDiscounts.AsNoTracking()
                        .Where(a => a.CustomerTierId == customer.CustomerTierId.Value)
                        .Select(a => a.DiscountPercentage)
                        .FirstOrDefaultAsync()
                    : 0;

                var gross = bill.TotalAmountBeforeDiscountOrExtra + bill.BillExtraValue;
                var tierDiscount = Math.Round(gross * (decimal)tierPercentage / 100, 2);
                if (bill.BillDiscountValue - pointsValue > Math.Max(settings.OrderDiscount, tierDiscount) + 0.01m)
                {
                    return new ResultMessage { Success = false, Message = "DiscountOverLimit" };
                }
            }

            // 9. the payments of a paid order (one method, or a part by visa and the rest by another method) must cover the bill
            var paymentError = await _orderLedger.SetPaymentsAsync(entity, order.Payments);
            if (paymentError is not null)
            {
                return new ResultMessage { Success = false, Message = paymentError };
            }

            // 10. customer balance and points, table and stock movements
            if (order.OnAccount)
            {
                customer.Account.Adjust(bill.TotalBillAmountAfterDiscountAndExtra);
            }

            customer.LoyaltyPointsNumber -= order.PointsRedeemed;
            if (cash && settings.LoyaltyAmountPerPoint > 0)
            {
                entity.PointsEarned = Math.Floor(bill.TotalBillAmountAfterDiscountAndExtra / settings.LoyaltyAmountPerPoint);
                customer.LoyaltyPointsNumber += entity.PointsEarned;
            }

            if (table is not null && !cash)
            {
                table.IsPending = true;
            }

            foreach (var movement in movements)
            {
                movement.ReferenceCode = entity.OrderCode;
            }
            await _context.StoreProductMovements.AddRangeAsync(movements);

            await _context.Orders.AddAsync(entity);
            await _context.SaveChangesAsync();

            // 11. a paid order gets its journal entry now
            if (entity.Cash)
            {
                if (billSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
                {
                    entity.Bill.IsEntryPosted = await _orderLedger.PostEntryAsync(entity);
                }

                await _context.SaveChangesAsync();
                await _billSupport.LinkEntryAsync(entity.Bill);
            }

            await transaction.CommitAsync();

            return new ResultMessage { Success = true, Data = entity.Id.ToString() };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Orders_OrderCode"))
        {
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method, List<OrderPaymentViewModel> payments)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            // already paid, nothing to do
            if (order.Cash)
            {
                return new ResultMessage { Success = true, Data = order.Id.ToString() };
            }

            if (order.Type == OrderType.Hall && order.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(order.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }

                var seated = await _context.TableReservations
                    .Where(a => a.TableId == order.TableId.Value && a.Status == ReservationStatus.Seated)
                    .ToListAsync();
                seated.ForEach(a => a.Status = ReservationStatus.Completed);
            }

            order.Cash = true;
            order.PaymentMethod = payments.FirstOrDefault()?.Method ?? method;
            order.ReceiverId = userId;
            order.Bill.Date = DateTime.Now;
            var settings = await GetSettingsAsync();
            _orderLedger.RecalculateBill(order, settings.OrderTax);
            var paymentError = await _orderLedger.SetPaymentsAsync(order, payments);
            if (paymentError is not null)
            {
                return new ResultMessage { Success = false, Message = paymentError };
            }

            // the customer earns loyalty points when the order is paid
            if (settings.LoyaltyAmountPerPoint > 0 && order.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value);
                if (customer is not null)
                {
                    order.PointsEarned = Math.Floor(order.Bill.TotalBillAmountAfterDiscountAndExtra / settings.LoyaltyAmountPerPoint);
                    customer.LoyaltyPointsNumber += order.PointsEarned;
                }
            }

            await _context.SaveChangesAsync();
            var billSetting = await _orderLedger.GetSettingAsync();
            if (billSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
            {
                order.Bill.IsEntryPosted = await _orderLedger.PostEntryAsync(order);
            }
            await _context.SaveChangesAsync();
            await _billSupport.LinkEntryAsync(order.Bill);
            await transaction.CommitAsync();

            return new ResultMessage { Success = true, Data = order.Id.ToString() };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> MoveItemAsync(int orderMenuItemId, int targetTableId)
    {
        try
        {
            var line = await _context.OrderMenuItems.Include(a => a.BillDetail).FirstOrDefaultAsync(a => a.Id == orderMenuItemId);
            if (line is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            var from = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .FirstOrDefaultAsync(a => a.Id == line.OrderId && !a.Cash && a.Type == OrderType.Hall);

            var to = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .FirstOrDefaultAsync(a => a.TableId == targetTableId && !a.Cash && a.Type == OrderType.Hall);

            if (from is null || to is null || from.Id == to.Id)
            {
                return new ResultMessage { Success = false, Message = "TargetTableHasNoOrder" };
            }

            var settings = await GetSettingsAsync();

            to.Lines.Add(line);
            line.BillDetail.BillMasterId = to.Id;
            _context.ChangeTracker.DetectChanges();

            // the orders that stay open get new totals
            var openOrders = new List<Order> { to };

            if (from.Lines.Any(a => a.OrderId == from.Id))
            {
                openOrders.Add(from);
            }
            else
            {
                await _orderLedger.RemoveOrderAsync(from);
            }

            foreach (var open in openOrders)
            {
                _orderLedger.RecalculateBill(open, settings.OrderTax);
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> MergeOrderAsync(int id, int targetTableId)
    {
        try
        {
            var from = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .FirstOrDefaultAsync(a => a.Id == id && !a.Cash && a.Type == OrderType.Hall);

            var to = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .FirstOrDefaultAsync(a => a.TableId == targetTableId && !a.Cash && a.Type == OrderType.Hall);

            if (from is null || to is null || from.Id == to.Id)
            {
                return new ResultMessage { Success = false, Message = "TargetTableHasNoOrder" };
            }

            var settings = await GetSettingsAsync();

            foreach (var line in from.Lines.ToList())
            {
                to.Lines.Add(line);
                line.BillDetail.BillMasterId = to.Id;
            }
            _context.ChangeTracker.DetectChanges();

            to.Service += from.Service;
            to.Bill.BillDiscountValue += from.Bill.BillDiscountValue;
            to.PointsRedeemed += from.PointsRedeemed;
            _orderLedger.RecalculateBill(to, settings.OrderTax);

            await _orderLedger.RemoveOrderAsync(from);

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> DeleteOrderAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                .Include(a => a.Lines).ThenInclude(a => a.Ingredients)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = true };
            }

            // give the ingredients back to the stock
            foreach (var line in order.Lines)
            {
                foreach (var ingredient in line.Ingredients)
                {
                    var adjusted = await _stock.AdjustAsync(ingredient.StoreId, ingredient.ProductId, ingredient.Quantity, allowNegative: true, createIfMissing: false);
                    if (adjusted)
                    {
                        await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                        {
                            StoreId = ingredient.StoreId,
                            ProductId = ingredient.ProductId,
                            QuantityDelta = ingredient.Quantity,
                            Source = StockMovementSource.OrderCancellation,
                            ReferenceCode = order.OrderCode
                        });
                    }
                }
            }

            // take the amount back from the customer account
            if (order.OnAccount && order.CustomerId.HasValue)
            {
                var customer = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value);
                if (customer is not null)
                {
                    customer.Account.Adjust(-order.TotalPrice);
                }
            }

            // give back the points the order spent and take back the points it earned
            if (order.CustomerId.HasValue && (order.PointsRedeemed > 0 || order.PointsEarned > 0))
            {
                var pointsOwner = await _context.Customers.FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value);
                if (pointsOwner is not null)
                {
                    pointsOwner.LoyaltyPointsNumber = Math.Max(0, pointsOwner.LoyaltyPointsNumber + order.PointsRedeemed - order.PointsEarned);
                }
            }

            await _orderLedger.RemoveOrderAsync(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> CompleteOrderAsync(int id)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            order.Complete = true;
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }
}
