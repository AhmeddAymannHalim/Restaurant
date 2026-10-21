using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using PaymentMethod = Shared.Enums.PaymentMethod;

namespace Restaurant.Tests;

public class OrderRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly OrderRepository _sut;
    private readonly StockRepository _stock;

    private string _user;
    private string _otherUser;
    private Store _store;
    private Product _flour;
    private MenuItem _dish;
    private Table _table;
    private Table _table2;

    public OrderRepositoryTests()
    {
        _sut = new OrderRepository(_db.Context);
        _stock = new StockRepository(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    // A dish of price 50 that uses 2 flour. 100 flour in stock, 10% tax, 5 service.
    private async Task Seed(decimal flourInStock = 100)
    {
        var seller = new ApplicationUser { UserName = "cashier", Email = "c@test.com" };
        var other = new ApplicationUser { UserName = "cashier2", Email = "c2@test.com" };
        _store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        _flour = new Product { Code = "1", ArName = "Flour", EnName = "Flour", Cost = 2 };
        var section = new MenuSection { Code = "1", ArName = "Section", EnName = "Section" };
        _table = new Table { Code = "1", ArName = "T1", EnName = "T1", NumOfPersons = 4 };
        _table2 = new Table { Code = "2", ArName = "T2", EnName = "T2", NumOfPersons = 4 };
        _db.Context.AddRange(seller, other, _store, _flour, section, _table, _table2);
        _db.Context.ApplicationSettings.Add(new ApplicationSetting { Password = "x", OrderTax = 10, OrderService = 5 });
        await _db.Context.SaveChangesAsync();

        _dish = new MenuItem
        {
            Code = "1", ArName = "Pizza", EnName = "Pizza", Price = 50, StoreId = _store.Id, MenuSectionId = section.Id,
            Components = new List<MenuItemComponent> { new() { ProductId = _flour.Id, Quantity = 2 } }
        };
        _db.Context.MenuItems.Add(_dish);
        await _db.Context.SaveChangesAsync();
        await _stock.AdjustAsync(_store.Id, _flour.Id, flourInStock);

        _user = seller.Id;
        _otherUser = other.Id;
        _db.Context.ChangeTracker.Clear();
    }

    private Order NewOrder(OrderType type, int quantity = 2, bool cash = false) => new()
    {
        Type = type,
        Cash = cash,
        TableId = type == OrderType.Hall ? _table.Id : null,
        Lines = new List<OrderMenuItem> { new() { MenuItemId = _dish.Id, BillDetail = new BillDetail { Quantity = quantity } } }
    };

    private async Task<decimal> Flour()
    {
        _db.Context.ChangeTracker.Clear();
        return (await _stock.FindAsync(_store.Id, _flour.Id)).Quantity ?? 0;
    }

    private async Task<Order> Reload(int id)
    {
        _db.Context.ChangeTracker.Clear();
        return await _db.Context.Orders.AsNoTracking().Include(a => a.Bill).Include(a => a.Lines).ThenInclude(a => a.BillDetail).SingleAsync(a => a.Id == id);
    }

    private async Task<int> Create(Order order)
    {
        var result = await _sut.CreateOrderAsync(order, _user);
        Assert.True(result.Success, result.Message);
        return int.Parse(result.Data);
    }

    [Fact]
    public async Task Create_HallPending_TakesIngredientsLocksTableAndComputesTotals()
    {
        await Seed();

        var id = await Create(NewOrder(OrderType.Hall));

        var order = await Reload(id);
        Assert.False(order.Cash);
        Assert.Equal("1", order.OrderCode);
        Assert.Equal(100, order.Bill.TotalAmountBeforeDiscountOrExtra);
        Assert.Equal(10, order.Tax);
        Assert.Equal(5, order.Service);
        Assert.Equal(115, order.Bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(115, order.Bill.CasherRemainingAmount);
        Assert.Equal(0, order.Bill.AmountPaided);
        Assert.Equal(8, order.Bill.BillCost); // 2 dishes * 2 flour * cost 2
        Assert.Equal(96, await Flour());
        Assert.True((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
        var movement = await _db.Context.StoreProductMovements.AsNoTracking().SingleAsync(a => a.Source == StockMovementSource.Order);
        Assert.Equal(-4, movement.QuantityDelta);
        Assert.Equal("1", movement.ReferenceCode);
    }

    [Fact]
    public async Task Create_SameDishTwice_SumsQuantity()
    {
        await Seed();
        var order = NewOrder(OrderType.Hall, 1);
        order.Lines.Add(new OrderMenuItem { MenuItemId = _dish.Id, BillDetail = new BillDetail { Quantity = 3 } });

        var id = await Create(order);

        var saved = await Reload(id);
        Assert.Equal(4, saved.Lines.Single().Quantity);
        Assert.Equal(92, await Flour());
    }

    [Fact]
    public async Task Create_TakeAway_IsPaidAtOnceWithNoService()
    {
        await Seed();

        var id = await Create(NewOrder(OrderType.TakeAway));

        var order = await Reload(id);
        Assert.True(order.Cash);
        Assert.Equal(_user, order.ReceiverId);
        Assert.Equal(PaymentMethod.Cash, order.PaymentMethod);
        Assert.Equal(0, order.Service);
        Assert.Equal(110, order.Bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(110, order.Bill.AmountPaided);
        Assert.Equal(0, order.Bill.CasherRemainingAmount);
    }

    [Fact]
    public async Task Create_HallOnBusyTable_ReturnsTableBusyAndKeepsStock()
    {
        await Seed();
        await Create(NewOrder(OrderType.Hall));

        var result = await _sut.CreateOrderAsync(NewOrder(OrderType.Hall), _user);

        Assert.Equal("TableBusy", result.Message);
        Assert.Equal(96, await Flour());
    }

    [Fact]
    public async Task Create_HallWithoutTable_ReturnsTableRequired()
    {
        await Seed();
        var order = NewOrder(OrderType.Hall);
        order.TableId = null;

        Assert.Equal("TableRequired", (await _sut.CreateOrderAsync(order, _user)).Message);
    }

    [Fact]
    public async Task Create_NoLines_ReturnsAtLeastOneItem()
    {
        await Seed();
        var order = NewOrder(OrderType.Hall);
        order.Lines.Clear();

        Assert.Equal("AtLeastOneItem", (await _sut.CreateOrderAsync(order, _user)).Message);
    }

    [Fact]
    public async Task Create_UnknownDish_ReturnsRecordNotFound()
    {
        await Seed();
        var order = NewOrder(OrderType.Hall);
        order.Lines.First().MenuItemId = 999;

        Assert.Equal("RecordNotFound", (await _sut.CreateOrderAsync(order, _user)).Message);
    }

    [Fact]
    public async Task Create_UnavailableDish_ReturnsMenuItemNotAvailable()
    {
        await Seed();
        var dish = await _db.Context.MenuItems.SingleAsync();
        dish.IsAvailable = false;
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        Assert.Equal("MenuItemNotAvailable", (await _sut.CreateOrderAsync(NewOrder(OrderType.Hall), _user)).Message);
    }

    [Fact]
    public async Task Create_NotEnoughIngredients_ReturnsInsufficientStockAndTakesNothing()
    {
        await Seed(flourInStock: 3);

        var result = await _sut.CreateOrderAsync(NewOrder(OrderType.Hall, 2), _user);

        Assert.Equal("InsufficientStock", result.Message);
        Assert.Equal(3, await Flour());
        Assert.Empty(_db.Context.Orders);
        Assert.False((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
    }

    [Fact]
    public async Task Create_ExactlyEnoughIngredients_IsAllowed()
    {
        await Seed(flourInStock: 4);

        await Create(NewOrder(OrderType.Hall, 2));

        Assert.Equal(0, await Flour());
    }

    [Fact]
    public async Task Create_ZeroOrNegativeQuantity_MustNotAddStockOrMoney()
    {
        await Seed();

        var result = await _sut.CreateOrderAsync(NewOrder(OrderType.Hall, -5), _user);

        Assert.False(result.Success);
        Assert.Equal(100, await Flour());
    }

    [Fact]
    public async Task Create_Delivery_NeedsDriverAndAddsZoneFee()
    {
        await Seed();
        var driver = new Employee { Code = "1", ArName = "Driver", EnName = "Driver" };
        var zone = new DeliveryZone { Code = "1", ArName = "Zone", EnName = "Zone", Fee = 12 };
        _db.Context.AddRange(driver, zone);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var noDriver = NewOrder(OrderType.Delivery);
        noDriver.DeliveryZoneId = zone.Id;
        Assert.Equal("DeliveryDriverRequired", (await _sut.CreateOrderAsync(noDriver, _user)).Message);

        var noZone = NewOrder(OrderType.Delivery);
        noZone.EmployeeId = driver.Id;
        Assert.Equal("DeliveryZoneRequired", (await _sut.CreateOrderAsync(noZone, _user)).Message);

        var good = NewOrder(OrderType.Delivery);
        good.EmployeeId = driver.Id;
        good.DeliveryZoneId = zone.Id;
        var order = await Reload(await Create(good));
        Assert.Equal(12, order.DeliveryFee);
        Assert.Equal(100 + 10 + 5 + 12, order.Bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.True(order.Cash);
    }

    [Fact]
    public async Task Create_OnAccount_NeedsCustomerAndRaisesWhatHeOwes()
    {
        await Seed();
        var account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit };
        var customer = new Customer { Account = account };
        _db.Context.Customers.Add(customer);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var noCustomer = NewOrder(OrderType.TakeAway);
        noCustomer.OnAccount = true;
        Assert.Equal("CustomerRequired", (await _sut.CreateOrderAsync(noCustomer, _user)).Message);

        var good = NewOrder(OrderType.TakeAway);
        good.OnAccount = true;
        good.CustomerId = customer.Id;
        var order = await Reload(await Create(good));

        Assert.Equal(PayType.Later, order.Bill.PayType);
        Assert.Equal(0, order.Bill.AmountPaided);
        Assert.Equal(110, order.Bill.CasherRemainingAmount);
        Assert.Null(order.PaymentMethod);
        Assert.Equal(110, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id)).BalanceValue);
    }

    [Fact]
    public async Task Create_DiscountIsCappedAtTheTotal()
    {
        await Seed();
        var order = NewOrder(OrderType.TakeAway);
        order.Bill = new BillMaster { BillDiscountValue = 9999 };

        var saved = await Reload(await Create(order));

        Assert.Equal(0, saved.Bill.TotalBillAmountAfterDiscountAndExtra);
    }

    [Fact]
    public async Task Create_NegativeDiscount_IsIgnored()
    {
        await Seed();
        var order = NewOrder(OrderType.TakeAway);
        order.Bill = new BillMaster { BillDiscountValue = -50 };

        var saved = await Reload(await Create(order));

        Assert.Equal(110, saved.Bill.TotalBillAmountAfterDiscountAndExtra);
    }

    [Fact]
    public async Task Create_CodesIncrease()
    {
        await Seed();
        await Create(NewOrder(OrderType.TakeAway));
        var id = await Create(NewOrder(OrderType.TakeAway));

        Assert.Equal("2", (await Reload(id)).OrderCode);
        Assert.Equal("3", await _sut.GetNextCodeAsync());
    }

    [Fact]
    public async Task Pay_PendingHallOrder_CollectsFreesTableAndCompletesSeatedReservation()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.Hall));
        _db.Context.TableReservations.Add(new TableReservation
        {
            TableId = _table.Id, CustomerName = "Ali", PhoneNumber = "01012345678", GuestCount = 2,
            ReservationDate = DateTime.Now, Status = ReservationStatus.Seated
        });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.PayOrderAsync(id, _otherUser, PaymentMethod.Visa);

        Assert.True(result.Success, result.Message);
        var order = await Reload(id);
        Assert.True(order.Cash);
        Assert.Equal(_otherUser, order.ReceiverId);
        Assert.Equal(PaymentMethod.Visa, order.PaymentMethod);
        Assert.Equal(115, order.Bill.AmountPaided);
        Assert.Equal(0, order.Bill.CasherRemainingAmount);
        Assert.False((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
        Assert.Equal(ReservationStatus.Completed, (await _db.Context.TableReservations.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Pay_AlreadyPaid_DoesNothingTwice()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.Hall));
        await _sut.PayOrderAsync(id, _user, PaymentMethod.Cash);

        var again = await _sut.PayOrderAsync(id, _otherUser, PaymentMethod.Visa);

        Assert.True(again.Success);
        var order = await Reload(id);
        Assert.Equal(_user, order.ReceiverId);
        Assert.Equal(PaymentMethod.Cash, order.PaymentMethod);
    }

    [Fact]
    public async Task Pay_Unknown_ReturnsRecordNotFound()
    {
        Assert.Equal("RecordNotFound", (await _sut.PayOrderAsync(404, "u", PaymentMethod.Cash)).Message);
    }

    [Fact]
    public async Task Delete_PendingOrder_GivesIngredientsBackAndFreesTable()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.Hall));

        var result = await _sut.DeleteOrderAsync(id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(100, await Flour());
        Assert.False((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
        Assert.Empty(_db.Context.Orders);
        Assert.Empty(_db.Context.BillMasters);
        Assert.Empty(_db.Context.BillDetails);
        Assert.Equal(1, await _db.Context.StoreProductMovements.CountAsync(a => a.Source == StockMovementSource.OrderCancellation));
    }

    [Fact]
    public async Task Delete_OnAccountOrder_TakesTheAmountOffTheCustomer()
    {
        await Seed();
        var account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit };
        var customer = new Customer { Account = account };
        _db.Context.Customers.Add(customer);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        var order = NewOrder(OrderType.TakeAway);
        order.OnAccount = true;
        order.CustomerId = customer.Id;
        var id = await Create(order);

        await _sut.DeleteOrderAsync(id);

        Assert.Equal(0, (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id)).BalanceValue);
    }

    [Fact]
    public async Task Delete_AfterShiftClosed_IsBlocked()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.TakeAway));
        await _sut.FinishShiftAsync(_user, 110);

        var result = await _sut.DeleteOrderAsync(id);

        Assert.Equal("ShiftClosed", result.Message);
        Assert.Single(_db.Context.Orders);
    }

    [Fact]
    public async Task Delete_Unknown_IsSuccess()
    {
        Assert.True((await _sut.DeleteOrderAsync(404)).Success);
    }

    [Fact]
    public async Task Merge_MovesAllLinesAndTotalsAndFreesTheSourceTable()
    {
        await Seed();
        var first = await Create(NewOrder(OrderType.Hall, 1));
        var secondOrder = NewOrder(OrderType.Hall, 2);
        secondOrder.TableId = _table2.Id;
        var second = await Create(secondOrder);

        var result = await _sut.MergeOrderAsync(second, _table.Id);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        var merged = await Reload(first);
        Assert.Equal(2, merged.Lines.Count);
        Assert.Equal(150, merged.Bill.TotalAmountBeforeDiscountOrExtra);
        Assert.Equal(15, merged.Tax);
        Assert.Equal(10, merged.Service);
        Assert.Equal(175, merged.Bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Single(_db.Context.Orders);
        Assert.False((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table2.Id)).IsPending);
        Assert.True((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
    }

    [Fact]
    public async Task Merge_TargetWithoutOrder_ReturnsTargetTableHasNoOrder()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.Hall));

        Assert.Equal("TargetTableHasNoOrder", (await _sut.MergeOrderAsync(id, _table2.Id)).Message);
        Assert.Equal("TargetTableHasNoOrder", (await _sut.MergeOrderAsync(id, _table.Id)).Message);
    }

    [Fact]
    public async Task MoveItem_MovesOneLineAndRecalculatesBothOrders()
    {
        await Seed();
        var secondDish = new MenuItem { Code = "2", ArName = "Salad", EnName = "Salad", Price = 20, StoreId = _store.Id, MenuSectionId = _dish.MenuSectionId };
        _db.Context.MenuItems.Add(secondDish);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var first = NewOrder(OrderType.Hall, 1);
        first.Lines.Add(new OrderMenuItem { MenuItemId = secondDish.Id, BillDetail = new BillDetail { Quantity = 1 } });
        var firstId = await Create(first);
        var secondOrder = NewOrder(OrderType.Hall, 1);
        secondOrder.TableId = _table2.Id;
        var secondId = await Create(secondOrder);
        var saladLine = (await Reload(firstId)).Lines.Single(a => a.MenuItemId == secondDish.Id);

        var result = await _sut.MoveItemAsync(saladLine.Id, _table2.Id);

        Assert.True(result.Success, result.Message);
        var source = await Reload(firstId);
        var target = await Reload(secondId);
        Assert.Single(source.Lines);
        Assert.Equal(2, target.Lines.Count);
        Assert.Equal(50 + 5 + 5, source.Bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(70 + 7 + 5, target.Bill.TotalBillAmountAfterDiscountAndExtra);
    }

    [Fact]
    public async Task MoveItem_LastLine_RemovesTheEmptySourceOrder()
    {
        await Seed();
        var firstId = await Create(NewOrder(OrderType.Hall, 1));
        var secondOrder = NewOrder(OrderType.Hall, 1);
        secondOrder.TableId = _table2.Id;
        var secondId = await Create(secondOrder);
        var line = (await Reload(firstId)).Lines.Single();

        var result = await _sut.MoveItemAsync(line.Id, _table2.Id);

        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Single(_db.Context.Orders);
        Assert.Equal(2, (await Reload(secondId)).Lines.Count);
        Assert.False((await _db.Context.Tables.AsNoTracking().SingleAsync(a => a.Id == _table.Id)).IsPending);
    }

    [Fact]
    public async Task MoveItem_UnknownLine_ReturnsRecordNotFound()
    {
        Assert.Equal("RecordNotFound", (await _sut.MoveItemAsync(404, 1)).Message);
    }

    [Fact]
    public async Task FinishShift_ClosesMyPaidOrdersAndStoresTheReport()
    {
        await Seed();
        await Create(NewOrder(OrderType.TakeAway));
        await Create(NewOrder(OrderType.TakeAway));
        await Create(NewOrder(OrderType.Hall));

        var result = await _sut.FinishShiftAsync(_user, 200);

        Assert.True(result.Success, result.Message);
        var report = await _db.Context.ShiftCloseReports.AsNoTracking().SingleAsync();
        Assert.Equal(220, report.ExpectedCash);
        Assert.Equal(200, report.CountedCash);
        Assert.Equal(-20, report.Variance);
        Assert.Equal(2, await _db.Context.Orders.CountAsync(a => a.FinishShift));
        Assert.False((await _db.Context.Orders.AsNoTracking().SingleAsync(a => a.Type == OrderType.Hall)).FinishShift);
    }

    [Fact]
    public async Task FinishShift_NegativeCash_IsRejected()
    {
        await Seed();

        Assert.Equal("CountedCashInvalid", (await _sut.FinishShiftAsync(_user, -1)).Message);
        Assert.Empty(_db.Context.ShiftCloseReports);
    }

    [Fact]
    public async Task FinishShift_ElectronicPaymentsAreNotCountedAsCashInDrawer()
    {
        await Seed();
        var order = NewOrder(OrderType.TakeAway);
        order.PaymentMethod = PaymentMethod.Visa;
        await Create(order);
        await Create(NewOrder(OrderType.TakeAway));

        await _sut.FinishShiftAsync(_user, 110);

        Assert.Equal(110, (await _db.Context.ShiftCloseReports.AsNoTracking().SingleAsync()).ExpectedCash);
    }

    [Fact]
    public async Task ShiftDashboard_ShowsCollectedPendingAndOthers()
    {
        await Seed();
        await Create(NewOrder(OrderType.TakeAway));
        await Create(NewOrder(OrderType.Hall));
        var otherPaid = await _sut.CreateOrderAsync(NewOrder(OrderType.TakeAway), _otherUser);
        Assert.True(otherPaid.Success);

        var mine = await _sut.GetShiftDashboardAsync(_user);

        Assert.Equal(110, mine.CollectedTotal);
        Assert.Equal(115, mine.PendingMineTotal);
        Assert.Equal(110, mine.MoneyInDrawer);
    }

    [Fact]
    public async Task Complete_MarksOrderAndUnknownFails()
    {
        await Seed();
        var id = await Create(NewOrder(OrderType.TakeAway));

        Assert.True((await _sut.CompleteOrderAsync(id)).Success);
        Assert.True((await Reload(id)).Complete);
        Assert.Equal("RecordNotFound", (await _sut.CompleteOrderAsync(404)).Message);
        Assert.Empty(await _sut.GetKitchenOrdersAsync());
    }

    [Fact]
    public async Task CashierMenu_ShowsPortionsFromStockAndHidesSoldOutDishes()
    {
        await Seed(flourInStock: 5);

        var menu = (await _sut.GetCashierMenuAsync()).ToList();
        Assert.Equal(2, menu.Single().AvailablePortions);
        Assert.True(menu.Single().IsAvailable);

        await _stock.AdjustAsync(_store.Id, _flour.Id, -4);
        _db.Context.ChangeTracker.Clear();
        var soldOut = (await _sut.GetCashierMenuAsync()).Single();
        Assert.Equal(0, soldOut.AvailablePortions);
        Assert.False(soldOut.IsAvailable);
    }

    [Fact]
    public async Task PagedOrders_FilterByStatus()
    {
        await Seed();
        await Create(NewOrder(OrderType.TakeAway));
        await Create(NewOrder(OrderType.Hall));

        var mine = await _sut.GetPagedOrdersAsync(new Shared.ViewModels.Commons.PageRequest(), _user);
        var pending = await _sut.GetPagedOrdersAsync(new Shared.ViewModels.Commons.PageRequest { Status = "pending" }, _user);

        Assert.Single(mine.Data);
        Assert.Single(pending.Data);
        Assert.Equal(OrderType.Hall, pending.Data[0].Type);
    }
}
