using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Conversations.ManageParticipants;

public record AddParticipantsCommand(Guid ConversationId, IReadOnlyCollection<Guid> UserIds) : ICommand;

public record RemoveParticipantCommand(Guid ConversationId, Guid UserId) : ICommand;

public record LeaveConversationCommand(Guid ConversationId) : ICommand;

public record PromoteParticipantCommand(Guid ConversationId, Guid UserId) : ICommand;

public record DemoteParticipantCommand(Guid ConversationId, Guid UserId) : ICommand;

public record RenameConversationCommand(Guid ConversationId, string Name) : ICommand;
