namespace Shared.ViewModels.Commons;

public sealed class PageRequest
{
    public const int DefaultPageSize = 25;
    public const int MaximumPageSize = 100;
    public static readonly int[] AllowedPageSizes = new[] { 10, 25, 50, 100 };

    private int _pageNo = 1;
    private int _pageSize = DefaultPageSize;

    public int PageNo
    {
        get => _pageNo;
        set => _pageNo = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = NormalizePageSize(value);
    }

    public string SearchString { get; set; }
    public string Sort { get; set; }
    public bool Descending { get; set; }

    /// <summary>Returns a representable SQL offset without throwing on hostile page numbers.</summary>
    public bool TryGetOffset(out int offset)
    {
        try
        {
            offset = checked((PageNo - 1) * PageSize);
            return true;
        }
        catch (OverflowException)
        {
            offset = 0;
            return false;
        }
    }

    public int Offset => TryGetOffset(out var offset) ? offset : int.MaxValue;

    public static int NormalizePageSize(int requested)
    {
        if (requested > MaximumPageSize)
            return MaximumPageSize;

        return AllowedPageSizes.Contains(requested) ? requested : DefaultPageSize;
    }
}
