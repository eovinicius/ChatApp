using System.Threading.Tasks;

using ChatApp.Application.UseCases.Users.RegisterUser;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserControllers : ControllerBase
{
    private readonly ISender _sender;

    public UserControllers(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        var command = new RegisterUserCommand(request.Name, request.Username, request.Password);

        var id = await _sender.Send(command);

        return CreatedAtAction(
            nameof(Register),
            new { id },
            new { id });
    }
}

public record UserRegisterRequest(string Name, string Username, string Password);