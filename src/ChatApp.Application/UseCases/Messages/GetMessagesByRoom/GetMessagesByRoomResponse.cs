namespace ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

public record GetMessagesByRoomResponse(string Content, string ContentType, Guid SenderId, DateTime SentAt) { }
