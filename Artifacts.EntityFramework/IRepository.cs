using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Artifacts.EntityFramework;

public interface IRepository<T, TContext> where T : class, IEntity, new () where TContext : DbContext
{
    //DbContext DbContext();
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate,  params Expression<Func<T, object>>[] includes);

    Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
}