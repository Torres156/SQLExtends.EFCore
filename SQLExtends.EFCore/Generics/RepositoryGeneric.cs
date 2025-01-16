using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SQLExtends.EFCore.Entities;

namespace SQLExtends.EFCore.Generics;

public abstract class RepositoryGeneric<TContext, TModel> : IRepositoryGeneric<TModel>, IDisposable
    where TContext : DbContext
    where TModel : class
{
    protected TContext _context;
    
    private RepositoryGeneric()
    {
        _context = (TContext?)Activator.CreateInstance(typeof(TContext), new object[] { new DbContextOptions<TContext>() })
                   ?? throw new NullReferenceException("The context was null.");
    }
    
    public void Insert(TModel model)
    {
        _context.Set<TModel>().Add(model);
        _context.SaveChanges();
    }

    public async Task InsertAsync(TModel model)
    {
        await _context.Set<TModel>().AddAsync(model);
        await _context.SaveChangesAsync();
    }

    public void Update(TModel model)
    {
        _context.Set<TModel>().Update(model);
        _context.SaveChanges();
    }

    public async Task UpdateAsync(TModel model)
    {
        _context.Set<TModel>().Update(model);
        await _context.SaveChangesAsync();
    }

    public void Delete(TModel model)
    {
        if (typeof(TModel).IsSubclassOf(typeof(EntitySoftDelete)))
        {
            (model as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
            _context.Set<TModel>().Update(model);
        }
        else
            _context.Set<TModel>().Remove(model);
        
        _context.SaveChanges();
    }

    public async Task DeleteAsync(TModel model)
    {
        if (typeof(TModel).IsSubclassOf(typeof(EntitySoftDelete)))
        {
            (model as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
            _context.Set<TModel>().Update(model);
        }
        else
            _context.Set<TModel>().Remove(model);
        
        await _context.SaveChangesAsync();
    }

    public TModel? Find(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
            {
                if (include.ReturnType is EntitySoftDelete)
                    (include as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
                
                query = query.Include(include);
            }
        
        return query.FirstOrDefault(predicate);
    }

    public async Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
            {
                if (include.ReturnType is EntitySoftDelete)
                    (include as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
                
                query = query.Include(include);
            }
        
        return await query.FirstOrDefaultAsync(predicate);
    }

    public TModel? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
            {
                if (include.ReturnType is EntitySoftDelete)
                    (include as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
                
                query = query.Include(include);
            }

        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return query.FirstOrDefault(predicate);
    }

    public async Task<TModel?> FindAsync(IEnumerable<string> selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
            {
                if (include.ReturnType is EntitySoftDelete)
                    (include as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
                
                query = query.Include(include);
            }

        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return await query.FirstOrDefaultAsync(predicate);
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}