using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Users.RegisterUser;

public record RegisterUserCommand(string Name, string Username, string Password) : ICommand<string?>;
