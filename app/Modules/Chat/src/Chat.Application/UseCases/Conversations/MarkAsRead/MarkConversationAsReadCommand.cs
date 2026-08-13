using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.MarkAsRead;

public record MarkConversationAsReadCommand(Guid ConversationId, Guid LastMessageId) : ICommand;
