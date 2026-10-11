using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

namespace Shared.Helpers;

public class JsonStringLocalizer : IStringLocalizer
{
    #region Constants
    private string _cultureName = CultureInfo.CurrentCulture.Name;
    private readonly JsonSerializer _serializer = new();
    #endregion

    #region Properties
    /// <summary>
    /// {0}: Resource Key
    /// </summary>
    private string _cacheKeyFormatted => $"Locale.{_cultureName}.{{0}}";
    private string _relativeFilePath => $"Resources/{_cultureName}.json";

    #endregion

    #region Fields
    private readonly IMemoryCacheService _memoryCacheService;
    #endregion

    #region Ctor
    public JsonStringLocalizer(IMemoryCacheService memoryCacheService)
    {
        _memoryCacheService = memoryCacheService;
    }
    #endregion

    #region Utils
    private string GetValueFromJSON(string name, string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return string.Empty;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(sReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName &&
                        string.Compare((string)jsonReader.Value, name, true) == 0)
                    {
                        jsonReader.Read();
                        return _serializer.Deserialize<string>(jsonReader);
                    }
                }

                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"::: Error reading JSON file: {ex.Message}");
            throw;
        }

    }
    private string GetString(string resourceKey)
    {
        // Prepare CacheKey 
        var cacheKey = string.Format(_cacheKeyFormatted, resourceKey);
        // Local function 
        string acuire()
        {
            var fullFilePath = Path.GetFullPath(_relativeFilePath);
            var result = GetValueFromJSON(resourceKey, fullFilePath);
            return result;
        }

        var result = _memoryCacheService.GetOrSet(
            cacheKey,
            acuire,
            new MemoryCacheEntryOptions());
        return result;
    }

    #endregion

    #region Implementation
    public LocalizedString this[string name]
    {
        get
        {
            string value = GetString(name);
            bool resourceNotFound = string.IsNullOrWhiteSpace(value);
            value = resourceNotFound ? name : value;
            return new LocalizedString(name, value, resourceNotFound);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localizedString = this[name];
            return !localizedString.ResourceNotFound
                ? new LocalizedString(name, string.Format(localizedString.Value, arguments), false)
                : localizedString;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    #endregion
}
