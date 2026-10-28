using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests;

public class TaxRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TaxRepository _sut;

    public TaxRepositoryTests()
    {
        _sut = new TaxRepository(_db.Context, _db.Http);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Upsert_New_SavesTheTax()
    {
        var result = await _sut.UpsertTaxAsync(new Tax { ArName = "قيمة مضافة", EnName = "VAT", Value = 14 });

        Assert.True(result.Success, result.Message);
        var saved = await _db.Context.Taxes.AsNoTracking().SingleAsync();
        Assert.Equal("VAT", saved.EnName);
        Assert.Equal(14, saved.Value);
        Assert.NotNull(saved.CreatedAt);
    }

    [Fact]
    public async Task Upsert_Existing_ChangesTheValueAndKeepsTheCreatedDate()
    {
        await _sut.UpsertTaxAsync(new Tax { ArName = "قيمة مضافة", EnName = "VAT", Value = 14 });
        var saved = await _db.Context.Taxes.AsNoTracking().SingleAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.UpsertTaxAsync(new Tax { Id = saved.Id, ArName = "قيمة مضافة", EnName = "VAT", Value = 5 });

        Assert.True(result.Success, result.Message);
        var updated = await _db.Context.Taxes.AsNoTracking().SingleAsync();
        Assert.Equal(5, updated.Value);
        Assert.Equal(saved.CreatedAt, updated.CreatedAt);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task Upsert_Unknown_ReturnsRecordNotFound()
    {
        var result = await _sut.UpsertTaxAsync(new Tax { Id = 404, ArName = "x", EnName = "x", Value = 1 });

        Assert.False(result.Success);
        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task GetPaged_Search_FindsByArabicOrEnglishName()
    {
        await _sut.UpsertTaxAsync(new Tax { ArName = "قيمة مضافة", EnName = "VAT", Value = 14 });
        await _sut.UpsertTaxAsync(new Tax { ArName = "معفى", EnName = "Exempt", Value = 0 });

        var byEnglish = await _sut.GetPagedTaxesAsync(new PageRequest { SearchString = "vat" });
        var byArabic = await _sut.GetPagedTaxesAsync(new PageRequest { SearchString = "معفى" });
        var all = await _sut.GetPagedTaxesAsync(new PageRequest());

        Assert.Equal("VAT", Assert.Single(byEnglish.Data).EnName);
        Assert.Equal("Exempt", Assert.Single(byArabic.Data).EnName);
        Assert.Equal(2, all.TotalRecords);
    }

    [Fact]
    public async Task Delete_RemovesTheTax()
    {
        await _sut.UpsertTaxAsync(new Tax { ArName = "معفى", EnName = "Exempt", Value = 0 });
        var id = (await _db.Context.Taxes.AsNoTracking().SingleAsync()).Id;
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.DeleteTaxAsync(id);

        Assert.True(result.Success, result.Message);
        Assert.False(await _db.Context.Taxes.AnyAsync());
        Assert.Null(await _sut.GetTaxByIdAsync(id));
    }

    [Fact]
    public async Task Delete_Unknown_IsSuccess()
    {
        Assert.True((await _sut.DeleteTaxAsync(404)).Success);
    }
}
