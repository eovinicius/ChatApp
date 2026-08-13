namespace BuildingBlocks.Authentication;

// Compartilhado entre os módulos: quem é o usuário autenticado da requisição atual.
// TryGetUserId existe para contextos onde a ausência de usuário é esperada (ex.: hub SignalR).
public interface IUserContext
{
    Guid UserId { get; }

    bool TryGetUserId(out Guid userId);
}
