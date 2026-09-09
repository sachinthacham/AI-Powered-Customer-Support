namespace SupportIQ.Infrastructure.Configuration;

/// <summary>Configuration for the Qdrant vector store. Bound from the "Qdrant" section.</summary>
public class QdrantOptions
{
    public const string SectionName = "Qdrant";

    /// <summary>Qdrant gRPC host, e.g. "localhost".</summary>
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 6334;

    public bool UseHttps { get; set; } = false;

    /// <summary>Required only when Qdrant Cloud/auth is enabled; empty for local Docker.</summary>
    public string? ApiKey { get; set; }

    public string CollectionName { get; set; } = "supportiq-knowledge";

    /// <summary>Must match the embedding model's output dimensionality (1536 for text-embedding-3-small).</summary>
    public int VectorSize { get; set; } = 1536;
}
