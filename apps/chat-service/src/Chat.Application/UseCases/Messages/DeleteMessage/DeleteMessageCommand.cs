using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Messages.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId, Guid RoomId) : ICommand { }