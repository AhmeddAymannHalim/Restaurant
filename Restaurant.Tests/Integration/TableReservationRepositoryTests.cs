using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

public class TableReservationRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TableReservationRepository _sut;
    private readonly DateTime _start = new(2030, 1, 1, 18, 0, 0);

    public TableReservationRepositoryTests() => _sut = new TableReservationRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<Restaurant.DAL.Models.Table> SeedTable(byte seats = 4)
    {
        var table = new Restaurant.DAL.Models.Table { Code = "1", ArName = "Table", EnName = "T", NumOfPersons = seats };
        _db.Context.Tables.Add(table);
        await _db.Context.SaveChangesAsync();
        return table;
    }

    private TableReservation Reservation(int tableId, DateTime? at = null, ReservationStatus status = ReservationStatus.Confirmed) => new()
    {
        TableId = tableId,
        GuestName = " Ali ",
        PhoneNumber = "01012345678",
        GuestCount = 2,
        ReservationDate = at ?? _start,
        DurationMinutes = 60,
        Status = status
    };

    [Fact]
    public async Task Upsert_Valid_SavesAndTrimsName()
    {
        var table = await SeedTable();

        var result = await _sut.UpsertReservationAsync(Reservation(table.Id));

        Assert.True(result.Success, result.Message);
        var saved = await _sut.GetReservationByIdAsync(1);
        Assert.Equal("Ali", saved.GuestName);
        Assert.Equal(table.Id, saved.Table.Id);
    }

    [Theory]
    [InlineData("name", "Reservation.Error.NameRequired")]
    [InlineData("phone", "Reservation.Error.PhoneInvalid")]
    [InlineData("email", "Reservation.Error.EmailInvalid")]
    [InlineData("date", "Reservation.Error.DateRequired")]
    [InlineData("guests0", "Reservation.Error.GuestsRequired")]
    [InlineData("guests9", "Reservation.Error.TooManyGuests")]
    [InlineData("table", "Reservation.Error.TableRequired")]
    public async Task Upsert_Invalid_ReturnsLocalizedKeyAndSavesNothing(string field, string expected)
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id);
        switch (field)
        {
            case "name": entity.GuestName = "  "; break;
            case "phone": entity.PhoneNumber = "12"; break;
            case "email": entity.Email = "no-at-sign"; break;
            case "date": entity.ReservationDate = default; break;
            case "guests0": entity.GuestCount = 0; break;
            case "guests9": entity.GuestCount = 9; break;
            case "table": entity.TableId = 999; break;
        }

        var result = await _sut.UpsertReservationAsync(entity);

        Assert.False(result.Success);
        Assert.Equal(expected, result.Message);
        Assert.Empty(_db.Context.TableReservations);
    }

    [Fact]
    public async Task Upsert_OverlapsConfirmedReservation_ReturnsTableTaken()
    {
        var table = await SeedTable();
        Assert.True((await _sut.UpsertReservationAsync(Reservation(table.Id))).Success);

        var result = await _sut.UpsertReservationAsync(Reservation(table.Id, _start.AddMinutes(30)));

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TableTaken", result.Message);
    }

    [Fact]
    public async Task Upsert_StartsWhenPreviousEnds_IsAllowed()
    {
        var table = await SeedTable();
        Assert.True((await _sut.UpsertReservationAsync(Reservation(table.Id))).Success);

        var result = await _sut.UpsertReservationAsync(Reservation(table.Id, _start.AddMinutes(60)));

        Assert.True(result.Success, result.Message);
    }

    [Fact]
    public async Task Upsert_EditingSameReservation_DoesNotConflictWithItself()
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id);
        Assert.True((await _sut.UpsertReservationAsync(entity)).Success);
        _db.Context.ChangeTracker.Clear();

        var edit = Reservation(table.Id);
        edit.Id = entity.Id;
        edit.GuestCount = 3;
        var result = await _sut.UpsertReservationAsync(edit);

        Assert.True(result.Success, result.Message);
    }

    [Fact]
    public async Task Upsert_UnknownId_ReturnsRecordNotFound()
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id);
        entity.Id = 77;

        var result = await _sut.UpsertReservationAsync(entity);

        Assert.False(result.Success);
        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task SetStatus_Rejected_StoresReasonAndClearsItOnOtherStatus()
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id, status: ReservationStatus.Pending);
        await _sut.UpsertReservationAsync(entity);

        await _sut.SetStatusAsync(entity.Id, ReservationStatus.Rejected, "  full ");
        Assert.Equal("full", (await _sut.GetReservationByIdAsync(entity.Id)).RejectionReason);

        await _sut.SetStatusAsync(entity.Id, ReservationStatus.Cancelled, "ignored");
        Assert.Null((await _sut.GetReservationByIdAsync(entity.Id)).RejectionReason);
    }

    [Fact]
    public async Task SetStatus_ConfirmOverlapping_ReturnsTableTaken()
    {
        var table = await SeedTable();
        await _sut.UpsertReservationAsync(Reservation(table.Id));
        var pending = Reservation(table.Id, _start.AddMinutes(10), ReservationStatus.Pending);
        _db.Context.TableReservations.Add(pending);
        await _db.Context.SaveChangesAsync();

        var result = await _sut.SetStatusAsync(pending.Id, ReservationStatus.Confirmed, null);

        Assert.False(result.Success);
        Assert.Equal("Reservation.Error.TableTaken", result.Message);
    }

    [Fact]
    public async Task SetStatus_UnknownId_ReturnsRecordNotFound()
    {
        var result = await _sut.SetStatusAsync(5, ReservationStatus.Confirmed, null);

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task Arrive_Confirmed_SeatsAndReturnsTableId()
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id);
        await _sut.UpsertReservationAsync(entity);

        var result = await _sut.ArriveAsync(entity.Id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(table.Id.ToString(), result.Data);
        Assert.Equal(ReservationStatus.Seated, (await _sut.GetReservationByIdAsync(entity.Id)).Status);
    }

    [Fact]
    public async Task Arrive_UnknownId_ReturnsRecordNotFound()
    {
        Assert.Equal("RecordNotFound", (await _sut.ArriveAsync(5)).Message);
    }

    [Fact]
    public async Task GetPaged_PendingFirst_AndFiltersByStatusAndSearch()
    {
        var table = await SeedTable();
        await _sut.UpsertReservationAsync(Reservation(table.Id, _start, ReservationStatus.Confirmed));
        var pending = Reservation(table.Id, _start.AddDays(1), ReservationStatus.Pending);
        pending.GuestName = "Omar";
        await _sut.UpsertReservationAsync(pending);

        var all = await _sut.GetPagedReservationsAsync(new PageRequest(), null);
        var onlyConfirmed = await _sut.GetPagedReservationsAsync(new PageRequest(), ReservationStatus.Confirmed);
        var search = await _sut.GetPagedReservationsAsync(new PageRequest { SearchString = "OMAR" }, null);

        Assert.Equal("Omar", all.Data[0].GuestName);
        Assert.Single(onlyConfirmed.Data);
        Assert.Single(search.Data);
    }

    [Fact]
    public async Task Delete_RemovesAndUnknownIsSuccess()
    {
        var table = await SeedTable();
        var entity = Reservation(table.Id);
        await _sut.UpsertReservationAsync(entity);

        Assert.True((await _sut.DeleteReservationAsync(entity.Id)).Success);
        Assert.Null(await _sut.GetReservationByIdAsync(entity.Id));
        Assert.True((await _sut.DeleteReservationAsync(entity.Id)).Success);
    }
}
