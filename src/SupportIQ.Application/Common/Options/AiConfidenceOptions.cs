namespace SupportIQ.Application.Common.Options;

/// <summary>
/// Application-level thresholds for how much we trust an AI ticket analysis result.
/// These are NOT a statistically calibrated confidence interval - they are a practical,
/// tunable policy: "below what score do we make a human look at this before it goes further".
/// See README "Confidence and Human Escalation" for the rationale.
/// </summary>
public class AiConfidenceOptions
{
    public const string SectionName = "AiConfidence";

    /// <summary>At or above this score, the AI result is accepted with no extra flag.</summary>
    public double AcceptThreshold { get; set; } = 0.85;

    /// <summary>
    /// At or above this score (but below <see cref="AcceptThreshold"/>), the AI result is
    /// accepted but the ticket is left for a human to double-check.
    /// Below this score, the ticket is automatically escalated.
    /// </summary>
    public double ReviewThreshold { get; set; } = 0.70;
}
