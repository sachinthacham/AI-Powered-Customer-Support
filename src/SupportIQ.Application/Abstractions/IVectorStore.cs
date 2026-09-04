namespace SupportIQ.Application.Abstractions;

public record VectorChunk(int ChunkIndex, string Text, ReadOnlyMemory<float> Embedding);

public record VectorSearchResult(Guid DocumentId, string DocumentTitle, int ChunkIndex, string Text, float Score);

/// <summary>
/// Abstraction over the vector database (Qdrant) used for semantic search. Kept separate from
/// <see cref="IEmbeddingService"/> because "turn text into a vector" and "store/search vectors"
/// are genuinely different concerns with different failure modes and different providers.
/// </summary>
public interface IVectorStore
{
    /// <summary>Creates the collection if it doesn't already exist. Safe to call on every startup.</summary>
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces all chunks for a document (deletes any existing ones for that id first).</summary>
    Task UpsertChunksAsync(Guid documentId, string documentTitle, IReadOnlyList<VectorChunk> chunks, CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK, CancellationToken cancellationToken = default);
}
