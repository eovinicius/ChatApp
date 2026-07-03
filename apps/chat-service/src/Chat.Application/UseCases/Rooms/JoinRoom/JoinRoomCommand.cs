using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Rooms.JoinRoom;

public record JoinRoomCommand(Guid RoomId, string? Password = null) : ICommand;