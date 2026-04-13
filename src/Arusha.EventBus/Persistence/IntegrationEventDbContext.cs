namespace Arusha.EventBus;

public class IntegrationEventDbContext : DbContext
{
    public DbSet<IntegrationEventLog> IntegrationEventLogs => Set<IntegrationEventLog>();

    public IntegrationEventDbContext(DbContextOptions<IntegrationEventDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IntegrationEventLog>(builder =>
        {
            builder.ToTable("IntegrationEventLogs", "eventbus");

            builder.HasKey(e => new { e.EventId, e.Type, e.EventHandler });

            builder.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Event)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.EventHandler)
                .HasMaxLength(255)
                .HasDefaultValue(string.Empty)
                .IsRequired();

            builder.Property(e => e.Payload)
                .IsRequired();

            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.Event);
            builder.HasIndex(e => e.EventHandler);
            builder.HasIndex(e => e.CreatedOnUtc);
            builder.HasIndex(e => e.LastAttemptOnUtc);
        });
    }
}
