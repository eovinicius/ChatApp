using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId, Guid RoomId) : ICommand { }