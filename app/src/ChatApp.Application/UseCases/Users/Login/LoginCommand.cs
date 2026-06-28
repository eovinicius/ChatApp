using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Users.Login;

public record LoginCommand(string Username, string Password) : ICommand<string>;
