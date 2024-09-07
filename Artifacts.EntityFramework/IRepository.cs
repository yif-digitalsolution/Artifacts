using System.Linq.Expressions;

namespace Artifacts.EntityFramework;

public interface IRepository<T>
{
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> SearchASync(Expression<Func<T, bool>> predicate);
}