using SharedKernel;

namespace Identity.Domain.Events;

public record UserRegisteredEvent(Guid UserId, string Username) : IDomainEvent;
