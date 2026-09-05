namespace SupportIQ.Application.DTOs;

public record RagSourceDto(string Document, int Chunk, double Relevance);

public record RagAnswerDto(string Answer, double Confidence, IReadOnlyList<RagSourceDto> Sources);
