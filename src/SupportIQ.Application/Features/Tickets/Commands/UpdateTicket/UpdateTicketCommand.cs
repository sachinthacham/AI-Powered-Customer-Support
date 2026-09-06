using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Commands.UpdateTicket;

public record UpdateTicketCommand(Guid Id, string? Title, string? Description) : IRequest<TicketDto>;
