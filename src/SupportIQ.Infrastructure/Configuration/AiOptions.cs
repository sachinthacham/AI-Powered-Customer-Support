namespace SupportIQ.Infrastructure.Configuration;

/// <summary>Configuration for the chat/completion AI provider. Bound from the "Ai" section.</summary>
public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Informational only today ("OpenAI") - kept so a second provider can be added later.</summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>Read from configuration/environment variables only - never hardcoded, never logged.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Chat/completion model, e.g. "gpt-4o-mini".</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Embedding model, e.g. "text-embedding-3-small".</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>Optional override for OpenAI-compatible endpoints (e.g. Azure OpenAI, local proxies).</summary>
    public string? BaseUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;
}
