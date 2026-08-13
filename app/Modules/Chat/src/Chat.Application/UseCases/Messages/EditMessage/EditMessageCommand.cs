using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Messages.EditMessage;

public record EditMessageCommand(Guid MessageId, string Content) : ICommand;
