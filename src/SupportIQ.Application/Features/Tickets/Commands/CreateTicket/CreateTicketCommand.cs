using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Commands.CreateTicket;

public record CreateTicketCommand(string Title, string Description, string CustomerEmail) : IRequest<TicketDto>;
