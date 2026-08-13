using Asp.Versioning.Builder;

using BuildingBlocks.Api;

using Chat.Application.UseCases.Messages.DeleteMessage;
using Chat.Application.UseCases.Messages.EditMessage;
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
            .MapGroup("api/v{version:apiVersion}/messages")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization()
            .RequireRateLimiting("chat")
            .WithTags("Messages");

        group.MapPut("{messageId:guid}", async (Guid messageId, EditMessageRequest request, ISender sender) =>
        {
            var result = await sender.Send(new EditMessageCommand(messageId, request.Content));

            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<EditMessageRequest>>();

        group.MapDelete("{messageId:guid}", async (Guid messageId, ISender sender) =>
        {
            var result = await sender.Send(new DeleteMessageCommand(messageId));

            return result.ToHttpResult();
        });

        // Fluxo de mídia: sobe o arquivo aqui, depois manda url + storageKey em SendMessage.
        group.MapPost("upload", async (IFormFile file, ISender sender) =>
        {
            if (file is null || file.Length == 0)
                return ApiResults.Problem(UploadFileErrors.EmptyFile);

            await using var stream = file.OpenReadStream();

            var command = new UploadFileCommand(
                file.FileName,
                file.ContentType,
                stream,
                Path.GetExtension(file.FileName));

            var result = await sender.Send(command);

            return result.ToHttpResult(response => Results.Ok(new
            {
                url = response.FileUrl,
                storageKey = response.StorageKey,
                fileName = response.FileName,
                sizeBytes = response.SizeBytes
            }));
        }).DisableAntiforgery();

        return app;
    }
}
