namespace Arusha.Template.ArchitectureTest;

/// <summary>
/// Architecture tests to enforce Clean Architecture dependency rules.
/// Uses NetArchTest.Rules for compile-time enforcement.
/// </summary>
public class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Primitives.Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Any_Other_Projects()
    {
        // Arrange
        var otherProjects = new[]
        {
            ApplicationAssembly.GetName().Name,
            InfrastructureAssembly.GetName().Name
        };

        // Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAll(otherProjects!)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not reference Application or Infrastructure layers");
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        // Arrange
        var infrastructureProject = InfrastructureAssembly.GetName().Name;

        // Act
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(infrastructureProject!)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not reference Infrastructure layer");
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api()
    {
        // Arrange - Infrastructure should not reference the API layer
        // This ensures proper dependency direction
        
        // Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Arusha.Template.Api")
            .GetResult();

        // Assert - Infrastructure should not depend on API
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should not reference API layer");
    }
}
