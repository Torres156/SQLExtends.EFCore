using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SQLExtends.EFCore.Entities;

namespace SQLExtends.EFCore;

public static class ContextExtends
{
    public static void SetTimestamps(this DbContext context)
    {
        var entities = context.ChangeTracker.Entries()
            .Where(x => x.Entity is EntityGeneric && (x.State == EntityState.Added || x.State == EntityState.Modified));

        var now = DateTime.UtcNow;
        now = TimeZoneInfo.ConvertTime(now, DateTimeExtends.TimeZoneDefault);
        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((EntityGeneric)entity.Entity).CreatedAt = now;
                ((EntityGeneric)entity.Entity).UpdatedAt = now;
            }
            else
            {
                entity.Property("CreatedAt").IsModified = false;
                ((EntityGeneric)entity.Entity).UpdatedAt = now;
            }
        }
    }

    public static void SetSoftDelete(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(EntitySoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var clrType = entityType.ClrType;
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(EntitySoftDelete.DeletedAt));
                var nullValue = Expression.Constant(null, typeof(DateTime?));
                var comparison = Expression.Equal(property, nullValue);
                var lambda = Expression.Lambda(comparison, parameter);

                builder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }
}