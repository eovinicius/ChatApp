using MediatR;

using SharedKernel;

namespace BuildingBlocks.Messaging;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
