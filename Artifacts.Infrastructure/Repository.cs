using System.Linq.Expressions;
using Artifacts.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Artifacts.Infrastructure;

public class Repository<T, TContext, TKey> : IRepository<T, TContext, TKey> where T : class, IEntity<TKey>, new() where TContext : DbContext //IAuditableEntity, new()
{
    private readonly TContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _disposed = false;
    public Repository(TContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    //TODO: hacer uso de currentUser 
    /*
Applicacation      
     public interface ICurrentUser
{
    int UserId { get; }
    int CompanyId { get; }
}

    Infra
    public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContext;

    public CurrentUser(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public int UserId =>
        int.Parse(_httpContext.HttpContext!.User.FindFirst("UserId")!.Value);

    public int CompanyId =>
        int.Parse(_httpContext.HttpContext!.User.FindFirst("CompanyId")!.Value);
}

     
     
     */

    public async Task<T> InsertAsync(T entity, CancellationToken ct, bool commitChanges = true )
    {
        try
        {
            //TODO: Validar que el usuario que esta insertando sea el mismo que creo el registro
            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.CreatedAt = DateTime.Now;
                auditableEntity.CreatedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
                auditableEntity.UpdatedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
                auditableEntity.UpdatedAt = DateTime.Now;
            }
            if (entity is ICompany<TKey> companyEntity)
            {
                var companyIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "CompanyId");
                
                companyEntity.Company = (TKey)Convert.ChangeType(companyIdClaim.Value, typeof(TKey));
            }  

            await _dbContext.Set<T>().AddAsync(entity);
            if(commitChanges )
                await _dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception e)
        {
            throw new Exception("No se pudo insertar el registro", e);
        }
    }
    public async Task<T> UpdateAsync(T entity)
    {
        var result = await _dbContext.Set<T>().FirstOrDefaultAsync(x => EqualityComparer<TKey>.Default.Equals(x.Id, entity.Id));
        if (result == null)
        {
            throw new Exception("No se encontró el registro para actualizar");
        }

        if (entity is IAuditable auditableEntity && result is IAuditable resultAuditable)
        {
            auditableEntity.UpdatedBy = GetCurrentUserId()?.ToString();
            auditableEntity.UpdatedAt = DateTime.Now;
        }

        _dbContext.Entry(result).CurrentValues.SetValues(entity);
        await _dbContext.SaveChangesAsync();


        return entity;
    }
    public async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(x => EqualityComparer<TKey>.Default.Equals(x.Id, id));
        if (entity == null)
        {
            throw new Exception("No se encontró el registro para eliminar");
        }

        if (entity is ISoftDelete auditableEntity)
        {
            auditableEntity.DeletedBy = GetCurrentUserId()?.ToString();
            auditableEntity.DeletedAt  = DateTime.Now;
            _dbContext.Update(auditableEntity);
        }
        else
        {
            _dbContext.Remove(entity);
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<T?> GetByIdAsync(TKey id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();



        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDelete)e).DeletedBy == null);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync(e => EqualityComparer<TKey>.Default.Equals(e.Id, id));
    }

    public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDelete)e).DeletedBy == null);
        }

        return await query.AsNoTracking().ToListAsync();
    }


    public async Task<PagedResult<T>> PaginedSearchAsync(Expression<Func<T, bool>> predicate, int pageIndex = 1,
                                            int pageSize = 10, params Expression<Func<T, object>>[] includes)
    {

        IQueryable<T> query = SearchAsync(predicate, pageIndex, pageSize, includes);
        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var items = await query
            .ToListAsync();

        return new PagedResult<T>(
                         items,
                         pageIndex,
                         pageSize,
                         totalRecords,
                         totalPages
                     );
     
    }

    public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)

    {
        return await SearchAsync(predicate, pageIndex: null, pageSize: null, includes).ToListAsync();
    }


    private IQueryable<T> SearchAsync(Expression<Func<T, bool>> predicate, int? pageIndex = null,
                                            int? pageSize = null, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDelete)e).DeletedBy == null);
        }

        query = query.Where(predicate);

        if (pageIndex.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        return query.AsNoTracking().Where(predicate);
    }

    public async Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDelete)e).DeletedBy == null);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync(predicate);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext?.Dispose();
            }
            _disposed = true;
        }
    }

    public DbContext DbContext()
    {
        return _dbContext;


    }


    private TKey GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId");
        if (userIdClaim == null)
        {
            throw new Exception("No se encontró el UserId en los claims del usuario.");
        }
        return (TKey)Convert.ChangeType(userIdClaim.Value, typeof(TKey));
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}