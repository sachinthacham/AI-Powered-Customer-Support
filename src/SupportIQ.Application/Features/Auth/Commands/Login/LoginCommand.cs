using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResultDto>;
