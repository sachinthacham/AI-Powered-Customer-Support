using MediatR;

namespace SupportIQ.Application.Features.Tickets.Commands.GenerateResponse;

public record GenerateResponseCommand(Guid TicketId) : IRequest<string>;
