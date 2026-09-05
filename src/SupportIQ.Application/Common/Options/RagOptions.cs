namespace SupportIQ.Application.Common.Options;

/// <summary>Tuning knobs for the retrieval-augmented generation pipeline. Bound from the "Rag" section.</summary>
public class RagOptions
{
    public const string SectionName = "Rag";

    /// <summary>How many chunks to retrieve from the vector store per question.</summary>
    public int TopK { get; set; } = 4;

    /// <summary>
    /// Chunks scoring below this cosine-similarity threshold are treated as "not relevant enough"
    /// and excluded before the LLM ever sees them. If none clear the bar, we skip the LLM call
    /// entirely and return the low-confidence fallback answer.
    /// </summary>
    public double MinRelevanceScore { get; set; } = 0.70;

    /// <summary>Target size of each document chunk, in characters.</summary>
    public int ChunkSize { get; set; } = 800;

    /// <summary>How many characters of overlap between consecutive chunks, to avoid splitting a fact across a boundary.</summary>
    public int ChunkOverlap { get; set; } = 100;
}
