using SupportIQ.Application.AI;

namespace SupportIQ.Application.Abstractions;

/// <summary>
/// Orchestrates the retrieval-augmented generation pipeline: embed the question, search the
/// vector store, and (if enough relevant context was found) ask the LLM to answer using only
/// that context. See README "RAG Pipeline" for the full flow diagram.
/// </summary>
public interface IRagService
{
    Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default);
}
