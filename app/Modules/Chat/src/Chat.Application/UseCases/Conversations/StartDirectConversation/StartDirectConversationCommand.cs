using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.StartDirectConversation;

public record StartDirectConversationCommand(Guid TargetUserId) : ICommand<Guid>;
