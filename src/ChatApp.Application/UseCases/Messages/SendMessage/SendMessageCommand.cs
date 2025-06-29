using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

public record SendMessageCommand(Guid RoomId, string Message) : ICommand<Guid>;
