using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Commands.EscalateTicket;

public record EscalateTicketCommand(Guid TicketId, string Reason) : IRequest<TicketDto>;
