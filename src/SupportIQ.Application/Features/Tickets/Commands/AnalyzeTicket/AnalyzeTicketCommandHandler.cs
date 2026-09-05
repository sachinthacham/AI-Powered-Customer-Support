using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Options;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.AnalyzeTicket;

/// <summary>
/// Orchestrates AI ticket analysis: calls the AI abstraction, applies the confidence-based
/// escalation policy (see README "Confidence and Human Escalation"), persists both the
/// ticket's current AI state and an immutable history row, and returns the result.
/// </summary>
public class AnalyzeTicketCommandHandler : IRequestHandler<AnalyzeTicketCommand, TicketAnalysisResultDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAiService _aiService;
    private readonly IApplicationDbContext _context;
    private readonly AiConfidenceOptions _confidenceOptions;
    private readonly ILogger<AnalyzeTicketCommandHandler> _logger;

    public AnalyzeTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketAiService aiService,
        IApplicationDbContext context,
        IOptions<AiConfidenceOptions> confidenceOptions,
        ILogger<AnalyzeTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _aiService = aiService;
        _context = context;
        _confidenceOptions = confidenceOptions.Value;
        _logger = logger;
    }

    public async Task<TicketAnalysisResultDto> Handle(AnalyzeTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.TicketId);

        var result = await _aiService.AnalyzeTicketAsync(ticket, cancellationToken);

        ticket.ApplyAiAnalysis(
            result.Category, result.Priority, result.Sentiment, result.Summary, result.SuggestedResponse,
            result.Tags, result.Confidence);

        var escalated = result.Confidence < _confidenceOptions.ReviewThreshold;
        var needsReview = !escalated && result.Confidence < _confidenceOptions.AcceptThreshold;

        if (escalated)
        {
            ticket.Escalate(
                $"AI analysis confidence ({result.Confidence:P0}) is below the automatic escalation threshold ({_confidenceOptions.ReviewThreshold:P0}).");
        }

        ticket.RecordAnalysis(TicketAnalysis.Create(
            ticket.Id, result.Category, result.Priority, result.Sentiment, result.Summary,
            result.SuggestedResponse, result.Tags, result.Confidence, escalated, result.ModelUsed));

        _context.AuditLogs.Add(AuditLog.Create(
            nameof(SupportTicket),
            ticket.Id.ToString(),
            escalated ? "AiAnalysisEscalated" : needsReview ? "AiAnalysisNeedsReview" : "AiAnalysisApplied",
            "ai",
            $"confidence={result.Confidence:F2}, model={result.ModelUsed}"));

        await _context.SaveChangesAsync(cancellationToken);

        if (needsReview)
        {
            _logger.LogWarning(
                "Ticket {TicketId} analyzed with medium confidence ({Confidence:F2}) - recommended for human review",
                ticket.Id, result.Confidence);
        }

        return new TicketAnalysisResultDto(
            ticket.Id, result.Category.ToString(), result.Priority.ToString(), result.Sentiment.ToString(),
            result.Summary, result.Tags, result.SuggestedResponse, result.Confidence, escalated);
    }
}
