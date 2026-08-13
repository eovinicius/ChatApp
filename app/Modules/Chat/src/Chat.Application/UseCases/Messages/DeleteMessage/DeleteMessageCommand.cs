using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Messages.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId) : ICommand;
