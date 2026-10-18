using Restaurant.DAL.Helpers;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ILocationRepository
{
    Task<ResolvedLocation> ResolveAsync(int? townId);
}
