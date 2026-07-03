using Chat.Domain.Abstractions;

using MediatR;

namespace Chat.Application.Abstractions.Messaging;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
