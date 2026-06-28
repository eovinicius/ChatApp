using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Rooms.LeaveRoom;

public record LeaveRoomCommand(Guid RoomId) : ICommand;