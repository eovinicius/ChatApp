using BuildingBlocks.Messaging;

namespace Identity.Application.UseCases.Users.GetMe;

public record GetMeQuery : IQuery<GetMeResponse>;

public sealed record GetMeResponse(Guid Id, string Name, string Username, string? AvatarUrl, DateTime? LastSeenAt);
