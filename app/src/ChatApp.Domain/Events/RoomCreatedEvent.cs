using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Events;

public record RoomCreatedEvent(Guid RoomId, string RoomName, Guid OwnerId) : IDomainEvent;
