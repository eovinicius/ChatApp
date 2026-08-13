using Chat.Domain.Conversations;

using SharedKernel;

namespace Chat.Domain.Events;

// Os eventos carregam só identificadores: quem trata carrega a conversa para
// descobrir os destinatários atuais, evitando lista de participantes defasada.
public record ConversationCreatedEvent(Guid ConversationId, ConversationType Type, Guid CreatorId) : IDomainEvent;

public record ParticipantAddedEvent(Guid ConversationId, Guid UserId) : IDomainEvent;

public record ParticipantRemovedEvent(Guid ConversationId, Guid UserId) : IDomainEvent;

public record MessagesReadEvent(Guid ConversationId, Guid UserId, Guid LastReadMessageId, DateTime LastReadMessageSentAt) : IDomainEvent;
