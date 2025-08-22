using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Utils.Exceptions;

namespace Artifacts.EntityFramework;

public class Repository<T,TContext> : IRepository<T, TContext> where T : class, IEntity, new () where TContext : DbContext //IAuditableEntity, new()
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
        _dbContext.Update(entity);
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

        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        return await query.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(predicate);
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