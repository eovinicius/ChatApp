using ChatApp.Application.UseCases.Rooms.CreateRoom;
using ChatApp.Application.UseCases.Rooms.CreateRoomAsAnonymous;
using ChatApp.Application.UseCases.Rooms.JoinRoom;
using ChatApp.Application.UseCases.Rooms.LeaveRoom;
using ChatApp.Domain.Abstractions;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("default")]
public class ChatRoomController : ControllerBase
{
    private readonly ISender _sender;
    public ChatRoomController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomRequest request)
    {
        var result = await _sender.Send(new CreateChatroomCommand(request.RoomName, request.IsPrivate, request.Password));
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Name });
        }

        return CreatedAtAction(
            nameof(CreateChatRoom),
            new { id = result.Value },
            new { id = result.Value });
    }

    [HttpPost("anonymous")]
    public async Task<IActionResult> CreateChatRoomAsAnonymous([FromBody] CreateChatRoomAsAnonymousRequest request)
    {
        var result = await _sender.Send(new CreateRoomAsAnonymousCommand(request.RoomName, request.GuestName));

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Name });
        }

        return CreatedAtAction(
            nameof(CreateChatRoomAsAnonymous),
            new { id = result.Value },
            new { id = result.Value });
    }

    [Authorize]
    [HttpPost("{roomId:guid}/join")]
    public async Task<IActionResult> JoinRoom(Guid roomId, [FromBody] JoinRoomRequest? request = null)
    {
        var result = await _sender.Send(new JoinRoomCommand(roomId, request?.Password));

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Name });

        return Ok();
    }

    [Authorize]
    [HttpDelete("{roomId:guid}/leave")]
    public async Task<IActionResult> LeaveRoom(Guid roomId)
    {
        var result = await _sender.Send(new LeaveRoomCommand(roomId));

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Name });

        return NoContent();
    }

    public sealed class CreateChatRoomRequest
    {
        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
    }

    public sealed class CreateChatRoomAsAnonymousRequest
    {
        public string RoomName { get; set; }
        public string GuestName { get; set; }
    }

    public sealed class JoinRoomRequest
    {
        public string? Password { get; set; }
    }

}