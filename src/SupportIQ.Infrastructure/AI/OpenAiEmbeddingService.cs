using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Infrastructure.Configuration;

namespace SupportIQ.Infrastructure.AI;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient _client;
    private readonly AiOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<OpenAiEmbeddingService> _logger;

    public OpenAiEmbeddingService(OpenAIClient client, IOptions<AiOptions> options, ILogger<OpenAiEmbeddingService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
        _resiliencePipeline = AiResiliencePipelineFactory.Create(_options);
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var results = await EmbedBatchAsync(new[] { text }, cancellationToken);
        return results[0];
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new AIServiceException("The AI provider is not configured. Set the Ai:ApiKey environment variable.");

        if (texts.Count == 0)
            return Array.Empty<ReadOnlyMemory<float>>();

        var embeddingClient = _client.GetEmbeddingClient(_options.EmbeddingModel);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
                async ct => await embeddingClient.GenerateEmbeddingsAsync(texts, cancellationToken: ct),
                cancellationToken);

            _logger.LogInformation("Generated {Count} embeddings in {ElapsedMs}ms", texts.Count, stopwatch.ElapsedMilliseconds);

            return result.Value.Select(e => e.ToFloats()).ToList();
        }
        catch (Exception ex) when (ex is ClientResultException or TimeoutRejectedException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Embedding generation failed for {Count} texts after {ElapsedMs}ms", texts.Count, stopwatch.ElapsedMilliseconds);
            throw new AIServiceException("The AI provider failed to generate embeddings. Please try again shortly.", ex);
        }
    }
}
