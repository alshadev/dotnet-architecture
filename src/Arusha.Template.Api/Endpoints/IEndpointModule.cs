namespace Arusha.Template.Api.Endpoints;

/// <summary>
/// Base interface for endpoint modules.
/// Implements the REPR pattern endpoint organization.
/// </summary>
public interface IEndpointModule
{
    /// <summary>
    /// Maps the endpoints for this module.
    /// </summary>
    void MapEndpoints(IEndpointRouteBuilder app);
}
