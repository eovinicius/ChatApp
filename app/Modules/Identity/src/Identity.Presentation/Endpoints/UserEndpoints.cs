using Asp.Versioning.Builder;

using BuildingBlocks.Api;

using Identity.Application.UseCases.Users.GetMe;
using Identity.Application.UseCases.Users.SearchUsers;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Presentation.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization()
            .RequireRateLimiting("chat")
            .WithTags("Users");

        // Ponto de partida de uma conversa 1x1: achar com quem falar.
        group.MapGet("", async (string? search, int? take, ISender sender) =>
        {
            var result = await sender.Send(new SearchUsersQuery(search ?? string.Empty, take ?? 20));

            return result.ToHttpResult();
        });

        group.MapGet("me", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMeQuery());

            return result.ToHttpResult();
        });

        return app;
    }
}
