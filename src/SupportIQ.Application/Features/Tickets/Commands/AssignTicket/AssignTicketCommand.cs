using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Commands.AssignTicket;

public record AssignTicketCommand(Guid TicketId, Guid AgentId) : IRequest<TicketDto>;
