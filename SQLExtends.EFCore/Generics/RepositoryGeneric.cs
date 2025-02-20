using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SQLExtends.EFCore.Entities;

namespace SQLExtends.EFCore.Generics;

public abstract class RepositoryGeneric<TContext, TModel>(TContext context) : IRepositoryGeneric<TModel>, IDisposable
    where TContext : DbContext
    where TModel : class
{
    private readonly TContext _context = context;
    private const int ChunkSize = 1000;

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

    public TModel? Find<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? query.FirstOrDefault(predicate) : query.FirstOrDefault();
    }

    public async Task<TModel?> FindAsync<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
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

    public TModel? Find(Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? query.FirstOrDefault(predicate) : query.FirstOrDefault();
    }

    public async Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? await query.FirstOrDefaultAsync(predicate) : await query.FirstOrDefaultAsync();
    }

    public object? Find<TProperty>(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
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

    public async Task<object?> FindAsync<TProperty>(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
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

    public object? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
 
        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return predicate != null ? query.FirstOrDefault(predicate) : query.FirstOrDefault();
    }

    public async Task<object?> FindAsync(IEnumerable<string> selects, Expression<Func<TModel, bool>> predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        if (selects.Any())
        {
            var selectString = string.Join(",", selects);
            query = (IQueryable<TModel>)query.Select($"new ({selectString})");
        }
        
        return predicate != null ? await query.FirstOrDefaultAsync(predicate) : await query.FirstOrDefaultAsync();
    }

    public IEnumerable<TModel> Get<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? query.Where(predicate).ToList() : query.ToList();
    }

    public async Task<IEnumerable<TModel>> GetAsync<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);
        
        return predicate != null ? await query.Where(predicate).ToListAsync() : await query.ToListAsync();
    }

    public IEnumerable<TModel> Get(Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? query.Where(predicate).ToList() : query.ToList();
    }

    public async Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? await query.Where(predicate).ToListAsync() : await query.ToListAsync();
    }

    public IEnumerable<TModel> GetWithOrder<TProperty>(Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TProperty>>[] includes)
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

    public async Task<IEnumerable<TModel>> GetWithOrderAsync<TProperty>(Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TProperty>>[] includes)
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

    public int Count(Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? query.Count(predicate) : query.Count();
    }

    public async Task<int> CountAsync(Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        return predicate != null ? await query.CountAsync(predicate) : await query.CountAsync();
    }

    /// <param name="pageNum">Starts value on 0</param>
    /// <returns></returns>
    public Paginate<TModel> Paginate<TProperty>(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
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
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public async Task<Paginate<TModel>> PaginateAsync<TProperty>(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes)
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
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public Paginate<TModel> Paginate(int pageNum, int take, Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);
        
        var count = query.Count();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = query.Skip(pageNum).Take(take).ToList();
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public async Task<Paginate<TModel>> PaginateAsync(int pageNum, int take, Expression<Func<TModel, bool>>? predicate)
    {
        var query = _context.Set<TModel>().AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);
        
        var count = await query.CountAsync();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = await query.Skip(pageNum).Take(take).ToListAsync();
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public Paginate<TModel> PaginateWithOrder<TProperty>(int pageNum, int take, Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TProperty>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);
        
        if (order == QueryOrders.Ascending)
            query = query.OrderBy(orderPropertity);
        else if(order == QueryOrders.Descending)
            query = query.OrderByDescending(orderPropertity);
        
        var count = query.Count();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = query.Skip(pageNum).Take(take).ToList();
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public async Task<Paginate<TModel>> PaginateWithOrderAsync<TProperty>(int pageNum, int take, Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate,
        params Expression<Func<TModel, TProperty>>[] includes)
    {
        var query = _context.Set<TModel>().AsQueryable();
        
        if (includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);
        
        if (order == QueryOrders.Ascending)
            query = query.OrderBy(orderPropertity);
        else if(order == QueryOrders.Descending)
            query = query.OrderByDescending(orderPropertity);
        
        var count = await query.CountAsync();
        if (count == 0)
            return [];
        
        pageNum = Math.Min(pageNum, (count / take) * take);
        var result = await query.Skip(pageNum).Take(take).ToListAsync();
        return EFCore.Paginate<TModel>.CreateCustom(result, pageNum, take, count);
    }

    public bool Exists(Expression<Func<TModel, bool>> predicate)
    {
        return _context.Set<TModel>().Any(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate)
    {
        return await _context.Set<TModel>().AnyAsync(predicate);
    }
    
    public async Task UpdateBulkAsync(IEnumerable<TModel> collections, int chunkSize = ChunkSize)
    {
        if (!collections.Any()) return;
        
        var connectionString = _context.Database.GetConnectionString() ?? string.Empty;
        
        string tableName = GetTableName(context.Set<TModel>());
        const string tempTableName = "#TempTable";

        DataTable table = ToDataTable(collections.ToArray());

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            var columnNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]").ToArray());
            var createTableCmd = $"""CREATE TABLE {tempTableName} ({columnNames})""";
            await using (var cmd = new SqlCommand(createTableCmd, connection, transaction))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            using SqlBulkCopy bulkCopy = new(connection, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = tempTableName
            };

            foreach (DataColumn column in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(table);

            var mergeSql = $@"
                MERGE INTO {tableName} AS Target
                USING {tempTableName} AS Source
                ON Target.Id = Source.Id
                WHEN MATCHED THEN
                    UPDATE SET {string.Join(", ", table.Columns.Cast<DataColumn>().Where(c => c.ColumnName != "Id").Select(c => $"Target.[{c.ColumnName}] = Source.[{c.ColumnName}]").ToArray())};";
            
            await using (var cmd = new SqlCommand(mergeSql, connection, transaction))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new SqlCommand($"DROP TABLE {tempTableName}", connection, transaction))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    public async Task InsertBulkAsync(IEnumerable<TModel> collections, int chunkSize = ChunkSize, int maxParallelism = 4)
    {
        if (!collections.Any()) return; // Avoids enumeration

        var tableName = context.Model.FindEntityType(typeof(TModel))?.GetTableName();
        var connectionString = context.Database.GetConnectionString() ?? string.Empty;

        int chunkCount = 0;
        var chunks = collections.Chunk(chunkSize);

        await Parallel.ForEachAsync(chunks, new ParallelOptions { MaxDegreeOfParallelism = maxParallelism }, async (chunk, _) =>
        {
            int currentChunk = Interlocked.Increment(ref chunkCount); // Ensures correct chunk indexing
            await InsertChunkAsync(chunk, tableName, connectionString);
        });
    }
    
    async Task InsertChunkAsync(IEnumerable<TModel> chunk, string tableName, string connectionString)
    {
        await using SqlConnection? connection = new(connectionString);
        await connection.OpenAsync();
        await using SqlTransaction? transaction = connection.BeginTransaction();

        try
        {
            using SqlBulkCopy? bulkCopy = new(connection, SqlBulkCopyOptions.TableLock, transaction);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.BatchSize = ChunkSize;

            DataTable table = ToDataTable(chunk);

            foreach (DataColumn column in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(table);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static DataTable ToDataTable(IEnumerable<TModel> data)
    {
        DataTable table = new(typeof(TModel).Name);
        PropertyInfo[] properties = typeof(TModel).GetProperties()
            .Where(p => p.CanRead
                          && p.GetCustomAttribute<NotMappedAttribute>() == null
                          && p.Name != "Id"
                          && !(typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string)) 
                          && (p.PropertyType.IsPrimitive
                              || p.PropertyType == typeof(string)
                              || p.PropertyType == typeof(decimal)
                              || p.PropertyType == typeof(DateTime)
                              || p.PropertyType.IsEnum
                              || (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))))
            .ToArray();
        
        foreach (var prop in properties)
        {
            Type colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            table.Columns.Add(prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name, colType);
        }

        foreach (TModel? item in data)
        {
            DataRow row = table.NewRow();
            foreach (PropertyInfo? prop in properties)
            {
                row[prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }

        return table;
    }
    
    private static string GetTableName(DbSet<TModel> set)
    {
        var tableAttribute = typeof(TModel).GetCustomAttribute<TableAttribute>()?.Name;
        return tableAttribute ?? set.EntityType.GetTableName() ?? typeof(TModel).Name;
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