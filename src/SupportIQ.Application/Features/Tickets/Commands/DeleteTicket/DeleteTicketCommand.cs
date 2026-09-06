using MediatR;

namespace SupportIQ.Application.Features.Tickets.Commands.DeleteTicket;

public record DeleteTicketCommand(Guid Id) : IRequest;
