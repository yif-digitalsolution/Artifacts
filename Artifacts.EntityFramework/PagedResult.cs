namespace Artifacts.EntityFramework;

public class PagedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; }
}