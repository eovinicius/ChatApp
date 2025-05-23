using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Rooms.JoinRoom;

public record JoinRoomCommand(Guid RoomId, string? Password = null) : ICommand;