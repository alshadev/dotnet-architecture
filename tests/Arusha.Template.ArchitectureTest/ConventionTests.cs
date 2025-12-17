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
    public void Command_Handlers_Should_Have_Handler_Suffix()
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
        // Auditable properties that are allowed to have public setters (for EF Core)
        var auditableProperties = new[] { "CreatedOnUtc", "ModifiedOnUtc" };
        
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Domain.Primitives.Entity<>))
            .GetTypes();

        foreach (var entityType in entityTypes)
        {
            var properties = entityType.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                .Where(p => !auditableProperties.Contains(p.Name)); // Exclude auditable properties

            properties.Should().BeEmpty(
                $"Entity '{entityType.Name}' should not have public setters (except auditable properties)");
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
