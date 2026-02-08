namespace Artifacts.Utils;

public class PagedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; }

    public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount, int totalPages)
    {
        Items = items;
        PageSize = page;
        PageSize = pageSize;
        TotalRecords = totalCount;
        TotalPages = totalPages;
    }
}