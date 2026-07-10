namespace BuildingBlocks.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}