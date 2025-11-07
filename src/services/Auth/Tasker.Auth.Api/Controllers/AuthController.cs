using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tasker.Auth.Application.Users.Commands.LoginUser;
using Tasker.Auth.Application.Users.Commands.RegisterUser;

namespace Tasker.Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    private readonly IConfiguration _cfg;

    public AuthController(IMediator mediator, IConfiguration cfg)
    {
        _mediator = mediator;
        _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResult>> Register([FromBody] RegisterUserCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return Created($"/users/{result.UserId}", result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResult>> Login([FromBody] LoginUserCommandBody body, CancellationToken ct)
    {
        var ttlMin = _cfg.GetValue<int?>("Auth:SessionTtlMinutes") ?? 60 * 24 * 7;
        var cmd = new LoginUserCommand(body.Email, body.Password, TimeSpan.FromMinutes(ttlMin));
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    public sealed record LoginUserCommandBody(string Email, string Password);
}