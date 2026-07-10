using BuildingBlocks.Clock;

namespace Chat.Infrastructure.Services;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
