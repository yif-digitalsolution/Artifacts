using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Utils.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Artifacts.EntityFramework;

public class Repository<T, TContext> : IRepository<T, TContext> where T : class, IEntity, new() where TContext : DbContext //IAuditableEntity, new()
{
    private readonly TContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _disposed = false;
    public Repository(TContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T> InsertAsync(T entity)
    {
        try
        {
            //TODO: Validar que el usuario que esta insertando sea el mismo que creo el registro
            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.CreatedDate = DateTime.Now;
                auditableEntity.CreatedBy = 1;//_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                auditableEntity.LastModifiedBy = 1;// _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                auditableEntity.LastModifiedDate = DateTime.Now;
            }

            await _dbContext.Set<T>().AddAsync(entity);
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
        var result = await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (result == null)
        {
            throw new Exception("No se encontró el registro para actualizar");
        }

        if (entity is IAuditableEntity auditableEntity && result is IAuditableEntity resultAuditable)
        {
            auditableEntity.LastModifiedBy = 1; //_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            auditableEntity.LastModifiedDate = DateTime.Now;
            auditableEntity.CreatedDate = resultAuditable.CreatedDate;
            auditableEntity.CreatedBy = resultAuditable.CreatedBy;
        }

        _dbContext.Entry(result).CurrentValues.SetValues(entity);
        await _dbContext.SaveChangesAsync();


        return entity;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = _dbContext.Set<T>().FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {
            throw new NotFoundException($"No se encontro {nameof(T)} con el Id = {id}");
        }

        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();



        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IAuditableEntity)e).DeletedBy == null);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IAuditableEntity)e).DeletedBy == null);
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

        return new PagedResult<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
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

        if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IAuditableEntity)e).DeletedBy == null);
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

    public async Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IAuditableEntity)e).DeletedBy == null);
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
}