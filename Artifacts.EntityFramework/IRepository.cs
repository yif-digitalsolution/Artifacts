using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Artifacts.EntityFramework;

public interface IRepository<T>
{
    //DbContext DbContext();
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate);

    Task<T?> First(Expression<Func<T, bool>> predicate);
}