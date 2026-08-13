using BuildingBlocks.Messaging;

namespace Identity.Application.UseCases.Users.Login;

public record LoginCommand(string Username, string Password) : ICommand<string>;
