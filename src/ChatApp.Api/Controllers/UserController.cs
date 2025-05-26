using System.Threading.Tasks;

using ChatApp.Application.UseCases.Users.RegisterUser;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        var command = new RegisterUserCommand(request.Name, request.Username, request.Password);

        var result = await _sender.Send(command);

        return CreatedAtAction(
            nameof(Register),
            new { result },
            new { result });
    }
}

public record UserRegisterRequest(string Name, string Username, string Password);