using Asp.Versioning.Builder;

using Chat.Application.UseCases.Users.Login;
using Chat.Application.UseCases.Users.RegisterUser;
using Chat.Presentation.Requests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chat.Presentation.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/User")
            .WithApiVersionSet(versionSet)
            .RequireRateLimiting("auth")
            .WithTags("User");

        group.MapPost("register", async (UserRegisterRequest request, ISender sender) =>
        {
            var result = await sender.Send(new RegisterUserCommand(request.Name, request.Username, request.Password));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Ok(new { token = result.Value });
        }).AddEndpointFilter<ValidationFilter<UserRegisterRequest>>();

        group.MapPost("login", async (UserLoginRequest request, ISender sender) =>
        {
            var result = await sender.Send(new LoginCommand(request.Username, request.Password));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Ok(new { token = result.Value });
        }).AddEndpointFilter<ValidationFilter<UserLoginRequest>>();

        return app;
    }
}
