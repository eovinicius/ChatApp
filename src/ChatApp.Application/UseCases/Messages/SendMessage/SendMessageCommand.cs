using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

public record SendMessageCommand(Guid RoomId, MessageContent Content) : ICommand<Guid>;

public record MessageContent(string Type, string Data);
