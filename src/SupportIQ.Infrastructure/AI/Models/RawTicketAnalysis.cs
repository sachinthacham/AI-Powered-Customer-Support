using System.Text.Json.Serialization;

namespace SupportIQ.Infrastructure.AI.Models;

/// <summary>
/// The raw shape returned by the AI provider's structured JSON output, before it has been
/// validated and converted into the strongly-typed <see cref="Application.AI.TicketAnalysisResult"/>.
/// Field names must match the JSON schema sent to the provider exactly.
/// </summary>
internal class RawTicketAnalysis
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("suggestedResponse")]
    public string SuggestedResponse { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
