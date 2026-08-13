using Asp.Versioning.Builder;

using BuildingBlocks.Api;

using Identity.Application.UseCases.Users.Login;
using Identity.Application.UseCases.Users.RegisterUser;
using Identity.Presentation.Requests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .RequireRateLimiting("auth")
            .WithTags("Auth");

        group.MapPost("register", async (UserRegisterRequest request, ISender sender) =>
        {
            var result = await sender.Send(new RegisterUserCommand(request.Name, request.Username, request.Password));

            return result.ToHttpResult(token => Results.Ok(new { token }));
        }).AddEndpointFilter<ValidationFilter<UserRegisterRequest>>();

        group.MapPost("login", async (UserLoginRequest request, ISender sender) =>
        {
            var result = await sender.Send(new LoginCommand(request.Username, request.Password));

            return result.ToHttpResult(token => Results.Ok(new { token }));
        }).AddEndpointFilter<ValidationFilter<UserLoginRequest>>();

        return app;
    }
}
