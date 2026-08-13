using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.GetConversationById;

public record GetConversationByIdQuery(Guid ConversationId) : IQuery<ConversationDetailsResponse>;

public sealed record ConversationDetailsResponse(
    Guid Id,
    string Type,
    string Title,
    string? AvatarUrl,
    Guid? OwnerId,
    DateTime CreatedAt,
    DateTime LastActivityAt,
    IReadOnlyList<ConversationParticipantResponse> Participants);

public sealed record ConversationParticipantResponse(
    Guid UserId,
    string Name,
    string Username,
    string? AvatarUrl,
    string Role,
    DateTime JoinedAt,
    bool IsMuted);
