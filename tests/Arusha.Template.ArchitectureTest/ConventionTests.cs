namespace Arusha.Template.ArchitectureTest;

/// <summary>
/// Tests to enforce coding conventions and patterns.
/// </summary>
public class ConventionTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Primitives.Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_Events_Should_Have_DomainEvent_Suffix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(Domain.Abstractions.IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .Or()
            .HaveNameEndingWith("IntegrationEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All domain events should have 'DomainEvent' or 'IntegrationEvent' suffix");
    }

    [Fact]
    public void Handlers_In_Features_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Features")
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Command/Query handlers should be sealed");
    }

    [Fact]
    public void Repositories_Should_Have_Repository_Suffix()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespaceContaining("Repositories")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Repository implementations should have 'Repository' suffix");
    }

    [Fact]
    public void Entities_Should_Have_Private_Setters()
    {
        // Get all property names from infrastructure interfaces (allowed to have public setters)
        var auditableProperties = typeof(Domain.Abstractions.IAuditableEntity)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        var softDeletableProperties = typeof(Domain.Abstractions.ISoftDeletable)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        // RowVersion is managed by EF Core for optimistic concurrency
        var baseEntityProperties = typeof(Domain.Primitives.Entity<>)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        var infrastructureProperties = auditableProperties
            .Union(softDeletableProperties)
            .Union(baseEntityProperties)
            .ToHashSet();
        
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Domain.Primitives.Entity<>))
            .GetTypes();

        foreach (var entityType in entityTypes)
        {
            var properties = entityType.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                .Where(p => !infrastructureProperties.Contains(p.Name)); // Exclude infrastructure properties

            properties.Should().BeEmpty(
                $"Entity '{entityType.Name}' should not have public setters (except infrastructure properties: {string.Join(", ", infrastructureProperties)})");
        }
    }

    [Fact]
    public void Value_Objects_Should_Be_Sealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Domain.Primitives.ValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Value objects should be sealed");
    }

    [Fact]
    public void Handlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Handlers should be internal (accessed via MediatR)");
    }
}
