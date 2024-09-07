using System.Linq.Expressions;

namespace Artifacts.EntityFramework;

public class QueryParameters<T>
{
    public QueryParameters(int pagina, int top)
    {
        Pagina = pagina;
        Top = top;
        Filter = null;
        OrderBy = null;
        OrderByDescending = null;
    }
    public int Pagina { get; set; }
    public int Top { get; set; }
    public Expression<Func<T, bool>> Filter { get; set; }
    public Func<T, object> OrderBy { get; set; }
    public Func<T, object> OrderByDescending { get; set; }
}