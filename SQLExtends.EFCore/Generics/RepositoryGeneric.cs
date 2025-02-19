﻿using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SQLExtends.EFCore.Entities;

namespace SQLExtends.EFCore.Generics;

public abstract class RepositoryGeneric<TContext, TModel>(DbContext context) : IRepositoryGeneric<TModel>, IDisposable
    where TContext : DbContext
    where TModel : class
{
    private readonly TContext _context = (TContext)context;

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

    public void InsertRange(IEnumerable<TModel> models)
    {
        _context.Set<TModel>().AddRange(models);
        _context.SaveChanges();
    }

    public async Task InsertRangeAsync(IEnumerable<TModel> models)
    {
        await _context.Set<TModel>().AddRangeAsync(models);
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

    public void UpdateRange(IEnumerable<TModel> models)
    {
        _context.Set<TModel>().UpdateRange(models);
        _context.SaveChanges();
    }

    public async Task UpdateRangeAsync(IEnumerable<TModel> models)
    {
        _context.Set<TModel>().UpdateRange(models);
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

    public void DeleteRange(IEnumerable<TModel> models)
    {
        if (typeof(TModel).IsSubclassOf(typeof(EntitySoftDelete)))
        {
            foreach (var model in models)
                (model as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
            
            _context.Set<TModel>().UpdateRange(models);
        }
        else
            _context.Set<TModel>().RemoveRange(models);
        
        _context.SaveChanges();
    }

    public async Task DeleteRangeAsync(IEnumerable<TModel> models)
    {
        if (typeof(TModel).IsSubclassOf(typeof(EntitySoftDelete)))
        {
            foreach (var model in models)
                (model as EntitySoftDelete)!.DeletedAt = DateTime.UtcNow.ToTimeZone();
            
            _context.Set<TModel>().UpdateRange(models);
        }
        else
            _context.Set<TModel>().RemoveRange(models);
        
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

    public IEnumerable<TModel> GetWithOrder(Expression<Func<TModel, TModel>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (order == QueryOrders.Ascending)
            query = query.OrderBy(orderPropertity);
        else if(order == QueryOrders.Descending)
            query = query.OrderByDescending(orderPropertity);
        
        return predicate != null ? query.Where(predicate).ToList() : query.ToList();
    }

    public async Task<IEnumerable<TModel>> GetWithOrderAsync(Expression<Func<TModel, TModel>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TModel>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        if (order == QueryOrders.Ascending)
            query = query.OrderBy(orderPropertity);
        else if(order == QueryOrders.Descending)
            query = query.OrderByDescending(orderPropertity);
        
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

    public bool Exists(Expression<Func<TModel, bool>> predicate)
    {
        return _context.Set<TModel>().Any(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate)
    {
        return await _context.Set<TModel>().AnyAsync(predicate);
    }

    public IQueryable<TModel> Query()
    {
        return _context.Set<TModel>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}