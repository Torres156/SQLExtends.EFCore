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

    public TModel? Find(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? query.FirstOrDefault(predicate) : query.FirstOrDefault();
    }

    public async Task<TModel?> FindAsync(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
            {
                if (include.ReturnType is EntitySoftDelete)
                    (include as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
                
                query = query.Include(include);
            }
        
        return predicate != null ? await query.FirstOrDefaultAsync(predicate) : await query.FirstOrDefaultAsync();
    }

    public object? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return predicate != null ? query.FirstOrDefault(predicate) : query.FirstOrDefault();
    }

    public async Task<object?> FindAsync(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return predicate != null ? await query.FirstOrDefaultAsync(predicate) : await query.FirstOrDefaultAsync();
    }

    public IEnumerable<TModel> Get(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? query.Where(predicate).ToList() : query.ToList();
    }

    public async Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? await query.Where(predicate).ToListAsync() : await query.ToListAsync();
    }

    public int Count(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? query.Count(predicate) : query.Count();
    }

    public async Task<int> CountAsync(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? await query.CountAsync(predicate) : await query.CountAsync();
    }

    /// <param name="pageNum">Starts value on 0</param>
    /// <returns></returns>
    public Paginate<TModel> Paginate(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);
        
        var count = query.Count();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = query.Skip(pageNum).Take(take).ToList();
        return Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public async Task<Paginate<TModel>> PaginateAsync(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);
        
        var count = await query.CountAsync();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = await query.Skip(pageNum).Take(take).ToListAsync();
        return Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}