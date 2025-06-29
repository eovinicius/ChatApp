using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.EditMessage;

public record EditMessageCommand(Guid MessageId, string Content, Guid RoomId) : ICommand { }
