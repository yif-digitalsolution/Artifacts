using Artifacts.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Artifacts.Infrastructure;

public interface IRepository<T, TContext,TKey> where T : class, IEntity<TKey>, new () where TContext : DbContext
{
    Task<T> InsertAsync(T entity, CancellationToken ct, bool commitChanges = true);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
    Task<T?> GetByIdAsync(TKey id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<PagedResult<T>> PaginedSearchAsync(Expression<Func<T, bool>> predicate, int pageIndex = 1,
                                            int pageSize = 10, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
}