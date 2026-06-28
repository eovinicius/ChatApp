using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Events;

public record MessageSentEvent(Guid MessageId, Guid ChatRoomId, Guid SenderId) : IDomainEvent;
