using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;

    public LocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResolvedLocation> ResolveAsync(int? townId)
    {
        var result = new ResolvedLocation();
        if (!townId.HasValue)
        {
            return result;
        }

        var town = await _context.Towns.AsNoTracking()
            .Where(a => a.Id == townId.Value)
            .Select(a => new { a.GovernorateId, a.Governorate.CountryId })
            .FirstOrDefaultAsync();

        if (town != null)
        {
            result.GovId = town.GovernorateId;
            result.CountryId = town.CountryId;
        }

        return result;
    }
}
