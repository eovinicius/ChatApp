using ChatApp.Application.Abstractions.Clock;

namespace ChatApp.Infrastructure.Services;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
