using Chat.Domain.Conversations;

namespace Chat.Application.UseCases.Conversations.GetMyConversations;

// Linha crua vinda do Dapper. Traz OtherUserId em vez de nome — a hidratação
// acontece no handler, via Identity.Contracts.
public sealed record ConversationListItem(
    Guid Id,
    ConversationType Type,
    string? GroupName,
    string? GroupAvatarUrl,
    DateTime LastActivityAt,
    Guid? OtherUserId,
    Guid? LastMessageId,
    string? LastMessagePreview,
    string? LastMessageContentType,
    Guid? LastMessageSenderId,
    DateTime? LastMessageSentAt,
    bool LastMessageDeleted,
    int UnreadCount);
