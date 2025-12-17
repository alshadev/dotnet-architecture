namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Application DbContext implementing IUnitOfWork.
/// Demonstrates:
/// - Multi-provider support (SQL Server, PostgreSQL)
/// - Automatic auditing
/// - Domain event dispatching (hybrid: immediate + outbox)
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext, IUnitOfWork
{
    private readonly IMediator _mediator = null;
    private readonly ILogger<ApplicationDbContext> _logger = null;
    private readonly ICurrentUserService _currentUser = null;
    private IDbContextTransaction _currentTransaction = null;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IMediator mediator,
        ILogger<ApplicationDbContext> logger,
        ICurrentUserService currentUser)
        : base(options)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUser = currentUser;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotentRequest> IdempotentRequests => Set<IdempotentRequest>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Override ASP.NET Core Identity table names
        modelBuilder.Entity<IdentityUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Apply soft-delete filter to all soft-deletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var compareExpression = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        UpdateAuditableEntities();
        ApplySoftDeletes();

        // Capture audit trail before persistence
        var auditEntries = BuildAuditEntries();

        // Get all domain events from aggregates
        var domainEvents = GetDomainEvents();

        // Dispatch immediate events (same transaction)
        await DispatchImmediateDomainEventsAsync(domainEvents, cancellationToken);

        // Convert integration events to outbox messages
        AddIntegrationEventsToOutbox(domainEvents);

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count != 0)
        {
            AuditTrails.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        // Clear domain events from aggregates
        ClearDomainEvents();

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        var utcNow = DateTime.UtcNow;
        var userId = _currentUser?.UserId ?? "system";

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

    private void ApplySoftDeletes()
    {
        var softDeleteEntries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        var utcNow = DateTime.UtcNow;
        var userId = _currentUser?.UserId ?? "system";

        foreach (var entry in softDeleteEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedOnUtc = utcNow;
            entry.Entity.DeletedBy = userId;
        }
    }

    private List<AuditTrail> BuildAuditEntries()
    {
        var utcNow = DateTime.UtcNow;
        var userId = _currentUser?.UserId ?? "system";

        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditTrail)
            .Where(e => e.Entity is not OutboxMessage)
            .ToList();

        var auditEntries = new List<AuditTrail>();

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
                audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Added)
            {
                audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
            }

            auditEntries.Add(audit);
        }

        return auditEntries;
    }

    private List<IDomainEvent> GetDomainEvents()
    {
        return ChangeTracker.Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
    }

    private async Task DispatchImmediateDomainEventsAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        var immediateEvents = domainEvents
            .Where(e => e is IImmediateDomainEvent)
            .ToList();

        if (_mediator is null)
        {
            return;
        }

        var logger = _logger ?? NullLogger<ApplicationDbContext>.Instance;

        foreach (var domainEvent in immediateEvents)
        {
            logger.LogDebug(
                "Dispatching immediate domain event {EventType}",
                domainEvent.GetType().Name);

            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    private void AddIntegrationEventsToOutbox(IEnumerable<IDomainEvent> domainEvents)
    {
        var integrationEvents = domainEvents
            .Where(e => e is IIntegrationEvent)
            .ToList();

        foreach (var integrationEvent in integrationEvents)
        {
            var outboxMessage = OutboxMessage.Create(integrationEvent);
            OutboxMessages.Add(outboxMessage);

            (_logger ?? NullLogger<ApplicationDbContext>.Instance).LogDebug(
                "Added integration event {EventType} to outbox",
                integrationEvent.GetType().Name);
        }
    }

    private void ClearDomainEvents()
    {
        var aggregateRoots = ChangeTracker.Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }
    }
}
