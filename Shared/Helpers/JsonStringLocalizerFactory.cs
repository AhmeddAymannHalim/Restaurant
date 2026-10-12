using Microsoft.Extensions.Localization;

namespace Shared.Helpers;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IMemoryCacheService _cacheService;

    public JsonStringLocalizerFactory(IMemoryCacheService cacheService)
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
