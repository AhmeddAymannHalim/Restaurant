using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

// Tests for the table floor and the upcoming reservations on the dashboard.
public class DashboardRepositoryTests
{
    private const string UserId = "user-1";

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Table> AddTableAsync(ApplicationDbContext context, string name, byte seats = 4, bool inUse = false)
    {
        var table = new Table { ArName = name + " ar", EnName = name, NumOfPersons = seats, IsPending = inUse };
        context.Tables.Add(table);
        await context.SaveChangesAsync();
        return table;
    }

    private static async Task<TableReservation> AddBookingAsync(ApplicationDbContext context, int tableId, DateTime when,
        ReservationStatus status = ReservationStatus.Confirmed, string name = "Ahmed")
    {
        var booking = new TableReservation
        {
            TableId = tableId,
            CustomerName = name,
            PhoneNumber = "01012345678",
            GuestCount = 3,
            ReservationDate = when,
            Status = status
        };
        context.TableReservations.Add(booking);
        await context.SaveChangesAsync();
        return booking;
    }

    private static async Task<Order> AddOpenHallOrderAsync(ApplicationDbContext context, int tableId)
    {
        var order = new Order
        {
            OrderCode = Guid.NewGuid().ToString("N"),
            Type = OrderType.Hall,
            TableId = tableId,
            Cash = false,
            Bill = new BillMaster { Code = Guid.NewGuid().ToString("N"), Date = DateTime.Now, TotalBillAmountAfterDiscountAndExtra = 80 }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    // ---------- table state ----------

    [Fact]
    public async Task GetDashboardAsync_TableWithNoOrderAndNoBooking_IsFree()
    {
        // Arrange
        using var context = CreateContext();
        await AddTableAsync(context, "T1");
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        var card = Assert.Single(model.Tables);
        Assert.Equal(DashboardTableState.Free, card.State);
        Assert.Null(card.ReservationId);
        Assert.Null(card.OrderId);
    }

    [Fact]
    public async Task GetDashboardAsync_BookingInTheNextHour_ShowsTableReservedWithGuestDetails()
    {
        // Arrange
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        var when = DateTime.Now.AddMinutes(45);
        var booking = await AddBookingAsync(context, table.Id, when, name: "Mona");
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        var card = Assert.Single(model.Tables);
        Assert.Equal(DashboardTableState.Reserved, card.State);
        Assert.Equal(booking.Id, card.ReservationId);
        Assert.Equal("Mona", card.CustomerName);
        Assert.Equal("01012345678", card.PhoneNumber);
        Assert.Equal(3, card.Guests);
        Assert.Equal(when, card.ReservedAt);
        Assert.Equal(ReservationStatus.Confirmed, card.BookingStatus);
    }

    [Fact]
    public async Task GetDashboardAsync_WaitingBooking_ShowsTableReservedAsPending()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(1), ReservationStatus.Pending);
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        var card = Assert.Single(model.Tables);
        Assert.Equal(DashboardTableState.Reserved, card.State);
        Assert.Equal(ReservationStatus.Pending, card.BookingStatus);
    }

