namespace SupportIQ.Application.AI;

/// <summary>One retrieved chunk that was used (or considered) when grounding a RAG answer.</summary>
public record RagSource(string Document, int Chunk, double Relevance);

/// <summary>
/// The result of a RAG question. <see cref="Confidence"/> is derived from vector-similarity
/// scores of the retrieved chunks, not from the LLM self-reporting - see README "Confidence and
/// Human Escalation" for why we treat model self-assessment as unreliable.
/// </summary>
public record RagAnswer(string Answer, double Confidence, IReadOnlyList<RagSource> Sources);
