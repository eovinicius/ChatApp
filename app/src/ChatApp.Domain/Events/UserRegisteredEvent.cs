using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Events;

public record UserRegisteredEvent(Guid UserId, string Username) : IDomainEvent;
