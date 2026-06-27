using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "Usuário não encontrado.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Usuário ou senha inválidos.");
    public static readonly Error UsernameAlreadyTaken = new("User.UsernameAlreadyTaken", "O nome de usuário já está em uso.");
}
