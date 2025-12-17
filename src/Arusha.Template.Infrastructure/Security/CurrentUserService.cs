namespace Arusha.Template.Infrastructure.Security;

/// <summary>
/// Resolves current user and correlation id from the HTTP context.
/// </summary>
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    public string CorrelationId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return "system";
            }

            if (httpContext.Items.TryGetValue("CorrelationId", out var value) && value is string correlationId)
            {
                return correlationId;
            }

            var generated = httpContext.TraceIdentifier;
            httpContext.Items["CorrelationId"] = generated;
            return generated;
        }
    }
}
