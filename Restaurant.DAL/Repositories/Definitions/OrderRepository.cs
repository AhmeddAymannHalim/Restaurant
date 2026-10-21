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
    private readonly IShiftPasswordRepository _shiftPassword;
    private readonly IOrderLedgerRepository _orderLedger;
    private readonly IStockRepository _stock;
    private readonly IBillSupportRepository _billSupport;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
        _shiftPassword = new ShiftPasswordRepository();
        _orderLedger = new OrderLedgerRepository(context);
        _stock = new StockRepository(context);
        _billSupport = new BillSupportRepository(context);
    }

    // ---------------------------------------------------------------- shift

    public async Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId)
    {
        var openOrders = _context.Orders.AsNoTracking().Where(a => !a.FinishShift);

        var model = new ShiftDashboardViewModel();

        var paidTypes = new[] { OrderType.Hall, OrderType.Delivery, OrderType.TakeAway };
        foreach (var type in paidTypes)
        {
            var paid = openOrders.Where(a => a.Type == type
                && a.Cash
                && !a.OnAccount
                && a.SellerId == userId
                && a.ReceiverId == userId);
            model.Collected.Add(new ShiftRowViewModel
            {
                Type = type,
                Count = await paid.CountAsync(),
                Amount = await paid.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0
            });
        }

        var unpaidTypes = new[] { OrderType.Hall, OrderType.Delivery };
        foreach (var type in unpaidTypes)
        {
            var mine = openOrders.Where(a => a.Type == type && !a.Cash && a.SellerId == userId);
            model.PendingMine.Add(new ShiftRowViewModel
            {
                Type = type,
                Count = await mine.CountAsync(),
                Amount = await mine.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0
            });

            var others = openOrders.Where(a => a.Type == type && !a.Cash && a.SellerId != userId);
            model.PendingOthers.Add(new ShiftRowViewModel
            {
                Type = type,
                Count = await others.CountAsync(),
                Amount = await others.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0
            });
        }

        var fromOthers = openOrders.Where(a => a.Cash && !a.OnAccount && a.SellerId != userId && a.ReceiverId == userId);
        model.CollectedFromOthers.Count = await fromOthers.CountAsync();
        model.CollectedFromOthers.Amount = await fromOthers.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

        var electronic = openOrders.Where(a => a.Cash
            && !a.OnAccount
            && a.PaymentMethod != null
            && a.PaymentMethod != PaymentMethod.Cash
            && a.ReceiverId == userId);
        model.Electronic.Count = await electronic.CountAsync();
        model.Electronic.Amount = await electronic.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

        var onAccount = openOrders.Where(a => a.OnAccount && (a.SellerId == userId || a.ReceiverId == userId));
        model.OnAccount.Count = await onAccount.CountAsync();
        model.OnAccount.Amount = await onAccount.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

        model.LastShiftReport = await _context.ShiftCloseReports.AsNoTracking()
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

        return model;
    }

    public async Task<ResultMessage> FinishShiftAsync(string userId, decimal countedCash)
    {
        try
        {
            if (countedCash < 0)
            {
                return new ResultMessage { Success = false, Message = "CountedCashInvalid" };
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
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<bool> VerifyShiftPasswordAsync(string password)
    {
        var setting = await _context.ApplicationSettings.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
        if (setting is null || !_shiftPassword.Verify(setting.Password, password, out var needsRehash))
        {
            return false;
        }

        if (needsRehash)
        {
            setting.Password = _shiftPassword.Hash(password);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<IEnumerable<Order>> GetIncompleteCashOrdersAsync(string userId)
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Bill)
            .Include(a => a.Table)
            .Where(a => a.Cash && !a.FinishShift && a.ReceiverId == userId && !a.Complete)
            .OrderBy(a => a.Bill.Date)
            .ToListAsync();
    }

    // ---------------------------------------------------------------- read

    public async Task<PaginatedResult<Order>> GetPagedOrdersAsync(PageRequest request, string userId)
    {
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
                    if (component.Quantity <= 0)
                    {
                        continue;
                    }

                    var inStock = _stock.FindQuantity(stock, dish.StoreId, component.ProductId);
                    portions = Math.Min(portions, (int)Math.Floor(inStock / component.Quantity));
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

            // 2. the customer, only when the order is on account
            Customer customer = null;
            if (order.OnAccount)
            {
                customer = order.CustomerId.HasValue
                    ? await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value)
                    : null;
                if (customer is null)
                {
                    return new ResultMessage { Success = false, Message = "CustomerRequired" };
                }
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
                if (cash)
                {
                    order.TableId = null;
                }
                else
                {
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
            }
            else
            {
                order.TableId = null;
                cash = true;
                if (!order.EmployeeId.HasValue || !await _context.Employees.AnyAsync(a => a.Id == order.EmployeeId))
                {
                    return new ResultMessage { Success = false, Message = "DeliveryDriverRequired" };
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
                    var used = quantity * component.Quantity;
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

            // 6. the bill and the order
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
                BillDiscountValue = order.Bill?.BillDiscountValue ?? 0,
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
                Service = order.Type == OrderType.TakeAway ? 0 : settings.OrderService,
                Lines = lines
            };
            entity.Tax = Math.Round(entity.Lines.Sum(a => a.Price) * settings.OrderTax / 100, 2);
            var subTotal = entity.Lines.Sum(a => a.Price);
            var extras = entity.Tax + entity.Service + entity.DeliveryFee;
            var discount = Math.Min(Math.Max(0, entity.Bill.BillDiscountValue), subTotal + extras);
            var total = subTotal + extras - discount;
            var cost = entity.Lines.Sum(a => a.UnitCost * a.Quantity);
            entity.Bill.TotalAmountBeforeDiscountOrExtra = subTotal;
            entity.Bill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
            entity.Bill.BillDiscountValue = discount;
            entity.Bill.TotalDiscountValueOnBillAndItems = discount;
            entity.Bill.BillExtraValue = extras;
            entity.Bill.TotalExtraValueOnBillAndItems = extras;
            entity.Bill.TotalBillAmountAfterDiscountAndExtra = total;
            entity.Bill.BillCost = cost;
            entity.Bill.BillProfit = total - cost;
            entity.Bill.PayType = entity.OnAccount ? PayType.Later : PayType.Cash;
            entity.Bill.IsEntryPosted = false;
            entity.Bill.AmountPaided = entity.Cash && !entity.OnAccount ? total : 0;
            entity.Bill.CasherRemainingAmount = total - entity.Bill.AmountPaided;
            entity.Bill.UpdatedAt = DateTime.Now;

            // 7. customer balance, table and stock movements
            if (customer is not null)
            {
                customer.Account.Adjust(bill.TotalBillAmountAfterDiscountAndExtra);
            }

            if (table is not null)
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

            // 8. a paid order gets its journal entry now
            if (entity.Cash)
            {
                if (billSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
                {
                    entity.Bill.IsEntryPosted = await _orderLedger.PostEntryAsync(entity);
                }

                await _context.SaveChangesAsync();
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

    public async Task<ResultMessage> PayOrderAsync(int id, string userId, PaymentMethod method)
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
            order.PaymentMethod = method;
            order.ReceiverId = userId;
            order.Bill.Date = DateTime.Now;
            var settings = await GetSettingsAsync();
            order.Tax = Math.Round(order.Lines.Sum(a => a.Price) * settings.OrderTax / 100, 2);
            var subTotal = order.Lines.Sum(a => a.Price);
            var extras = order.Tax + order.Service + order.DeliveryFee;
            var discount = Math.Min(Math.Max(0, order.Bill.BillDiscountValue), subTotal + extras);
            var total = subTotal + extras - discount;
            var cost = order.Lines.Sum(a => a.UnitCost * a.Quantity);
            order.Bill.TotalAmountBeforeDiscountOrExtra = subTotal;
            order.Bill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
            order.Bill.BillDiscountValue = discount;
            order.Bill.TotalDiscountValueOnBillAndItems = discount;
            order.Bill.BillExtraValue = extras;
            order.Bill.TotalExtraValueOnBillAndItems = extras;
            order.Bill.TotalBillAmountAfterDiscountAndExtra = total;
            order.Bill.BillCost = cost;
            order.Bill.BillProfit = total - cost;
            order.Bill.PayType = order.OnAccount ? PayType.Later : PayType.Cash;
            order.Bill.IsEntryPosted = false;
            order.Bill.AmountPaided = order.Cash && !order.OnAccount ? total : 0;
            order.Bill.CasherRemainingAmount = total - order.Bill.AmountPaided;
            order.Bill.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            var billSetting = await _orderLedger.GetSettingAsync();
            if (billSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
            {
                order.Bill.IsEntryPosted = await _orderLedger.PostEntryAsync(order);
            }
            await _context.SaveChangesAsync();
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
                // the last dish moved away, so the source order is deleted
                if (from.Type == OrderType.Hall && from.TableId.HasValue)
                {
                    var freedTable = await _context.Tables.FindAsync(from.TableId.Value);
                    if (freedTable is not null)
                    {
                        freedTable.IsPending = false;
                    }
                }

                await _orderLedger.RemoveEntryAsync(from.Id);
                var fromLines = from.Lines.Where(a => a.OrderId == from.Id).ToList();
                _context.OrderIngredients.RemoveRange(fromLines.SelectMany(a => a.Ingredients));
                _context.OrderMenuItems.RemoveRange(fromLines);
                _context.BillDetails.RemoveRange(fromLines.Select(a => a.BillDetail).Where(a => a is not null));
                _context.Orders.Remove(from);
                if (from.Bill is not null)
                {
                    _context.BillMasters.Remove(from.Bill);
                }
            }

            foreach (var open in openOrders)
            {
                open.Tax = Math.Round(open.Lines.Sum(a => a.Price) * settings.OrderTax / 100, 2);
                var subTotal = open.Lines.Sum(a => a.Price);
                var extras = open.Tax + open.Service + open.DeliveryFee;
                var discount = Math.Min(Math.Max(0, open.Bill.BillDiscountValue), subTotal + extras);
                var total = subTotal + extras - discount;
                var cost = open.Lines.Sum(a => a.UnitCost * a.Quantity);
                open.Bill.TotalAmountBeforeDiscountOrExtra = subTotal;
                open.Bill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
                open.Bill.BillDiscountValue = discount;
                open.Bill.TotalDiscountValueOnBillAndItems = discount;
                open.Bill.BillExtraValue = extras;
                open.Bill.TotalExtraValueOnBillAndItems = extras;
                open.Bill.TotalBillAmountAfterDiscountAndExtra = total;
                open.Bill.BillCost = cost;
                open.Bill.BillProfit = total - cost;
                open.Bill.PayType = open.OnAccount ? PayType.Later : PayType.Cash;
                open.Bill.IsEntryPosted = false;
                open.Bill.AmountPaided = open.Cash && !open.OnAccount ? total : 0;
                open.Bill.CasherRemainingAmount = total - open.Bill.AmountPaided;
                open.Bill.UpdatedAt = DateTime.Now;
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
            to.Tax = Math.Round(to.Lines.Sum(a => a.Price) * settings.OrderTax / 100, 2);
            var subTotal = to.Lines.Sum(a => a.Price);
            var extras = to.Tax + to.Service + to.DeliveryFee;
            var discount = Math.Min(Math.Max(0, to.Bill.BillDiscountValue), subTotal + extras);
            var total = subTotal + extras - discount;
            var cost = to.Lines.Sum(a => a.UnitCost * a.Quantity);
            to.Bill.TotalAmountBeforeDiscountOrExtra = subTotal;
            to.Bill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
            to.Bill.BillDiscountValue = discount;
            to.Bill.TotalDiscountValueOnBillAndItems = discount;
            to.Bill.BillExtraValue = extras;
            to.Bill.TotalExtraValueOnBillAndItems = extras;
            to.Bill.TotalBillAmountAfterDiscountAndExtra = total;
            to.Bill.BillCost = cost;
            to.Bill.BillProfit = total - cost;
            to.Bill.PayType = to.OnAccount ? PayType.Later : PayType.Cash;
            to.Bill.IsEntryPosted = false;
            to.Bill.AmountPaided = to.Cash && !to.OnAccount ? total : 0;
            to.Bill.CasherRemainingAmount = total - to.Bill.AmountPaided;
            to.Bill.UpdatedAt = DateTime.Now;

            if (from.Type == OrderType.Hall && from.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(from.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }
            }

            await _orderLedger.RemoveEntryAsync(from.Id);
            var fromLines = from.Lines.Where(a => a.OrderId == from.Id).ToList();
            _context.OrderIngredients.RemoveRange(fromLines.SelectMany(a => a.Ingredients));
            _context.OrderMenuItems.RemoveRange(fromLines);
            _context.BillDetails.RemoveRange(fromLines.Select(a => a.BillDetail).Where(a => a is not null));
            _context.Orders.Remove(from);
            if (from.Bill is not null)
            {
                _context.BillMasters.Remove(from.Bill);
            }

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

            if (order.FinishShift)
            {
                return new ResultMessage { Success = false, Message = "ShiftClosed" };
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

            if (order.Type == OrderType.Hall && order.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(order.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }
            }

            await _orderLedger.RemoveEntryAsync(order.Id);
            var orderLines = order.Lines.Where(a => a.OrderId == order.Id).ToList();
            _context.OrderIngredients.RemoveRange(orderLines.SelectMany(a => a.Ingredients));
            _context.OrderMenuItems.RemoveRange(orderLines);
            _context.BillDetails.RemoveRange(orderLines.Select(a => a.BillDetail).Where(a => a is not null));
            _context.Orders.Remove(order);
            if (order.Bill is not null)
            {
                _context.BillMasters.Remove(order.Bill);
            }

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
