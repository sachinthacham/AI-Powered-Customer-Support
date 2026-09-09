using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
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
using SupportIQ.Domain.Entities;
using SupportIQ.Domain.Enums;
using SupportIQ.Infrastructure.AI.Models;
using SupportIQ.Infrastructure.Configuration;

namespace SupportIQ.Infrastructure.AI;

/// <summary>
/// OpenAI-backed implementation of <see cref="ITicketAiService"/>. All prompt text lives in
/// SupportIQ.Application.AI.Prompts; this class is only responsible for calling the provider,
/// enforcing structured output, applying resilience, and validating the result before it ever
/// reaches application code.
/// </summary>
public class OpenAiTicketAiService : ITicketAiService
{
    private readonly OpenAIClient _client;
    private readonly AiOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<OpenAiTicketAiService> _logger;

    public OpenAiTicketAiService(OpenAIClient client, IOptions<AiOptions> options, ILogger<OpenAiTicketAiService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
        _resiliencePipeline = AiResiliencePipelineFactory.Create(_options);
    }

    public async Task<TicketAnalysisResult> AnalyzeTicketAsync(SupportTicket ticket, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var chatClient = _client.GetChatClient(_options.Model);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(TicketAnalysisPrompt.SystemPrompt),
            new UserChatMessage(TicketAnalysisPrompt.BuildUserPrompt(ticket))
        };

        var chatOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                TicketAnalysisSchema.SchemaName,
                TicketAnalysisSchema.Build(),
                jsonSchemaIsStrict: true)
        };

        var stopwatch = Stopwatch.StartNew();
        ChatCompletion completion;
        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
                async ct => await chatClient.CompleteChatAsync(messages, chatOptions, ct),
                cancellationToken);
            completion = result.Value;
        }
        catch (Exception ex) when (IsProviderFailure(ex))
        {
            _logger.LogError(ex, "AI ticket analysis call failed for TicketId {TicketId} after {ElapsedMs}ms",
                ticket.Id, stopwatch.ElapsedMilliseconds);
            throw new AIServiceException("The AI provider failed to analyze this ticket. Please try again shortly.", ex);
        }

        _logger.LogInformation("AI ticket analysis completed for TicketId {TicketId} in {ElapsedMs}ms",
            ticket.Id, stopwatch.ElapsedMilliseconds);

        return ParseAndValidate(completion, _options.Model);
    }

    public async Task<string> GenerateResponseAsync(SupportTicket ticket, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var chatClient = _client.GetChatClient(_options.Model);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SuggestedResponsePrompt.SystemPrompt),
            new UserChatMessage(SuggestedResponsePrompt.BuildUserPrompt(ticket))
        };

        var stopwatch = Stopwatch.StartNew();
        ChatCompletion completion;
        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(
                async ct => await chatClient.CompleteChatAsync(messages, cancellationToken: ct),
                cancellationToken);
            completion = result.Value;
        }
        catch (Exception ex) when (IsProviderFailure(ex))
        {
            _logger.LogError(ex, "AI response generation failed for TicketId {TicketId} after {ElapsedMs}ms",
                ticket.Id, stopwatch.ElapsedMilliseconds);
            throw new AIServiceException("The AI provider failed to generate a response. Please try again shortly.", ex);
        }

        _logger.LogInformation("AI response generation completed for TicketId {TicketId} in {ElapsedMs}ms",
            ticket.Id, stopwatch.ElapsedMilliseconds);

        var text = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(text))
            throw new AIServiceException("The AI provider returned an empty response.");

        return text.Trim();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new AIServiceException("The AI provider is not configured. Set the Ai:ApiKey environment variable.");
    }

    private static bool IsProviderFailure(Exception ex) =>
        ex is ClientResultException or TimeoutRejectedException or BrokenCircuitException;

    private static TicketAnalysisResult ParseAndValidate(ChatCompletion completion, string modelUsed)
    {
        var rawJson = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(rawJson))
            throw new AIServiceException("The AI provider returned an empty analysis.");

        RawTicketAnalysis raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawTicketAnalysis>(rawJson)
                  ?? throw new JsonException("Deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new AIServiceException("The AI provider returned output that could not be parsed as valid JSON.", ex);
        }

        if (!Enum.TryParse<TicketCategory>(raw.Category, ignoreCase: true, out var category))
            throw new AIServiceException($"The AI provider returned an invalid category: '{raw.Category}'.");
        if (!Enum.TryParse<TicketPriority>(raw.Priority, ignoreCase: true, out var priority))
            throw new AIServiceException($"The AI provider returned an invalid priority: '{raw.Priority}'.");
        if (!Enum.TryParse<TicketSentiment>(raw.Sentiment, ignoreCase: true, out var sentiment))
            throw new AIServiceException($"The AI provider returned an invalid sentiment: '{raw.Sentiment}'.");
        if (string.IsNullOrWhiteSpace(raw.Summary))
            throw new AIServiceException("The AI provider returned an empty summary.");
        if (string.IsNullOrWhiteSpace(raw.SuggestedResponse))
            throw new AIServiceException("The AI provider returned an empty suggested response.");

        var confidence = Math.Clamp(raw.Confidence, 0.0, 1.0);
        var tags = raw.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Take(10).ToList();

        return new TicketAnalysisResult(
            category, priority, sentiment, raw.Summary.Trim(), tags, raw.SuggestedResponse.Trim(), confidence, modelUsed);
    }
}
