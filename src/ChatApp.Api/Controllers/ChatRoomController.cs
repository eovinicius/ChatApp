using ChatApp.Application.UseCases.Rooms.CreateRoom;
using ChatApp.Domain.Abstractions;

using MediatR;

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

    public sealed class CreateChatRoomRequest
    {
        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
    }

}