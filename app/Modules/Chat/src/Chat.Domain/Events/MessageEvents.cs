using SharedKernel;

namespace Chat.Domain.Events;

public record MessageSentEvent(Guid MessageId, Guid ConversationId, Guid SenderId, DateTime SentAt) : IDomainEvent;

public record MessageEditedEvent(Guid MessageId, Guid ConversationId, DateTime EditedAt) : IDomainEvent;

public record MessageDeletedEvent(Guid MessageId, Guid ConversationId, DateTime DeletedAt) : IDomainEvent;
