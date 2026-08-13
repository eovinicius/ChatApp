using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.GetMyConversations;

// A tela inicial: conversas do usuário ordenadas por atividade, com prévia e não-lidas.
public record GetMyConversationsQuery(DateTime? Before = null, int Take = 30)
    : IQuery<IReadOnlyList<GetMyConversationsResponse>>;

public sealed record GetMyConversationsResponse(
    Guid Id,
    string Type,
    string Title,
    string? AvatarUrl,
    Guid? OtherUserId,
    DateTime LastActivityAt,
    int UnreadCount,
    ConversationLastMessage? LastMessage);

public sealed record ConversationLastMessage(
    Guid Id,
    string Content,
    string ContentType,
    Guid SenderId,
    DateTime SentAt,
    bool IsDeleted);
