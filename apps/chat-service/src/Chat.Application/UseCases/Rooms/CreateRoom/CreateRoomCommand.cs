using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Rooms.CreateRoom;

public record CreateChatroomCommand(string Name, bool IsPrivate, string? Password = null) : ICommand<Guid>;