using SharedKernel;

namespace Chat.Domain.Events;

public record UserRegisteredEvent(Guid UserId, string Username) : IDomainEvent;
