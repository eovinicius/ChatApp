using BuildingBlocks.Messaging;

namespace Identity.Application.UseCases.Users.SearchUsers;

// Necessário para abrir uma conversa 1x1: sem busca não há como escolher o destinatário.
public record SearchUsersQuery(string Term, int Take = 20) : IQuery<IReadOnlyList<SearchUsersResponse>>;

public sealed record SearchUsersResponse(Guid Id, string Name, string Username, string? AvatarUrl);
