using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class TableReservationRepositoryTests
{
    // A reservation holds its table for 2 hours (see TableReservationRepository.SlotMinutes).
    private const int HoldMinutes = 120;

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Table> AddTableAsync(ApplicationDbContext context, string name, byte seats)
    {
        var table = new Table { ArName = name + " ar", EnName = name, NumOfPersons = seats };
        context.Tables.Add(table);
        await context.SaveChangesAsync();
        return table;
    }

    // A valid booking for tomorrow evening. Each test changes only the one thing it is about.
    private static TableReservation NewBooking(int tableId, DateTime? when = null)
    {
        return new TableReservation
        {
            TableId = tableId,
            CustomerName = "Ahmed",
            PhoneNumber = "01012345678",
            GuestCount = 2,
            ReservationDate = when ?? DateTime.Today.AddDays(1).AddHours(20)
        };
    }

    private static DateTime TomorrowEvening => DateTime.Today.AddDays(1).AddHours(20);

    // ---------- BookAsync: the happy path ----------

    [Fact]
    public async Task BookAsync_ValidBooking_SavesItAsPending()
    {
        // Arrange
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        // Act
        var result = await repository.BookAsync(NewBooking(table.Id), customerId: null);

        // Assert
        Assert.True(result.Success);
        var saved = Assert.Single(context.TableReservations);
        Assert.Equal(ReservationStatus.Pending, saved.Status);
        Assert.Equal("Ahmed", saved.CustomerName);
        Assert.NotEqual(default, saved.CreatedAt);
    }

    [Fact]
    public async Task BookAsync_CallerSendsIdAndStatus_TheyAreIgnored()
    {
        // Arrange: a guest must not be able to book straight into "Confirmed"
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.Id = 99;
        booking.Status = ReservationStatus.Confirmed;

        // Act
        await repository.BookAsync(booking, customerId: null);

        // Assert
        var saved = Assert.Single(context.TableReservations);
        Assert.NotEqual(99, saved.Id);
        Assert.Equal(ReservationStatus.Pending, saved.Status);
    }

    // ---------- BookAsync: input rules ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BookAsync_EmptyName_ReturnsNameRequired(string name)
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.CustomerName = name;

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.NameRequired", result.Message);
        Assert.Empty(context.TableReservations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12345")]               // too short
    [InlineData("1234567890123456")]    // too long
    public async Task BookAsync_BadPhone_ReturnsPhoneInvalid(string phone)
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.PhoneNumber = phone;

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.PhoneInvalid", result.Message);
    }

    [Theory]
    [InlineData("+20 101 234 5678")]
    [InlineData("(010) 1234-5678")]
    public async Task BookAsync_PhoneWithSpacesOrSigns_IsAccepted(string phone)
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.PhoneNumber = phone;

        var result = await repository.BookAsync(booking, null);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task BookAsync_EmailWithoutAtSign_ReturnsEmailInvalid()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.Email = "not-an-email";

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.EmailInvalid", result.Message);
    }

    [Fact]
    public async Task BookAsync_NoEmail_IsFine()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.Email = "   ";

        var result = await repository.BookAsync(booking, null);

        Assert.True(result.Success);
        Assert.Null(Assert.Single(context.TableReservations).Email);
    }

    // ---------- BookAsync: table and guests ----------

    [Fact]
    public async Task BookAsync_TableDoesNotExist_ReturnsTableRequired()
    {
        using var context = CreateContext();
        var repository = new TableReservationRepository(context);

        var result = await repository.BookAsync(NewBooking(tableId: 12345), null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TableRequired", result.Message);
    }

    [Fact]
    public async Task BookAsync_ZeroGuests_ReturnsGuestsRequired()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.GuestCount = 0;

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.GuestsRequired", result.Message);
    }

    [Fact]
    public async Task BookAsync_MoreGuestsThanSeats_ReturnsTooManyGuests()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.GuestCount = 5;

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TooManyGuests", result.Message);
    }

    [Fact]
    public async Task BookAsync_GuestsExactlyFillTheTable_IsAccepted()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.GuestCount = 4;

        var result = await repository.BookAsync(booking, null);

        Assert.True(result.Success);
    }

    // ---------- BookAsync: how far ahead ----------

    [Fact]
    public async Task BookAsync_LessThanThirtyMinutesAway_ReturnsTooSoon()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        var result = await repository.BookAsync(NewBooking(table.Id, DateTime.Now.AddMinutes(10)), null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TooSoon", result.Message);
    }

    [Fact]
    public async Task BookAsync_InThePast_ReturnsTooSoon()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        var result = await repository.BookAsync(NewBooking(table.Id, DateTime.Now.AddDays(-1)), null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TooSoon", result.Message);
    }

    [Fact]
    public async Task BookAsync_MoreThanSixtyDaysAway_ReturnsTooFar()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        var result = await repository.BookAsync(NewBooking(table.Id, DateTime.Now.AddDays(61)), null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TooFar", result.Message);
    }

    // ---------- BookAsync: double booking ----------

    [Theory]
    [InlineData(0)]                    // exactly the same time
    [InlineData(-(HoldMinutes - 1))]   // starts just before the first booking ends
    [InlineData(HoldMinutes - 1)]      // starts just before the first booking ends
    public async Task BookAsync_SameTableOverlappingTime_ReturnsTableTaken(int minutesApart)
    {
        // Arrange
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id, TomorrowEvening), null);

        // Act
        var result = await repository.BookAsync(NewBooking(table.Id, TomorrowEvening.AddMinutes(minutesApart)), null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TableTaken", result.Message);
        Assert.Single(context.TableReservations);
    }

    [Theory]
    [InlineData(-HoldMinutes)]
    [InlineData(HoldMinutes)]
    [InlineData(HoldMinutes * 2)]
    public async Task BookAsync_SameTableAfterTheHoldEnds_IsAccepted(int minutesApart)
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id, TomorrowEvening), null);

        var result = await repository.BookAsync(NewBooking(table.Id, TomorrowEvening.AddMinutes(minutesApart)), null);

        Assert.True(result.Success);
        Assert.Equal(2, context.TableReservations.Count());
    }

    [Fact]
    public async Task BookAsync_DifferentTableSameTime_IsAccepted()
    {
        using var context = CreateContext();
        var first = await AddTableAsync(context, "T1", 4);
        var second = await AddTableAsync(context, "T2", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(first.Id), null);

        var result = await repository.BookAsync(NewBooking(second.Id), null);

        Assert.True(result.Success);
    }

    [Theory]
    [InlineData(ReservationStatus.Rejected)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Seated)]
    public async Task BookAsync_EarlierBookingIsNoLongerActive_TableCanBeBookedAgain(ReservationStatus status)
    {
        // Arrange: only waiting and confirmed bookings hold a table
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var old = NewBooking(table.Id, TomorrowEvening);
        old.CustomerName = "Old";
        old.Status = status;
        context.TableReservations.Add(old);
        await context.SaveChangesAsync();
        var repository = new TableReservationRepository(context);

        // Act
        var result = await repository.BookAsync(NewBooking(table.Id, TomorrowEvening), null);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task BookAsync_SignedInCustomer_IsLinkedToTheBooking()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        await repository.BookAsync(NewBooking(table.Id), customerId: "user-1");

        Assert.Equal("user-1", Assert.Single(context.TableReservations).CustomerId);
    }

    // ---------- SetStatusAsync ----------

    [Fact]
    public async Task SetStatusAsync_Confirm_ChangesStatus()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id), null);
        var id = context.TableReservations.Single().Id;

        var result = await repository.SetStatusAsync(id, ReservationStatus.Confirmed, null);

        Assert.True(result.Success);
        Assert.Equal(ReservationStatus.Confirmed, context.TableReservations.Single().Status);
    }

    [Fact]
    public async Task SetStatusAsync_Reject_SavesTheTrimmedReason()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id), null);
        var id = context.TableReservations.Single().Id;

        await repository.SetStatusAsync(id, ReservationStatus.Rejected, "  Closed for a private party  ");

        var saved = context.TableReservations.Single();
        Assert.Equal(ReservationStatus.Rejected, saved.Status);
        Assert.Equal("Closed for a private party", saved.RejectionReason);
    }

    [Fact]
    public async Task SetStatusAsync_ReasonOnlyKeptForRejected()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id), null);
        var id = context.TableReservations.Single().Id;

        await repository.SetStatusAsync(id, ReservationStatus.Confirmed, "should be dropped");

        Assert.Null(context.TableReservations.Single().RejectionReason);
    }

    [Fact]
    public async Task SetStatusAsync_UnknownId_ReturnsRecordNotFound()
    {
        using var context = CreateContext();
        var repository = new TableReservationRepository(context);

        var result = await repository.SetStatusAsync(999, ReservationStatus.Confirmed, null);

        Assert.False(result.Success);
        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task SetStatusAsync_ConfirmWhenAnotherBookingHoldsTheTable_ReturnsTableTaken()
    {
        // Arrange: two waiting bookings for the same table and time (possible if one was added by staff)
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var first = NewBooking(table.Id, TomorrowEvening);
        var second = NewBooking(table.Id, TomorrowEvening.AddMinutes(30));
        context.TableReservations.AddRange(first, second);
        await context.SaveChangesAsync();
        var repository = new TableReservationRepository(context);

        // Act
        var result = await repository.SetStatusAsync(second.Id, ReservationStatus.Confirmed, null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TableTaken", result.Message);
        Assert.Equal(ReservationStatus.Pending, context.TableReservations.Single(a => a.Id == second.Id).Status);
    }

    // ---------- GetTableSlotsAsync ----------

    [Fact]
    public async Task GetTableSlotsAsync_OnlyTheBookedTableIsReserved()
    {
        // Arrange
        using var context = CreateContext();
        var booked = await AddTableAsync(context, "T1", 4);
        var free = await AddTableAsync(context, "T2", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(booked.Id, TomorrowEvening), null);

        // Act
        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 2);

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.True(slots.Single(a => a.Table.Id == booked.Id).IsReserved);
        Assert.False(slots.Single(a => a.Table.Id == free.Id).IsReserved);
    }

    [Fact]
    public async Task GetTableSlotsAsync_RejectedBookingDoesNotMarkTheTable()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var rejected = NewBooking(table.Id, TomorrowEvening);
        rejected.Status = ReservationStatus.Rejected;
        context.TableReservations.Add(rejected);
        await context.SaveChangesAsync();
        var repository = new TableReservationRepository(context);

        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 2);

        Assert.False(Assert.Single(slots).IsReserved);
    }

    [Fact]
    public async Task GetTableSlotsAsync_SmallestTablesComeFirst()
    {
        using var context = CreateContext();
        await AddTableAsync(context, "Big", 8);
        await AddTableAsync(context, "Small", 2);
        await AddTableAsync(context, "Middle", 4);
        var repository = new TableReservationRepository(context);

        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 2);

        Assert.Equal(new byte[] { 2, 4, 8 }, slots.Select(a => a.Table.NumOfPersons).ToArray());
    }

    // ---------- things that moved into the repository ----------

    [Fact]
    public async Task GetTableSlotsAsync_TableSmallerThanTheGroup_IsMarkedTooSmall()
    {
        using var context = CreateContext();
        await AddTableAsync(context, "Two", 2);
        await AddTableAsync(context, "Six", 6);
        var repository = new TableReservationRepository(context);

        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 4);

        Assert.True(slots.Single(a => a.Table.EnName == "Two").TooSmall);
        Assert.False(slots.Single(a => a.Table.EnName == "Six").TooSmall);
    }

    [Fact]
    public async Task GetTableSlotsAsync_GroupFillsTheTableExactly_IsNotTooSmall()
    {
        using var context = CreateContext();
        await AddTableAsync(context, "Four", 4);
        var repository = new TableReservationRepository(context);

        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 4);

        Assert.False(Assert.Single(slots).TooSmall);
    }

    [Fact]
    public async Task BookAsync_NoDateChosen_ReturnsDateRequired()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        var booking = NewBooking(table.Id);
        booking.ReservationDate = default;

        var result = await repository.BookAsync(booking, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.DateRequired", result.Message);
    }

    [Fact]
    public async Task BookAsync_Success_TellsTheCallerTheNewId()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);

        var result = await repository.BookAsync(NewBooking(table.Id), null);

        Assert.Equal(context.TableReservations.Single().Id.ToString(), result.Data);
    }

    [Fact]
    public async Task ArriveAsync_MarksTheBookingSeatedAndGivesTheTable()
    {
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id), null);
        var id = context.TableReservations.Single().Id;

        var result = await repository.ArriveAsync(id);

        Assert.True(result.Success);
        Assert.Equal(table.Id.ToString(), result.Data);
        Assert.Equal(ReservationStatus.Seated, context.TableReservations.Single().Status);
    }

    [Fact]
    public async Task ArriveAsync_UnknownBooking_ReturnsRecordNotFound()
    {
        using var context = CreateContext();
        var repository = new TableReservationRepository(context);

        var result = await repository.ArriveAsync(999);

        Assert.False(result.Success);
        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task ArriveAsync_SeatedBookingNoLongerHoldsTheTableForOthers()
    {
        // Arrange: once the guests are seated the order holds the table, not the booking
        using var context = CreateContext();
        var table = await AddTableAsync(context, "T1", 4);
        var repository = new TableReservationRepository(context);
        await repository.BookAsync(NewBooking(table.Id, TomorrowEvening), null);
        await repository.ArriveAsync(context.TableReservations.Single().Id);

        // Act
        var slots = await repository.GetTableSlotsAsync(TomorrowEvening, 2);

        // Assert
        Assert.False(Assert.Single(slots).IsReserved);
    }
}
