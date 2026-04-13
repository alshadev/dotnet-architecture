namespace Arusha.Template.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically populates audit fields and builds audit trail entries.
/// Audit trail entries are added to the same SaveChanges call (single DB roundtrip).
/// </summary>
internal sealed class AuditSaveChangesInterceptor(
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not ApplicationDbContext dbContext)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var utcNow = dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var userId = currentUser?.UserId ?? "system";

        UpdateAuditableEntities(dbContext, utcNow, userId);
        ApplySoftDeletes(dbContext, utcNow, userId);
        AddAuditTrailEntries(dbContext, utcNow, userId);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(ApplicationDbContext context, DateTime utcNow, string userId)
    {
        var entries = context.ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOnUtc = utcNow;
                entry.Entity.CreatedBy = userId;
            }
            else
            {
                entry.Entity.ModifiedOnUtc = utcNow;
                entry.Entity.ModifiedBy = userId;
            }
        }
    }

    private static void ApplySoftDeletes(ApplicationDbContext context, DateTime utcNow, string userId)
    {
        var softDeleteEntries = context.ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in softDeleteEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedOnUtc = utcNow;
            entry.Entity.DeletedBy = userId;
        }
    }

    private static void AddAuditTrailEntries(ApplicationDbContext context, DateTime utcNow, string userId)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditTrail)
            .ToList();

        if (entries.Count == 0) return;

        foreach (var entry in entries)
        {
            var audit = new AuditTrail
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                Key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty,
                Action = entry.State.ToString(),
                PerformedBy = userId,
                PerformedOnUtc = utcNow
            };

            if (entry.State == EntityState.Modified)
            {
                audit.OldValues = System.Text.Json.JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                audit.NewValues = System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Added)
            {
                audit.NewValues = System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                audit.OldValues = System.Text.Json.JsonSerializer.Serialize(entry.OriginalValues.ToObject());
            }

            context.AuditTrails.Add(audit);
        }
    }
}
