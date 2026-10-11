namespace Shared.ViewModels.Commons;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int NoOfPages { get; set; }
    public string SearchString { get; set; }
    public string Sort { get; set; }
    public bool Descending { get; set; }

    public bool HasPreviousPage => PageNo > 1;
    public bool HasNextPage => PageNo < NoOfPages;

    public static PaginatedResult<T> Create(IEnumerable<T> data, int totalRecords, PageRequest request)
    {
        return new PaginatedResult<T>
        {
            Data = data.ToList(),
            TotalRecords = totalRecords,
            PageNo = request.PageNo,
            PageSize = request.PageSize,
            NoOfPages = totalRecords == 0 ? 0 : (int)Math.Ceiling((double)totalRecords / request.PageSize),
            SearchString = request.SearchString,
            Sort = request.Sort,
            Descending = request.Descending
        };
    }
}
