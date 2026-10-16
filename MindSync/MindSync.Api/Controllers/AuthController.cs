using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindSync.Api.Common;
using MindSync.Application.Features.Users.Commands.LoginUser;
using MindSync.Application.Features.Users.Commands.RegisterUser;
using System.Security.Claims;

namespace MindSync.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
    IMediator mediator,
    IHostEnvironment environment) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommandRequest command)
    {
        var userId = await mediator.Send(command);
        var response = ApiResponse<Guid>.Ok(userId, "Cadastro realizado com sucesso.");

        return CreatedAtAction(nameof(Register), new { id = userId }, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommandRequest command)
    {
        var result = await mediator.Send(command);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(2)
        };

        Response.Cookies.Append("MindSync.Auth", result.Token, cookieOptions);

        return Ok(ApiResponse<string>.Ok(result.UserId, "Login realizado com sucesso."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(ApiResponse<string>.Ok(userId));
    }
}