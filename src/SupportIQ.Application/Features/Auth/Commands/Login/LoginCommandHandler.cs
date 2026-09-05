using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == normalizedEmail, cancellationToken);

        if (agent is null || !_passwordHasher.Verify(request.Password, agent.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt");
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _tokenService.GenerateToken(agent);

        _logger.LogInformation("Agent {AgentId} logged in", agent.Id);

        return new LoginResultDto(token.Value, token.ExpiresAtUtc, agent.Email, agent.Name, agent.Role.ToString());
    }
}
