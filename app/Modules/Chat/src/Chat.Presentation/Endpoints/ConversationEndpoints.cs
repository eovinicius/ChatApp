using Asp.Versioning.Builder;

using BuildingBlocks.Api;

using Chat.Application.UseCases.Conversations.CreateGroupConversation;
using Chat.Application.UseCases.Conversations.GetConversationById;
using Chat.Application.UseCases.Conversations.GetMyConversations;
using Chat.Application.UseCases.Conversations.ManageParticipants;
using Chat.Application.UseCases.Conversations.MarkAsRead;
using Chat.Application.UseCases.Conversations.StartDirectConversation;
using Chat.Application.UseCases.Messages.GetMessages;
using Chat.Application.UseCases.Messages.SendMessage;
using Chat.Presentation.Requests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chat.Presentation.Endpoints;

public static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app
            .MapGroup("api/v{version:apiVersion}/conversations")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization()
            .RequireRateLimiting("chat")
            .WithTags("Conversations");

        // Idempotente: chamar duas vezes com o mesmo alvo devolve a mesma conversa.
        group.MapPost("direct", async (StartDirectConversationRequest request, ISender sender, LinkGenerator links, HttpContext http) =>
        {
            var result = await sender.Send(new StartDirectConversationCommand(request.TargetUserId));

            return result.ToCreatedResult(id => ConversationPath(links, http, id), id => new { id });
        }).AddEndpointFilter<ValidationFilter<StartDirectConversationRequest>>();

        group.MapPost("group", async (CreateGroupConversationRequest request, ISender sender, LinkGenerator links, HttpContext http) =>
        {
            var result = await sender.Send(new CreateGroupConversationCommand(request.Name, request.MemberIds));

            return result.ToCreatedResult(id => ConversationPath(links, http, id), id => new { id });
        }).AddEndpointFilter<ValidationFilter<CreateGroupConversationRequest>>();

        // A tela inicial.
        group.MapGet("", async (DateTime? before, int? take, ISender sender) =>
        {
            var result = await sender.Send(new GetMyConversationsQuery(before, take ?? 30));

            return result.ToPagedResult();
        });

        group.MapGet("{conversationId:guid}", async (Guid conversationId, ISender sender) =>
        {
            var result = await sender.Send(new GetConversationByIdQuery(conversationId));

            return result.ToHttpResult();
        });

        group.MapPatch("{conversationId:guid}", async (Guid conversationId, RenameConversationRequest request, ISender sender) =>
        {
            var result = await sender.Send(new RenameConversationCommand(conversationId, request.Name));

            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<RenameConversationRequest>>();

        group.MapPost("{conversationId:guid}/participants", async (Guid conversationId, AddParticipantsRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AddParticipantsCommand(conversationId, request.UserIds));

            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<AddParticipantsRequest>>();

        group.MapDelete("{conversationId:guid}/participants/me", async (Guid conversationId, ISender sender) =>
        {
            var result = await sender.Send(new LeaveConversationCommand(conversationId));

            return result.ToHttpResult();
        });

        group.MapDelete("{conversationId:guid}/participants/{userId:guid}", async (Guid conversationId, Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveParticipantCommand(conversationId, userId));

            return result.ToHttpResult();
        });

        group.MapPost("{conversationId:guid}/participants/{userId:guid}/promote", async (Guid conversationId, Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new PromoteParticipantCommand(conversationId, userId));

            return result.ToHttpResult();
        });

        group.MapPost("{conversationId:guid}/participants/{userId:guid}/demote", async (Guid conversationId, Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new DemoteParticipantCommand(conversationId, userId));

            return result.ToHttpResult();
        });

        // Zera as não-lidas e dispara o recibo de leitura para os outros.
        group.MapPost("{conversationId:guid}/read", async (Guid conversationId, MarkAsReadRequest request, ISender sender) =>
        {
            var result = await sender.Send(new MarkConversationAsReadCommand(conversationId, request.LastMessageId));

            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<MarkAsReadRequest>>();

        group.MapGet("{conversationId:guid}/messages", async (Guid conversationId, DateTime? before, int? take, ISender sender) =>
        {
            var result = await sender.Send(new GetMessagesQuery(conversationId, before, take ?? 30));

            return result.ToPagedResult();
        });

        group.MapPost("{conversationId:guid}/messages", async (Guid conversationId, SendMessageRequest request, ISender sender) =>
        {
            var command = new SendMessageCommand(
                conversationId,
                request.ContentType,
                request.Content,
                request.StorageKey,
                request.FileName,
                request.SizeBytes);

            var result = await sender.Send(command);

            // 201 sem Location: não existe rota GET para uma mensagem isolada.
            return result.ToCreatedResult(id => new { id });
        }).AddEndpointFilter<ValidationFilter<SendMessageRequest>>();

        return app;
    }

    // Evita o "/api/v1/" hardcoded que quebraria assim que existisse uma v2.
    private static string ConversationPath(LinkGenerator links, HttpContext http, Guid id)
        => links.GetPathByRouteValues(http, routeName: null, values: new { conversationId = id })
           ?? $"{http.Request.Path}/{id}";
}
