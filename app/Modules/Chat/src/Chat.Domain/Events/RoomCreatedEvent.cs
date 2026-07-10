using SharedKernel;

namespace Chat.Domain.Events;

public record RoomCreatedEvent(Guid RoomId, string RoomName, Guid OwnerId) : IDomainEvent;
