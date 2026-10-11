namespace Shared.Options;

public class InMemoryCacheOptions
{
    public string Prefix { get; set; }
    public string SlidingExpiration { get; set; }
    public string AbsoluteExpirationRelativeToNow { get; set; }
}
