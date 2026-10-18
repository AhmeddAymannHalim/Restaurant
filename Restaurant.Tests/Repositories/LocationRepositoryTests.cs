using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class LocationRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ResolveAsync_KnownTown_GivesItsGovernorateAndCountry()
    {
        // Arrange
        using var context = CreateContext();
        context.Countries.Add(new Country { Id = 7, ArName = "c", EnName = "c" });
        context.Governorates.Add(new Governorate { Id = 5, CountryId = 7, ArName = "g", EnName = "g" });
        context.Towns.Add(new Town { Id = 3, GovernorateId = 5, ArName = "t", EnName = "t" });
        await context.SaveChangesAsync();
        var repository = new LocationRepository(context);

        // Act
        var location = await repository.ResolveAsync(3);

        // Assert
        Assert.Equal(5, location.GovId);
        Assert.Equal(7, location.CountryId);
    }

    [Fact]
    public async Task ResolveAsync_NoTown_GivesAnEmptyResult()
    {
        using var context = CreateContext();
        var repository = new LocationRepository(context);

        var location = await repository.ResolveAsync(null);

        Assert.Null(location.GovId);
        Assert.Null(location.CountryId);
    }

    [Fact]
    public async Task ResolveAsync_UnknownTown_GivesAnEmptyResult()
    {
        using var context = CreateContext();
        var repository = new LocationRepository(context);

        var location = await repository.ResolveAsync(999);

        Assert.Null(location.GovId);
        Assert.Null(location.CountryId);
    }
}
