using ChatApp.Application.UseCases.Rooms.CreateRoom;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatRoomController : ControllerBase
{
    private readonly ISender _sender;
    public ChatRoomController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [EnableRateLimiting("default")]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomRequest request)
    {
        var result = await _sender.Send(new CreateChatroomCommand(request.RoomName, request.IsPrivate, request.Password));
        return CreatedAtAction(nameof(CreateChatRoom), new { result }, result);
    }

    public sealed class CreateChatRoomRequest
    {
        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
    }

}