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

    public async Task<ShiftDashboardViewModel> GetShiftDashboardAsync(string userId)
    {
        var paidTypes = new[] { OrderType.Hall, OrderType.Delivery, OrderType.Delivery_Online, OrderType.TakeAway };
        var unpaidTypes = new[] { OrderType.Hall, OrderType.Delivery, OrderType.Delivery_Online };

        var openOrders = _context.Orders.AsNoTracking().Where(a => !a.FinishShift);

        var collectedRows = new List<ShiftRowViewModel>();
        foreach (var type in paidTypes)
        {
            var bucket = openOrders.Where(a => a.Type == type
                && a.Cash
                && !a.OnAccount
                && a.SellerId == userId
                && a.ReceiverId == userId);
            var count = await bucket.CountAsync();
            var amount = await bucket.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;
            collectedRows.Add(new ShiftRowViewModel { Type = type, Count = count, Amount = amount });
        }

        var pendingMineRows = new List<ShiftRowViewModel>();
        foreach (var type in unpaidTypes)
        {
            var bucket = openOrders.Where(a => a.Type == type && !a.Cash && a.SellerId == userId);
            var count = await bucket.CountAsync();
            var amount = await bucket.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;
            pendingMineRows.Add(new ShiftRowViewModel { Type = type, Count = count, Amount = amount });
        }

        var pendingOthersRows = new List<ShiftRowViewModel>();
        foreach (var type in unpaidTypes)
        {
            var bucket = openOrders.Where(a => a.Type == type && !a.Cash && a.SellerId != userId);
            var count = await bucket.CountAsync();
            var amount = await bucket.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;
            pendingOthersRows.Add(new ShiftRowViewModel { Type = type, Count = count, Amount = amount });
        }

        var fromOthersQuery = openOrders.Where(a => a.Cash
            && !a.OnAccount
            && a.SellerId != userId
            && a.ReceiverId == userId);
        var fromOthersCount = await fromOthersQuery.CountAsync();
        var fromOthersAmount = await fromOthersQuery.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

        var electronicQuery = openOrders.Where(a => a.Cash
            && !a.OnAccount
            && a.PaymentMethod != null
            && a.PaymentMethod != PaymentMethod.Cash
            && a.ReceiverId == userId);
        var electronicCount = await electronicQuery.CountAsync();
        var electronicAmount = await electronicQuery.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

        var onAccountQuery = openOrders.Where(a => a.OnAccount
            && (a.SellerId == userId || a.ReceiverId == userId));
        var onAccountCount = await onAccountQuery.CountAsync();
        var onAccountAmount = await onAccountQuery.SumAsync(a => (decimal?)a.Bill.TotalBillAmountAfterDiscountAndExtra) ?? 0;

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
            Collected = collectedRows,
            PendingMine = pendingMineRows,
            PendingOthers = pendingOthersRows,
            CollectedFromOthers = new ShiftRowViewModel
            {
                Count = fromOthersCount,
                Amount = fromOthersAmount
            },
            Electronic = new ShiftRowViewModel
            {
                Count = electronicCount,
                Amount = electronicAmount
            },
            OnAccount = new ShiftRowViewModel
            {
                Count = onAccountCount,
                Amount = onAccountAmount
            },
            LastShiftReport = lastReport
        };
    }

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

    public async Task<IEnumerable<MenuItem>> GetCashierMenuAsync()
    {
        var menu = await _context.MenuItems.AsNoTracking()
            .Include(a => a.MenuSection)
            .Include(a => a.Components).ThenInclude(a => a.Product)
            .OrderBy(a => a.MenuSection.ArName)
            .ThenBy(a => a.ArName)
            .ToListAsync();

        var portions = await GetAvailablePortionsAsync();
        foreach (var dish in menu)
        {
            if (portions.ContainsKey(dish.Id))
            {
                dish.AvailablePortions = portions[dish.Id];
            }

            if (dish.AvailablePortions <= 0)
            {
                dish.IsAvailable = false;
            }
        }

        return menu;
    }

    public async Task<Dictionary<int, int>> GetAvailablePortionsAsync()
    {
        var stock = await _stock.QuantitiesAsync();
        var menuItems = await _context.MenuItems
            .AsNoTracking()
            .Include(a => a.Components)
            .Where(a => a.Components.Any())
            .ToListAsync();

        var portionsByMenuItem = new Dictionary<int, int>();
        foreach (var menuItem in menuItems)
        {
            portionsByMenuItem[menuItem.Id] = CountPortions(menuItem, stock);
        }

        return portionsByMenuItem;
    }

    private void AddNeed(List<StockQuantity> needs, int storeId, int productId, decimal quantity)
    {
        foreach (var need in needs)
        {
            if (need.StoreId == storeId && need.ProductId == productId)
            {
                need.Quantity += quantity;
                return;
            }
        }

        needs.Add(new StockQuantity { StoreId = storeId, ProductId = productId, Quantity = quantity });
    }

    private int CountPortions(MenuItem menuItem, List<StockQuantity> stock)
    {
        var portions = int.MaxValue;

        foreach (var component in menuItem.Components)
        {
            if (component.Quantity <= 0)
            {
                continue;
            }

            var inStock = _stock.FindQuantity(stock, menuItem.StoreId, component.ProductId);
            var possible = (int)Math.Floor(inStock / component.Quantity);
            if (possible < portions)
            {
                portions = possible;
            }
        }

        return portions;
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
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var settings = await GetSettingsAsync();

            var wanted = order.Lines
                .GroupBy(a => a.MenuItemId)
                .Select(g => new
                    {
                        MenuItemId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                .ToList();
            if (wanted.Count == 0)
            {
                return new ResultMessage { Success = false, Message = "AtLeastOneItem" };
            }

            var ids = wanted.Select(a => a.MenuItemId).ToList();
            var menu = await _context.MenuItems
                .Include(a => a.Components).ThenInclude(a => a.Product)
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();
            if (menu.Count != ids.Count)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (menu.Any(a => !a.IsAvailable))
            {
                return new ResultMessage { Success = false, Message = "MenuItemNotAvailable" };
            }

            Customer customer = null;
            if (order.OnAccount)
            {
                customer = order.CustomerId.HasValue ? await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value) : null;
                if (customer is null)
                {
                    return new ResultMessage { Success = false, Message = "CustomerRequired" };
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
                            return new ResultMessage { Success = false, Message = "TableRequired" };
                        }

                        if (table.IsPending)
                        {
                            if (!onlineOrderId.HasValue)
                            {
                                return new ResultMessage { Success = false, Message = "TableBusy" };
                            }

                            mergeInto = await _context.Orders
                                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                                .Include(a => a.Lines).ThenInclude(a => a.BillDetail)
                                .FirstOrDefaultAsync(a => a.TableId == table.Id
                                    && !a.Cash
                                    && a.Type == OrderType.Hall);
                            if (mergeInto is null)
                            {
                                return new ResultMessage { Success = false, Message = "TableBusy" };
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
                        return new ResultMessage { Success = false, Message = "RecordNotFound" };
                    }

                    if (!order.EmployeeId.HasValue
                        || !await _context.Employees.AnyAsync(a => a.Id == order.EmployeeId))
                    {
                        return new ResultMessage { Success = false, Message = "DeliveryDriverRequired" };
                    }
                    break;
            }

            var posSetting = await _orderLedger.GetSettingAsync();
            var itemIds = await _orderLedger.GetItemIdsAsync(menu);

            var lines = wanted.Select((w, index) =>
            {
                var dish = menu.First(a => a.Id == w.MenuItemId);
                return new OrderMenuItem
                {
                    MenuItemId = dish.Id,
                    BillDetail = new BillDetail
                    {
                        RowNumber = (byte)Math.Min(index + 1, byte.MaxValue),
                        ItemId = itemIds[dish.Id],
                        StoreId = dish.StoreId,
                        Quantity = w.Quantity,
                        UnitPrice = dish.Price,
                        TransRate = 1,
                        UnitCostAtPosting = dish.TotalCost,
                        TotalAmount = Math.Round(w.Quantity * dish.Price, 2),
                        CreatedAt = DateTime.Now
                    }
                };
            }).ToList();

            var needs = new List<StockQuantity>();
            foreach (var line in lines)
            {
                var dish = menu.First(a => a.Id == line.MenuItemId);
                foreach (var component in dish.Components)
                {
                    var used = line.Quantity * component.Quantity;
                    AddNeed(needs, dish.StoreId, component.ProductId, used);

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
                if (!await _stock.AdjustAsync(need.StoreId, need.ProductId, -need.Quantity, createIfMissing: false))
                {
                    return new ResultMessage { Success = false, Message = "InsufficientStock" };
                }

                movements.Add(new StoreProductMovement
                {
                    StoreId = need.StoreId,
                    ProductId = need.ProductId,
                    QuantityDelta = -need.Quantity,
                    Source = StockMovementSource.Order
                });
            }

            if (mergeInto is not null)
            {
                foreach (var line in lines)
                {
                    line.BillDetail.BillMasterId = mergeInto.Id;
                    mergeInto.Lines.Add(line);
                    mergeInto.Bill.BillDetails.Add(line.BillDetail);
                }

                
    {
        var recalcBill = mergeInto.Bill;
        var subTotal = mergeInto.Lines.Sum(a => a.Price);
        mergeInto.Tax = Math.Round(subTotal *  settings.OrderTax / 100, 2);

        var extras = mergeInto.Tax + mergeInto.Service + mergeInto.DeliveryFee;
        var gross = subTotal + extras;
        var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
        var discount = Math.Min(discountFloor, gross);
        var total = gross - discount;
        decimal cost = 0;
        foreach (var costLine in mergeInto.Lines)
        {
            cost += costLine.UnitCost * costLine.Quantity;
        }

        recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
        recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        recalcBill.BillDiscountValue = discount;
        recalcBill.TotalDiscountValueOnBillAndItems = discount;
        recalcBill.BillExtraValue = extras;
        recalcBill.TotalExtraValueOnBillAndItems = extras;
        recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
        var costLimit = 999_999_999_999m;
        recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
        recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
        recalcBill.PayType = mergeInto.OnAccount ? PayType.Later : PayType.Cash;
        recalcBill.IsEntryPosted = false;
        recalcBill.AmountPaided = mergeInto.Cash && !mergeInto.OnAccount ? total : 0;
        recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
        recalcBill.UpdatedAt = DateTime.Now;
    }


                var waiting = await _context.OnlineOrders.FirstOrDefaultAsync(a => a.Id == onlineOrderId.Value);
                if (waiting is not null && waiting.Status == OnlineOrderStatus.Rejected)
                {
                    return new ResultMessage { Success = false, Message = "OnlineOrderRejected" };
                }
                if (waiting is null || !waiting.IsPending)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
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
                await transaction.CommitAsync();

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
                            return new ResultMessage { Success = false, Message = "DeliveryZoneRequired" };
                        }

                        deliveryZoneId = zone.Id;
                        deliveryFee = zone.Fee;
                    }
                }
            }

            var now = DateTime.Now;
            var code = await GetNextCodeAsync();
            var phone = order.PhoneNumber;
            var bill = new BillMaster
            {
                Code = code,
                CodeWithoutSuffix = code,
                Date = now,
                CurrencyRate = 1,
                BillSettingId = posSetting.Id,
                BillDiscountValue = Math.Max(0, order.Bill?.BillDiscountValue ?? 0),
                CustomerOrSupplierPhoneNumber = phone,
                CustomerOrSupplierAccountId = customer?.AccountId,
                EmployeeId = order.EmployeeId,
                StoreId = lines.Select(a => a.BillDetail.StoreId).FirstOrDefault(),
                IsStorageEffected = true,
                CreatedAt = now,
                BillDetails = lines.Select(a => a.BillDetail).ToList()
            };
            bill.BillDailyNo = await _context.BillMasters.CountAsync(a => a.BillSettingId == posSetting.Id && a.Date.Date == now.Date) + 1;

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
                Lines = lines
            };
            
    {
        var recalcBill = entity.Bill;
        var subTotal = entity.Lines.Sum(a => a.Price);
        entity.Tax = Math.Round(subTotal *  settings.OrderTax / 100, 2);

        var extras = entity.Tax + entity.Service + entity.DeliveryFee;
        var gross = subTotal + extras;
        var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
        var discount = Math.Min(discountFloor, gross);
        var total = gross - discount;
        decimal cost = 0;
        foreach (var costLine in entity.Lines)
        {
            cost += costLine.UnitCost * costLine.Quantity;
        }

        recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
        recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        recalcBill.BillDiscountValue = discount;
        recalcBill.TotalDiscountValueOnBillAndItems = discount;
        recalcBill.BillExtraValue = extras;
        recalcBill.TotalExtraValueOnBillAndItems = extras;
        recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
        var costLimit = 999_999_999_999m;
        recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
        recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
        recalcBill.PayType = entity.OnAccount ? PayType.Later : PayType.Cash;
        recalcBill.IsEntryPosted = false;
        recalcBill.AmountPaided = entity.Cash && !entity.OnAccount ? total : 0;
        recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
        recalcBill.UpdatedAt = DateTime.Now;
    }


            if (customer is not null)
            {
                customer.Account.Adjust(bill.TotalBillAmountAfterDiscountAndExtra);
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
                    return new ResultMessage { Success = false, Message = "OnlineOrderRejected" };
                }
                if (online is null || !online.IsPending)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
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
                var postSetting = await _orderLedger.GetSettingAsync();
            if (postSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
            {
                await _orderLedger.PostEntryAsync(entity);
                entity.Bill.IsEntryPosted = true;
            }

            await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new ResultMessage
            {
                Success = true,
                Data = entity.Id.ToString()
            };
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
        try
        {
            var order = await _context.Orders
                .Include(a => a.Bill).ThenInclude(a => a.BillDetails)
                .Include(a => a.Lines).ThenInclude(a => a.BillDetail).FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (order.Cash)
            {
                return new ResultMessage
                {
                    Success = true,
                    Data = order.Id.ToString()
                };
            }

            if (order.Type == OrderType.Hall && order.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(order.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }
            }
            order.Cash = true;
            order.PaymentMethod = method;
            order.ReceiverId = userId;
            order.Bill.Date = DateTime.Now;
            var recalcSettings = await GetSettingsAsync();
            {
                var recalcBill = order.Bill;
                var subTotal = order.Lines.Sum(a => a.Price);
                order.Tax = Math.Round(subTotal * recalcSettings.OrderTax / 100, 2);

                var extras = order.Tax + order.Service + order.DeliveryFee;
                var gross = subTotal + extras;
                var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
                var discount = Math.Min(discountFloor, gross);
                var total = gross - discount;

                decimal cost = 0;
                foreach (var costLine in order.Lines)
                {
                    cost += costLine.UnitCost * costLine.Quantity;
                }

                recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
                recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
                recalcBill.BillDiscountValue = discount;
                recalcBill.TotalDiscountValueOnBillAndItems = discount;
                recalcBill.BillExtraValue = extras;
                recalcBill.TotalExtraValueOnBillAndItems = extras;
                recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
                var costLimit = 999_999_999_999m;
                recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
                recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
                recalcBill.PayType = order.OnAccount ? PayType.Later : PayType.Cash;
                recalcBill.IsEntryPosted = false;
                recalcBill.AmountPaided = order.Cash && !order.OnAccount ? total : 0;
                recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
                recalcBill.UpdatedAt = DateTime.Now;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();
            var postSetting = await _orderLedger.GetSettingAsync();
            if (postSetting.EntryGenerate == EntryGenerate.AutomaticEntryCreated)
            {
                await _orderLedger.PostEntryAsync(order);
                order.Bill.IsEntryPosted = true;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage
            {
                Success = true,
                Data = order.Id.ToString()
            };
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

            if (!from.Lines.Any(a => a.OrderId == from.Id))
            {
                if (from.Type == OrderType.Hall && from.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(from.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }
            }
                await _orderLedger.RemoveEntryAsync(from.Id);

            var ownLines = from.Lines.Where(a => a.OrderId == from.Id).ToList();
            var ownDetails = ownLines.Select(a => a.BillDetail).Where(a => a is not null).ToList();
            _context.OrderIngredients.RemoveRange(ownLines.SelectMany(a => a.Ingredients));
            _context.OrderMenuItems.RemoveRange(ownLines);
            _context.Orders.Remove(from);
            _context.BillDetails.RemoveRange(ownDetails);
            if (from.Bill is not null)
            {
                _context.BillMasters.Remove(from.Bill);
            }
            }
            else
            {
                
    {
        var recalcBill = from.Bill;
        var subTotal = from.Lines.Sum(a => a.Price);
        from.Tax = Math.Round(subTotal *  settings.OrderTax / 100, 2);

        var extras = from.Tax + from.Service + from.DeliveryFee;
        var gross = subTotal + extras;
        var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
        var discount = Math.Min(discountFloor, gross);
        var total = gross - discount;
        decimal cost = 0;
        foreach (var costLine in from.Lines)
        {
            cost += costLine.UnitCost * costLine.Quantity;
        }

        recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
        recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        recalcBill.BillDiscountValue = discount;
        recalcBill.TotalDiscountValueOnBillAndItems = discount;
        recalcBill.BillExtraValue = extras;
        recalcBill.TotalExtraValueOnBillAndItems = extras;
        recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
        var costLimit = 999_999_999_999m;
        recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
        recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
        recalcBill.PayType = from.OnAccount ? PayType.Later : PayType.Cash;
        recalcBill.IsEntryPosted = false;
        recalcBill.AmountPaided = from.Cash && !from.OnAccount ? total : 0;
        recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
        recalcBill.UpdatedAt = DateTime.Now;
    }

            }

            
    {
        var recalcBill = to.Bill;
        var subTotal = to.Lines.Sum(a => a.Price);
        to.Tax = Math.Round(subTotal *  settings.OrderTax / 100, 2);

        var extras = to.Tax + to.Service + to.DeliveryFee;
        var gross = subTotal + extras;
        var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
        var discount = Math.Min(discountFloor, gross);
        var total = gross - discount;
        decimal cost = 0;
        foreach (var costLine in to.Lines)
        {
            cost += costLine.UnitCost * costLine.Quantity;
        }

        recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
        recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        recalcBill.BillDiscountValue = discount;
        recalcBill.TotalDiscountValueOnBillAndItems = discount;
        recalcBill.BillExtraValue = extras;
        recalcBill.TotalExtraValueOnBillAndItems = extras;
        recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
        var costLimit = 999_999_999_999m;
        recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
        recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
        recalcBill.PayType = to.OnAccount ? PayType.Later : PayType.Cash;
        recalcBill.IsEntryPosted = false;
        recalcBill.AmountPaided = to.Cash && !to.OnAccount ? total : 0;
        recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
        recalcBill.UpdatedAt = DateTime.Now;
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
            
    {
        var recalcBill = to.Bill;
        var subTotal = to.Lines.Sum(a => a.Price);
        to.Tax = Math.Round(subTotal *  settings.OrderTax / 100, 2);

        var extras = to.Tax + to.Service + to.DeliveryFee;
        var gross = subTotal + extras;
        var discountFloor = Math.Max(0, recalcBill.BillDiscountValue);
        var discount = Math.Min(discountFloor, gross);
        var total = gross - discount;
        decimal cost = 0;
        foreach (var costLine in to.Lines)
        {
            cost += costLine.UnitCost * costLine.Quantity;
        }

        recalcBill.TotalAmountBeforeDiscountOrExtra = subTotal;
        recalcBill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        recalcBill.BillDiscountValue = discount;
        recalcBill.TotalDiscountValueOnBillAndItems = discount;
        recalcBill.BillExtraValue = extras;
        recalcBill.TotalExtraValueOnBillAndItems = extras;
        recalcBill.TotalBillAmountAfterDiscountAndExtra = total;
        var costLimit = 999_999_999_999m;
        recalcBill.BillCost = Math.Abs(cost) > costLimit ? null : cost;
        recalcBill.BillProfit = Math.Abs(total - cost) > costLimit ? null : total - cost;
        recalcBill.PayType = to.OnAccount ? PayType.Later : PayType.Cash;
        recalcBill.IsEntryPosted = false;
        recalcBill.AmountPaided = to.Cash && !to.OnAccount ? total : 0;
        recalcBill.CasherRemainingAmount = total - recalcBill.AmountPaided;
        recalcBill.UpdatedAt = DateTime.Now;
    }


            if (from.Type == OrderType.Hall && from.TableId.HasValue)
            {
                var freedTable = await _context.Tables.FindAsync(from.TableId.Value);
                if (freedTable is not null)
                {
                    freedTable.IsPending = false;
                }
            }
            await _orderLedger.RemoveEntryAsync(from.Id);

            var ownLines = from.Lines.Where(a => a.OrderId == from.Id).ToList();
            var ownDetails = ownLines.Select(a => a.BillDetail).Where(a => a is not null).ToList();
            _context.OrderIngredients.RemoveRange(ownLines.SelectMany(a => a.Ingredients));
            _context.OrderMenuItems.RemoveRange(ownLines);
            _context.Orders.Remove(from);
            _context.BillDetails.RemoveRange(ownDetails);
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
                .Include(a => a.Lines).ThenInclude(a => a.MenuItem).ThenInclude(a => a.Components)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (order is null)
            {
                return new ResultMessage { Success = true };
            }

            if (order.FinishShift)
            {
                return new ResultMessage { Success = false, Message = "ShiftClosed" };
            }

            foreach (var line in order.Lines)
            {
                var ingredients = line.Ingredients
                    .Select(a => new { a.StoreId, a.ProductId, a.Quantity })
                    .ToList();

                if (ingredients.Count == 0)
                {
                    ingredients = line.MenuItem.Components
                        .Select(a =>
                        {
                            var componentQuantity = line.Quantity * a.Quantity;

                            return new { StoreId = line.MenuItem.StoreId, a.ProductId, Quantity = componentQuantity };
                        })
                        .ToList();
                }

                foreach (var ingredient in ingredients)
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

            var ownLines = order.Lines.Where(a => a.OrderId == order.Id).ToList();
            var ownDetails = ownLines.Select(a => a.BillDetail).Where(a => a is not null).ToList();
            _context.OrderIngredients.RemoveRange(ownLines.SelectMany(a => a.Ingredients));
            _context.OrderMenuItems.RemoveRange(ownLines);
            _context.Orders.Remove(order);
            _context.BillDetails.RemoveRange(ownDetails);
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

    public async Task<IEnumerable<Order>> GetIncompleteCashOrdersAsync(string userId)
    {
        return await _context.Orders.AsNoTracking()
            .Include(a => a.Bill)
            .Include(a => a.Table)
            .Where(a => a.Cash && !a.FinishShift && a.ReceiverId == userId && !a.Complete)
            .OrderBy(a => a.Bill.Date)
            .ToListAsync();
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
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> RejectOnlineOrderAsync(int id, string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return new ResultMessage { Success = false, Message = "RejectionReasonRequired" };
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
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

}
