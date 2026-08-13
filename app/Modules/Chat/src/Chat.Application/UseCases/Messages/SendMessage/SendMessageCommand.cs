using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Messages.SendMessage;

// Para mídia, o cliente faz o upload antes e manda de volta a URL + StorageKey.
public record SendMessageCommand(
    Guid ConversationId,
    string ContentType,
    string Content,
    string? StorageKey = null,
    string? FileName = null,
    long? SizeBytes = null) : ICommand<Guid>;
