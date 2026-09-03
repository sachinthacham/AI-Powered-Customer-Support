using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportIQ.Application.DTOs;
using SupportIQ.Application.Features.Auth.Commands.Login;

namespace SupportIQ.API.Controllers;

/// <summary>Agent authentication. Issues the JWT bearer token used by every other endpoint.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Authenticates a support agent and returns a JWT bearer token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
