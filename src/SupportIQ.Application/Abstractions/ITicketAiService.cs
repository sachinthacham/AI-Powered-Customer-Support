using SupportIQ.Application.AI;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Abstractions;

/// <summary>
/// The Application layer's only door to AI-powered ticket analysis. Controllers and handlers
/// depend on this interface, never on OpenAI (or any other provider) directly - see
/// Infrastructure/AI/OpenAiTicketAiService for the concrete implementation.
/// </summary>
public interface ITicketAiService
{
    /// <summary>
    /// Classifies, prioritizes, and summarizes a ticket via structured AI output. Throws
    /// <see cref="Common.Exceptions.AIServiceException"/> if the provider fails or returns
    /// output that fails validation - callers should not have to guess at malformed data.
    /// </summary>
    Task<TicketAnalysisResult> AnalyzeTicketAsync(SupportTicket ticket, CancellationToken cancellationToken = default);

    /// <summary>Drafts a customer-facing reply for a ticket, independent of a full re-analysis.</summary>
    Task<string> GenerateResponseAsync(SupportTicket ticket, CancellationToken cancellationToken = default);
}
