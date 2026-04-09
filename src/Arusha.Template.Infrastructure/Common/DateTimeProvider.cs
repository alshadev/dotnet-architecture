namespace Arusha.Template.Infrastructure.Common;

/// <summary>
/// Default implementation of IDateTimeProvider using system clock.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
