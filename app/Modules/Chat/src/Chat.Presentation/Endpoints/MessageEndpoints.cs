using Asp.Versioning.Builder;

using Chat.Application.UseCases.Messages.DeleteMessage;
using Chat.Application.UseCases.Messages.EditMessage;
using Chat.Application.UseCases.Messages.GetMessagesByRoom;
using Chat.Application.UseCases.Messages.SendMessage;
using Chat.Application.UseCases.Messages.UploadFile;
using Chat.Presentation.Requests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chat.Presentation.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/Message")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization()
            .RequireRateLimiting("chat")
            .WithTags("Message");

        group.MapGet("", async (Guid roomId, ISender sender, int take = 20, DateTime? before = null) =>
        {
            var result = await sender.Send(new GetMessagesByRoomQuery(roomId, before, take));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Ok(result.Value);
        });

        group.MapPost("", async (SendMessageRequest request, ISender sender) =>
        {
            var result = await sender.Send(new SendMessageCommand(request.RoomId, request.Content, request.ContentType));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Created($"/api/v1/Message/{result.Value}", new { id = result.Value });
        });

        group.MapPut("{messageId:guid}", async (Guid messageId, EditMessageRequest request, ISender sender) =>
        {
            var result = await sender.Send(new EditMessageCommand(messageId, new MessageContent("text", request.Content), request.RoomId));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.NoContent();
        });

        group.MapDelete("{messageId:guid}", async (Guid messageId, Guid roomId, ISender sender) =>
        {
            var result = await sender.Send(new DeleteMessageCommand(messageId, roomId));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.NoContent();
        });

        group.MapPost("upload", async (IFormFile file, ISender sender) =>
        {
            if (file is null || file.Length == 0)
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "UploadFile.EmptyFile",
                    detail: "Nenhum arquivo enviado.");

            var extension = Path.GetExtension(file.FileName);
            var result = await sender.Send(new UploadFileCommand(file.FileName, file.ContentType, file.OpenReadStream(), extension));

            return result.IsFailure
                ? ApiResults.Problem(result.Error)
                : Results.Ok(new { url = result.Value.FileUrl });
        }).DisableAntiforgery();

        return app;
    }
}
