using System.ComponentModel.DataAnnotations;

using ChatApp.Application.UseCases.Users.RegisterUser;
using ChatApp.Domain.Abstractions;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.Name,
            request.Username,
            request.Password);

        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Name });
        }

        return Ok(new { token = result.Value });
    }
}

public sealed record UserRegisterRequest(
    [Required(ErrorMessage = "O nome é obrigatório")]
    string Name,

    [Required(ErrorMessage = "O username é obrigatório")]
    [MinLength(3, ErrorMessage = "O username deve ter no mínimo 3 caracteres")]
    string Username,

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    string Password);