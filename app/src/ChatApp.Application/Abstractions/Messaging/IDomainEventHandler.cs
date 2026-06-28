using ChatApp.Domain.Abstractions;

using MediatR;

namespace ChatApp.Application.Abstractions.Messaging;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
