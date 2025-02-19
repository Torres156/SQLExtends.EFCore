using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SQLExtends.EFCore;

public static class BulkInsertExtends
{
    const int ChunkSize = 1000;

    public static async Task InsertBulkAsync<T>(this DbSet<T> set, IEnumerable<T> collections, int chunkSize = ChunkSize) where T : class
    {
        if (!collections.Any()) return;

        var chunks = collections
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / chunkSize)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        var tableName = GetTableName(set);
        
        var connectionString = set.GetService<DbContext>().Database.GetConnectionString() ?? string.Empty;
        
        await Task.WhenAll(chunks.Select(chunk => InsertChunkAsync(chunk, tableName, connectionString)));
    }
    
    public static async Task UpdateBulkAsync<T>(this DbSet<T> set, IEnumerable<T> collections, int chunkSize = ChunkSize) where T : class
    {
        if (!collections.Any()) return;
        
        var connectionString = set.GetService<DbContext>().Database.GetConnectionString() ?? string.Empty;
        
        string tableName = GetTableName(set);
        const string tempTableName = "#TempTable";

        DataTable table = ToDataTable(collections.ToArray());

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            var columnNames = string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]").ToArray());
            var createTableCmd = $"CREATE TABLE {tempTableName} ({columnNames})";
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
    
    private static async Task InsertChunkAsync<T>(IEnumerable<T> chunk, string tableName, string connectionString) where T : class
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

    private static DataTable ToDataTable<T>(IEnumerable<T> data)
    {
        DataTable table = new(typeof(T).Name);
        PropertyInfo[] properties = typeof(T).GetProperties()
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

        foreach (T? item in data)
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
    
    private static string GetTableName<T>(DbSet<T> set) where T : class
    {
        var tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
        return tableAttribute ?? set.EntityType.GetTableName() ?? typeof(T).Name;
    }
}