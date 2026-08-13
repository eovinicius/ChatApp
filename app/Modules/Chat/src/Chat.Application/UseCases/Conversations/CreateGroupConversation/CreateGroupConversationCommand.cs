using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.CreateGroupConversation;

public record CreateGroupConversationCommand(string Name, IReadOnlyCollection<Guid> MemberIds) : ICommand<Guid>;
