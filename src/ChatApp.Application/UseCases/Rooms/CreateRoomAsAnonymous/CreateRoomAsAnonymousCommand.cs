using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Rooms.CreateRoomAsAnonymous;

public record CreateRoomAsAnonymousCommand(string RoomName, string GuestName) : ICommand<Guid>;