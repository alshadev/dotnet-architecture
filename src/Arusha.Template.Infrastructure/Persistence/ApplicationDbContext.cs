namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Application DbContext implementing IUnitOfWork.
/// Audit trail is handled by AuditSaveChangesInterceptor (single SaveChanges roundtrip).
/// Domain events are dispatched by DomainEventDispatcher (supports cascading).
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext, IUnitOfWork
{
    private readonly DomainEventDispatcher _domainEventDispatcher;
    private IDbContextTransaction _currentTransaction;

    /// <summary>
    /// Design-time constructor for EF Core migrations tooling.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        DomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
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

        // Apply soft-delete filter and concurrency tokens to domain entities
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

            if (entityType.ClrType.GetProperty("RowVersion") is not null
                && IsEntityType(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .IsRowVersion();
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before save (supports cascading)
        if (_domainEventDispatcher is not null)
        {
            await _domainEventDispatcher.DispatchAndClearAsync(this, cancellationToken);
        }

        // Audit fields, soft deletes, and audit trail entries are handled by AuditSaveChangesInterceptor
        // Everything is persisted in a single SaveChanges call
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null) return;

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
        if (_currentTransaction is null) return;

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <summary>
    /// Checks if a type inherits from Entity&lt;TId&gt; anywhere in its hierarchy.
    /// </summary>
    private static bool IsEntityType(Type type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
