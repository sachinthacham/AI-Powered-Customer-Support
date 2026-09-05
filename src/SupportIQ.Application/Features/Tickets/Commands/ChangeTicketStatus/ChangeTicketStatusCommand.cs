using MediatR;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Enums;

namespace SupportIQ.Application.Features.Tickets.Commands.ChangeTicketStatus;

public record ChangeTicketStatusCommand(Guid TicketId, TicketStatus Status) : IRequest<TicketDto>;
