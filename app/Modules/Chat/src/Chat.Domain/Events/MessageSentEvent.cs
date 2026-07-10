using SharedKernel;

namespace Chat.Domain.Events;

public record MessageSentEvent(Guid MessageId, Guid ChatRoomId, Guid SenderId) : IDomainEvent;
