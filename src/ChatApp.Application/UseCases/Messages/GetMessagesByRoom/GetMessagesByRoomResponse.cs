namespace ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

public record GetMessagesByRoomResponse(string Content, Guid SenderId, DateTime SentAt) { }
