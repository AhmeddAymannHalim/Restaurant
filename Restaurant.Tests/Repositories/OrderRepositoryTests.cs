using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using PaymentMethod = Shared.Enums.PaymentMethod;

namespace Restaurant.Tests.Repositories;

public class OrderRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Order BuildOrder(OrderType type, bool cash, bool onAccount, string sellerId, string receiverId,
        PaymentMethod? paymentMethod, decimal amount, bool finishShift = false)
    {
        return new Order
        {
            OrderCode = Guid.NewGuid().ToString("N"),
            Type = type,
            Cash = cash,
            OnAccount = onAccount,
            SellerId = sellerId,
            ReceiverId = receiverId,
            PaymentMethod = paymentMethod,
            FinishShift = finishShift,
            Bill = new BillMaster
            {
                Code = Guid.NewGuid().ToString("N"),
                Date = DateTime.Now,
                TotalBillAmountAfterDiscountAndExtra = amount
            }
        };
    }

    [Fact]
    public async Task GetShiftDashboardAsync_CashOrderSoldAndReceivedBySameUser_CountsAsCollected()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.Orders.Add(BuildOrder(OrderType.Hall, cash: true, onAccount: false, sellerId: userId, receiverId: userId,
            paymentMethod: PaymentMethod.Cash, amount: 100));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(1, result.CollectedCount);
        Assert.Equal(100, result.CollectedTotal);
        Assert.Equal(0, result.PendingMineCount);
        Assert.Equal(0, result.PendingOthersCount);
        Assert.Equal(0, result.CollectedFromOthers.Count);
        Assert.Equal(0, result.Electronic.Count);
        Assert.Equal(0, result.OnAccount.Count);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_UnpaidOrderSoldByCurrentUser_CountsAsPendingMine()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.Orders.Add(BuildOrder(OrderType.Hall, cash: false, onAccount: false, sellerId: userId, receiverId: null,
            paymentMethod: null, amount: 50));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(1, result.PendingMineCount);
        Assert.Equal(50, result.PendingMineTotal);
        Assert.Equal(0, result.CollectedCount);
        Assert.Equal(0, result.PendingOthersCount);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_UnpaidOrderSoldByOtherUser_CountsAsPendingOthers()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        const string otherId = "user-2";
        context.Orders.Add(BuildOrder(OrderType.Delivery, cash: false, onAccount: false, sellerId: otherId, receiverId: null,
            paymentMethod: null, amount: 75));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(0, result.PendingMineCount);
        Assert.Equal(1, result.PendingOthersCount);
        Assert.Equal(75, result.PendingOthersTotal);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_CashOrderSoldByOtherAndReceivedByCurrentUser_CountsAsCollectedFromOthers()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        const string otherId = "user-2";
        context.Orders.Add(BuildOrder(OrderType.TakeAway, cash: true, onAccount: false, sellerId: otherId, receiverId: userId,
            paymentMethod: PaymentMethod.Cash, amount: 60));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(0, result.CollectedCount);
        Assert.Equal(1, result.CollectedFromOthers.Count);
        Assert.Equal(60, result.CollectedFromOthers.Amount);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_CashOrderPaidElectronically_CountsAsElectronic()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.Orders.Add(BuildOrder(OrderType.Hall, cash: true, onAccount: false, sellerId: userId, receiverId: userId,
            paymentMethod: PaymentMethod.Visa, amount: 200));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(1, result.Electronic.Count);
        Assert.Equal(200, result.Electronic.Amount);
        // Electronic orders are still Cash+!OnAccount, so they are also counted in Collected
        Assert.Equal(1, result.CollectedCount);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_OnAccountOrderInvolvingCurrentUser_CountsAsOnAccount()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.Orders.Add(BuildOrder(OrderType.Hall, cash: true, onAccount: true, sellerId: userId, receiverId: userId,
            paymentMethod: null, amount: 300));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(1, result.OnAccount.Count);
        Assert.Equal(300, result.OnAccount.Amount);
        // OnAccount orders must not also count towards Collected (Collected bucket requires !OnAccount)
        Assert.Equal(0, result.CollectedCount);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_FinishedShiftOrders_AreExcludedFromAllBuckets()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.Orders.Add(BuildOrder(OrderType.Hall, cash: true, onAccount: false, sellerId: userId, receiverId: userId,
            paymentMethod: PaymentMethod.Cash, amount: 999, finishShift: true));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(0, result.CollectedCount);
        Assert.Equal(0, result.CollectedTotal);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_MixOfOrdersForDifferentUsers_OnlyCountsRequestedUserBuckets()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        const string otherId = "user-2";

        context.Orders.AddRange(
            BuildOrder(OrderType.Hall, cash: true, onAccount: false, sellerId: userId, receiverId: userId, paymentMethod: PaymentMethod.Cash, amount: 100),
            BuildOrder(OrderType.Delivery, cash: true, onAccount: false, sellerId: otherId, receiverId: otherId, paymentMethod: PaymentMethod.Cash, amount: 150),
            BuildOrder(OrderType.Hall, cash: false, onAccount: false, sellerId: userId, receiverId: null, paymentMethod: null, amount: 40),
            BuildOrder(OrderType.Hall, cash: false, onAccount: false, sellerId: otherId, receiverId: null, paymentMethod: null, amount: 55));
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.Equal(1, result.CollectedCount);
        Assert.Equal(100, result.CollectedTotal);
        Assert.Equal(1, result.PendingMineCount);
        Assert.Equal(40, result.PendingMineTotal);
        Assert.Equal(1, result.PendingOthersCount);
        Assert.Equal(55, result.PendingOthersTotal);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_NoLastShiftReport_ReturnsNullLastShiftReport()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync("user-1");

        // Assert
        Assert.Null(result.LastShiftReport);
    }

    [Fact]
    public async Task GetShiftDashboardAsync_HasShiftCloseReport_ReturnsMostRecentReport()
    {
        // Arrange
        using var context = CreateContext();
        const string userId = "user-1";
        context.ShiftCloseReports.AddRange(
            new ShiftCloseReport { UserId = userId, ExpectedCash = 100, CountedCash = 100, Variance = 0, HadIncompleteOrders = false },
            new ShiftCloseReport { UserId = userId, ExpectedCash = 200, CountedCash = 190, Variance = -10, HadIncompleteOrders = true });
        await context.SaveChangesAsync();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetShiftDashboardAsync(userId);

        // Assert
        Assert.NotNull(result.LastShiftReport);
        Assert.Equal(200, result.LastShiftReport.ExpectedCash);
        Assert.True(result.LastShiftReport.HadIncompleteOrders);
    }
}
