using Microsoft.EntityFrameworkCore;

namespace Restaurant.DAL.Helpers;

public static class LocationResolver
{
    public static async Task<(int? GovId, int? CountryId)> ResolveAsync(ApplicationDbContext context, int? townId)
    {
        if (!townId.HasValue)
        {
            return (null, null);
        }

        var location = await context.Towns.AsNoTracking()
            .Where(a => a.Id == townId.Value)
            .Select(a => new { a.GovernorateId, a.Governorate.CountryId })
            .FirstOrDefaultAsync();

        return location is null ? (null, null) : (location.GovernorateId, location.CountryId);
    }
}
