using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Users.RegisterUser;

public record RegisterUserCommand(string Name, string Username, string Password) : ICommand<string?>;
