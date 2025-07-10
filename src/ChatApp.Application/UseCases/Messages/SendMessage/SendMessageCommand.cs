using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

public record SendMessageCommand(Guid RoomId, string Content, string ContentType) : ICommand<Guid>;
