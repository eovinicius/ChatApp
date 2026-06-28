using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Rooms.CreateRoom;

public record CreateChatroomCommand(string Name, bool IsPrivate, string? Password = null) : ICommand<Guid>;