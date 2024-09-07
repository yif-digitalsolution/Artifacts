using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Artifacts.EntityFramework;

public class Repository<T> : IRepository<T>, IDisposable where T : class, IEntity, IAuditableEntity, new()
{
    private readonly DbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public Repository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T> InsertAsync(T entity)
    {
        try
        {
            //TODO: Validar que el usuario que esta insertando sea el mismo que creo el registro
            entity.CreatedDate = DateTime.Now;
            //entity.CreatedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            //entity.LastModifiedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            entity.LastModifiedDate = DateTime.Now;
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

        //TODO: Validar que el usuario que esta actualizando sea el mismo que creo el registro

        //entity.LastModifiedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        entity.LastModifiedDate = DateTime.Now;
        entity.CreatedDate = result.CreatedDate;
        entity.CreatedBy = result.CreatedBy;
        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = _dbContext.Set<T>().FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {
            //TODO: Hacer un log y exception 
            return false;
        }

        _dbContext.Set<T>().Remove(entity);
        _dbContext.SaveChanges();
        return true;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Set<T>().ToListAsync();

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<IEnumerable<T>> SearchASync(Expression<Func<T, bool>> predicate)
    {
        var result = await _dbContext.Set<T>().Where(predicate).ToListAsync();
        return result;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}