namespace SupportIQ.Domain.Entities;

/// <summary>
/// Metadata for a company knowledge base document ingested into the RAG pipeline.
/// The document's chunked text and embeddings live in the vector store (Qdrant);
/// this row is the durable, queryable record of what was ingested and when.
/// </summary>
public class KnowledgeDocument
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public int ChunkCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private KnowledgeDocument()
    {
    }

    public static KnowledgeDocument Create(string fileName, string title, string content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        var now = DateTime.UtcNow;
        return new KnowledgeDocument
        {
            Id = Guid.NewGuid(),
            FileName = fileName.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? fileName.Trim() : title.Trim(),
            Content = content,
            ChunkCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void SetChunkCount(int count)
    {
        ChunkCount = count;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Replaces title/content when re-ingesting a document whose file name already exists.</summary>
    public void Update(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Title = string.IsNullOrWhiteSpace(title) ? Title : title.Trim();
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
