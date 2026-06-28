using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.EditMessage;

public record EditMessageCommand(Guid MessageId, MessageContent Content, Guid RoomId) : ICommand { }

public record MessageContent(string Type, string Data);
