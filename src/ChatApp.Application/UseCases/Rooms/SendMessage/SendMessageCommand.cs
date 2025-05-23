using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Rooms.SendMessage;

public record SendMessageCommand(Guid RoomId, string Message) : ICommand<Guid>;
