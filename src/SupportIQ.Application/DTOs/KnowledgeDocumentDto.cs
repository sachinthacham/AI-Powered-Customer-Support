namespace SupportIQ.Application.DTOs;

public record KnowledgeDocumentDto(Guid Id, string FileName, string Title, int ChunkCount, DateTime CreatedAt, DateTime UpdatedAt);
