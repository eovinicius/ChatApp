using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Messages.SendMessage;

public record SendMessageCommand(
    Guid RoomId,
    string Content,
    string ContentType) : ICommand<Guid>;
