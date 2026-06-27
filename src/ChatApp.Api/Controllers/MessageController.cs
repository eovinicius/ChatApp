using ChatApp.Application.UseCases.Messages.DeleteMessage;
using ChatApp.Application.UseCases.Messages.EditMessage;
using ChatApp.Application.UseCases.Messages.GetMessagesByRoom;
using ChatApp.Application.UseCases.Messages.SendMessage;
using ChatApp.Application.UseCases.Messages.UploadFile;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("chat")]
public sealed class MessageController : ControllerBase
{
    private readonly ISender _sender;

    public MessageController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] Guid roomId, [FromQuery] int take = 20, [FromQuery] DateTime? before = null)
    {
        var result = await _sender.Send(new GetMessagesByRoomQuery(roomId, before, take));

        if (result.IsFailure)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Name);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var result = await _sender.Send(new SendMessageCommand(request.RoomId, request.Content, request.ContentType));

        if (result.IsFailure)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Name);

        return CreatedAtAction(nameof(SendMessage), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{messageId:guid}")]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageRequest request)
    {
        var result = await _sender.Send(new EditMessageCommand(messageId, new MessageContent("text", request.Content), request.RoomId));

        if (result.IsFailure)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Name);

        return NoContent();
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, [FromQuery] Guid roomId)
    {
        var result = await _sender.Send(new DeleteMessageCommand(messageId, roomId));

        if (result.IsFailure)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Name);

        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "UploadFile.EmptyFile", detail: "Nenhum arquivo enviado.");

        var extension = Path.GetExtension(file.FileName);
        var result = await _sender.Send(new UploadFileCommand(file.FileName, file.ContentType, file.OpenReadStream(), extension));

        if (result.IsFailure)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Name);

        return Ok(new { url = result.Value.FileUrl });
    }

    public sealed class SendMessageRequest
    {
        public Guid RoomId { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public sealed class EditMessageRequest
    {
        public Guid RoomId { get; set; }
        public string Content { get; set; }
    }
}
