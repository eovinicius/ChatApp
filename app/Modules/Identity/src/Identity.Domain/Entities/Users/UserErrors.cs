using SharedKernel;

namespace Identity.Domain.Entities.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "Usuário não encontrado.", ErrorType.NotFound);
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Usuário ou senha inválidos.", ErrorType.Unauthorized);
    public static readonly Error UsernameAlreadyTaken = new("User.UsernameAlreadyTaken", "O nome de usuário já está em uso.", ErrorType.Conflict);
    public static readonly Error EmptyName = new("User.EmptyName", "O nome do usuário não pode ser vazio.", ErrorType.Validation);
    public static readonly Error EmptyUsername = new("User.EmptyUsername", "O nome de usuário não pode ser vazio.", ErrorType.Validation);
    public static readonly Error EmptyPassword = new("User.EmptyPassword", "A senha não pode ser vazia.", ErrorType.Validation);
}