    [Fact]
    public async Task GetDashboardAsync_BookingJustStarted_StillHoldsTheTable()
    {
        // Arrange: guests are a little late, the table stays reserved for 30 minutes
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(-20));
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        Assert.Equal(DashboardTableState.Reserved, Assert.Single(model.Tables).State);
    }

    [Fact]
    public async Task GetDashboardAsync_BookingStartedOverThirtyMinutesAgo_TableIsFreeAgain()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(-45));
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        Assert.Equal(DashboardTableState.Free, Assert.Single(model.Tables).State);
    }

    [Fact]
    public async Task GetDashboardAsync_BookingMoreThanThreeHoursAway_TableIsStillFree()
    {
        // Arrange: it is on the reservations list, but it does not block the table yet
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(5));
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        Assert.Equal(DashboardTableState.Free, Assert.Single(model.Tables).State);
        Assert.Single(model.Reservations);
    }

    [Fact]
    public async Task GetDashboardAsync_TwoBookingsForOneTable_ShowsTheEarlierOne()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(150), name: "Later");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(20), name: "Sooner");
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        Assert.Equal("Sooner", Assert.Single(model.Tables).CustomerName);
    }

    [Theory]
    [InlineData(ReservationStatus.Rejected)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Seated)]
    public async Task GetDashboardAsync_FinishedBooking_IsNotShownAnywhere(ReservationStatus status)
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(30), status);
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        Assert.Empty(model.Reservations);
        Assert.Equal(DashboardTableState.Free, Assert.Single(model.Tables).State);
    }

    [Fact]
    public async Task GetDashboardAsync_TableWithOpenHallOrder_IsOccupiedAndKnowsTheOrder()
    {
        // Arrange
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", inUse: true);
        var order = await AddOpenHallOrderAsync(context, table.Id);
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        var card = Assert.Single(model.Tables);
        Assert.Equal(DashboardTableState.Occupied, card.State);
        Assert.Equal(order.Id, card.OrderId);
    }

    [Fact]
    public async Task GetDashboardAsync_OccupiedTableWithAlsoABooking_ShowsOccupied()
    {
        // Arrange: the guests are already sitting, so the table is in use, not "reserved"
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", inUse: true);
        await AddOpenHallOrderAsync(context, table.Id);
        await AddBookingAsync(context, table.Id, DateTime.Now.AddMinutes(30));
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        var card = Assert.Single(model.Tables);
        Assert.Equal(DashboardTableState.Occupied, card.State);
        Assert.Null(card.ReservationId);
    }

    [Fact]
    public async Task GetDashboardAsync_TablesAreMixed_EachGetsItsOwnState()
    {
        // Arrange
        using var context = CreateContext();
        var free = await AddTableAsync(context, "T1");
        var reserved = await AddTableAsync(context, "T2");
        var occupied = await AddTableAsync(context, "T3", inUse: true);
        await AddOpenHallOrderAsync(context, occupied.Id);
        await AddBookingAsync(context, reserved.Id, DateTime.Now.AddMinutes(30));
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        Assert.Equal(DashboardTableState.Free, model.Tables.Single(a => a.Id == free.Id).State);
        Assert.Equal(DashboardTableState.Reserved, model.Tables.Single(a => a.Id == reserved.Id).State);
        Assert.Equal(DashboardTableState.Occupied, model.Tables.Single(a => a.Id == occupied.Id).State);
    }

    // ---------- reservations list and counters ----------

    [Fact]
    public async Task GetDashboardAsync_Reservations_AreListedInTimeOrder()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(4), name: "Third");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(1), name: "First");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(2), name: "Second");
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        Assert.Equal(new[] { "First", "Second", "Third" }, model.Reservations.Select(a => a.CustomerName).ToArray());
    }

    [Fact]
    public async Task GetDashboardAsync_OldBookingFromYesterday_IsNotListed()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddDays(-1));
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        Assert.Empty(model.Reservations);
    }

    [Fact]
    public async Task GetDashboardAsync_BookingInThreeDays_IsNotListedYet()
    {
        // Arrange: the list only looks ahead until the end of tomorrow
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddDays(3));
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        Assert.Empty(model.Reservations);
    }

    [Fact]
    public async Task GetDashboardAsync_WaitingReservations_CountsOnlyPendingOnes()
    {
        // Arrange
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1");
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(1), ReservationStatus.Pending);
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(5), ReservationStatus.Pending);
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(9), ReservationStatus.Confirmed);
        await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(12), ReservationStatus.Rejected);
        var repository = new DashboardRepository(context);

        // Act
        var model = await repository.GetDashboardAsync(UserId);

        // Assert
        Assert.Equal(2, model.WaitingReservations);
    }

    [Fact]
    public async Task GetDashboardAsync_ReservationRow_CarriesTheTableNameAndNotes()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "Window table");
        var booking = await AddBookingAsync(context, table.Id, DateTime.Now.AddHours(1));
        booking.Notes = "Birthday cake";
        await context.SaveChangesAsync();
        var repository = new DashboardRepository(context);

        var model = await repository.GetDashboardAsync(UserId);

        var row = Assert.Single(model.Reservations);
        Assert.Equal("Window table", row.TableEnName);
        Assert.Equal("Birthday cake", row.Notes);
        Assert.Equal(table.Id, row.TableId);
    }
}
