using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace Shared.Helpers;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IMemoryCache _cacheService;

    public JsonStringLocalizerFactory(IMemoryCache cacheService)
    {
        _cacheService = cacheService;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer(_cacheService);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer(_cacheService);
    }
}
