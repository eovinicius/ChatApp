using BuildingBlocks.Messaging;

namespace Identity.Application.UseCases.Users.RegisterUser;

public record RegisterUserCommand(string Name, string Username, string Password) : ICommand<string?>;
