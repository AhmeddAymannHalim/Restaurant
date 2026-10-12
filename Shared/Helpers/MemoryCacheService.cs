using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace Shared.Helpers;

public interface IMemoryCacheService
{
    Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> acquire, MemoryCacheEntryOptions cacheOptions = null);
    T GetOrSet<T>(string cacheKey, Func<T> acquire, MemoryCacheEntryOptions cacheOptions = null);
    void Remove(string cacheKey);
    void Set<T>(string cacheKey, T item, MemoryCacheEntryOptions cacheOptions = null);
}

public class MemoryCacheService : IMemoryCacheService
{
    #region Fields
    private readonly IMemoryCache _memoryCache;
    private readonly InMemoryCacheOptions _memoryCacheOptions;
    private readonly ILogger<MemoryCacheService> _logger;
    #endregion

    #region Ctor
    public MemoryCacheService(
        IMemoryCache memoryCache,
        IOptions<InMemoryCacheOptions> memoryCacheOptions,
        ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _memoryCacheOptions = memoryCacheOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Utilities
    private MemoryCacheEntryOptions PrepareCacheEntryOptions(MemoryCacheEntryOptions cacheOptions)
    {
        if (cacheOptions is not null)
            return cacheOptions;

        cacheOptions = new()
        {
            SlidingExpiration = TimeSpan.Parse(_memoryCacheOptions.SlidingExpiration),
        };
        if (_memoryCacheOptions.AbsoluteExpirationRelativeToNow != "00:00:00")
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.Parse(_memoryCacheOptions.AbsoluteExpirationRelativeToNow);
        }
        return cacheOptions;
    }
    #endregion

    #region Methods
    public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> acquire, MemoryCacheEntryOptions cacheOptions = null)
    {
        if (_memoryCache.TryGetValue(cacheKey, out T cachedItem))
        {
            return await Task.FromResult(cachedItem!);
        }
        else
        {
            T item = await acquire();
            if (item is not null)
            {
                cacheOptions = PrepareCacheEntryOptions(cacheOptions);
                _memoryCache.Set(cacheKey,
                    item,
                    cacheOptions);
            }
            return item;
        }
    }

    public T GetOrSet<T>(string cacheKey, Func<T> acquire, MemoryCacheEntryOptions cacheOptions = null)
    {
        if (_memoryCache.TryGetValue(cacheKey, out T cachedItem))
        {
            return cachedItem!;
        }
        else
        {
            T item = acquire();
            if (item is not null)
            {
                cacheOptions = PrepareCacheEntryOptions(cacheOptions);
                _memoryCache.Set(cacheKey,
                    item,
                    cacheOptions);
            }
            return item;
        }
    }

    public void Remove(string cacheKey)
    {
        try
        {
            _memoryCache.Remove(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "::: Exception thrown while removing entity from the memory cache. {@cacheKey}",
                cacheKey);
        }
    }

    public void Set<T>(string cacheKey, T item, MemoryCacheEntryOptions cacheOptions = null)
    {
        if (string.IsNullOrEmpty(cacheKey) || item == null)
            return;

        cacheOptions = PrepareCacheEntryOptions(cacheOptions);
        try
        {
            _memoryCache.Set(cacheKey, item, cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "::: Exception thrown while calling adding to the memory cache. {@cacheKey}, {@item}",
                cacheKey,
                item);
        }
    }
    #endregion
}
