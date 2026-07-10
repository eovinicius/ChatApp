using Asp.Versioning.Builder;

using Chat.Application.UseCases.Rooms.CreateRoom;
using Chat.Application.UseCases.Rooms.JoinRoom;
using Chat.Application.UseCases.Rooms.LeaveRoom;
using Chat.Presentation.Requests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chat.Presentation.Endpoints;

public static class ChatRoomEndpoints
{
    public static IEndpointRouteBuilder MapChatRoomEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/ChatRoom")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization()
            .RequireRateLimiting("chat")
            .WithTags("ChatRoom");

        group.MapPost("", async (CreateChatRoomRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateChatroomCommand(request.RoomName, request.IsPrivate, request.Password));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Created($"/api/v1/ChatRoom/{result.Value}", new { id = result.Value });
        });

        group.MapPost("{roomId:guid}/join", async (Guid roomId, JoinRoomRequest? request, ISender sender) =>
        {
            var result = await sender.Send(new JoinRoomCommand(roomId, request?.Password));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Ok();
        });

        group.MapDelete("{roomId:guid}/leave", async (Guid roomId, ISender sender) =>
        {
            var result = await sender.Send(new LeaveRoomCommand(roomId));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.NoContent();
        });

        return app;
    }
}
