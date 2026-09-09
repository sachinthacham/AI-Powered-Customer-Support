using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.AI;
using SupportIQ.Application.AI.Prompts;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Options;
using SupportIQ.Infrastructure.Configuration;

namespace SupportIQ.Infrastructure.AI;

/// <summary>
/// Orchestrates the RAG pipeline: embed the question, search Qdrant, and - only if the results
/// clear the relevance bar - ask the LLM to answer using just those chunks. Confidence is
/// derived from the retrieval scores rather than asked of the model (see <see cref="RagAnswer"/>).
/// </summary>
public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly OpenAIClient _client;
    private readonly AiOptions _aiOptions;
    private readonly RagOptions _ragOptions;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<RagService> _logger;

    private const string InsufficientContextAnswer =
        "I don't have enough information in the knowledge base to answer this confidently.";

    public RagService(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        OpenAIClient client,
        IOptions<AiOptions> aiOptions,
        IOptions<RagOptions> ragOptions,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _client = client;
        _aiOptions = aiOptions.Value;
        _ragOptions = ragOptions.Value;
        _resiliencePipeline = AiResiliencePipelineFactory.Create(_aiOptions);
        _logger = logger;
    }

    public async Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var queryEmbedding = await _embeddingService.EmbedAsync(question, cancellationToken);
        var searchResults = await _vectorStore.SearchAsync(queryEmbedding, _ragOptions.TopK, cancellationToken);

        var relevantChunks = searchResults.Where(r => r.Score >= _ragOptions.MinRelevanceScore).ToList();

        _logger.LogInformation(
            "RAG search returned {TotalCount} chunks, {RelevantCount} above relevance threshold, in {ElapsedMs}ms",
            searchResults.Count, relevantChunks.Count, stopwatch.ElapsedMilliseconds);

        if (relevantChunks.Count == 0)
        {
            return new RagAnswer(InsufficientContextAnswer, 0.0, Array.Empty<RagSource>());
        }

        if (string.IsNullOrWhiteSpace(_aiOptions.ApiKey))
            throw new AIServiceException("The AI provider is not configured. Set the Ai:ApiKey environment variable.");

        var chatClient = _client.GetChatClient(_aiOptions.Model);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(GroundedAnswerPrompt.SystemPrompt),
            new UserChatMessage(GroundedAnswerPrompt.BuildUserPrompt(question, relevantChunks))
        };

        ChatCompletion completion;
        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
                async ct => await chatClient.CompleteChatAsync(messages, cancellationToken: ct),
                cancellationToken);
            completion = result.Value;
        }
        catch (Exception ex) when (ex is ClientResultException or TimeoutRejectedException or BrokenCircuitException)
        {
            _logger.LogError(ex, "RAG grounded generation failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw new AIServiceException("The AI provider failed to answer this question. Please try again shortly.", ex);
        }

        var answerText = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(answerText))
            throw new AIServiceException("The AI provider returned an empty answer.");

        _logger.LogInformation("RAG question answered in {ElapsedMs}ms using {ChunkCount} chunks",
            stopwatch.ElapsedMilliseconds, relevantChunks.Count);

        var confidence = relevantChunks.Max(r => r.Score);
        var sources = relevantChunks
            .Select(r => new RagSource(r.DocumentTitle, r.ChunkIndex, r.Score))
            .ToList();

        return new RagAnswer(answerText.Trim(), confidence, sources);
    }
}
