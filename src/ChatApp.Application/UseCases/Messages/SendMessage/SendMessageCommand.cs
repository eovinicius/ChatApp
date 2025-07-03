using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Entities.Messages;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

public record SendMessageCommand(Guid RoomId, MessageContent Content) : ICommand<Guid>;

public record MessageContent(ContentType ContentType, string Data);
