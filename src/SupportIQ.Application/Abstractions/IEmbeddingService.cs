namespace SupportIQ.Application.Abstractions;

/// <summary>Converts text into vector embeddings for semantic search. Implemented with OpenAI embeddings in Infrastructure.</summary>
public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Embeds many texts in one provider call where possible - cheaper than embedding one at a time during document ingestion.</summary>
    Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
