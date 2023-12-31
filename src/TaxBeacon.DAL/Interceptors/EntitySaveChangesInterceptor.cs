﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TaxBeacon.DAL.Interceptors;

public static class EntityEntryExtension
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry is not null
            && r.TargetEntry.Metadata.IsOwned()
            && r.TargetEntry.State is EntityState.Added or EntityState.Modified);
}

public class EntitySaveChangesInterceptor: SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDateTimeUtc = DateTime.UtcNow;
            }

            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.LastModifiedDateTimeUtc = DateTime.UtcNow;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedDateTimeUtc = DateTime.UtcNow;
                entry.State = EntityState.Modified;
            }
        }
    }
}
