using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Commands.AnalyzeTicket;

public record AnalyzeTicketCommand(Guid TicketId) : IRequest<TicketAnalysisResultDto>;
