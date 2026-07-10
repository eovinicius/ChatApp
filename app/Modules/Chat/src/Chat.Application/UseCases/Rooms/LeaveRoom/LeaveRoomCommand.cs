using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Rooms.LeaveRoom;

public record LeaveRoomCommand(Guid RoomId) : ICommand;