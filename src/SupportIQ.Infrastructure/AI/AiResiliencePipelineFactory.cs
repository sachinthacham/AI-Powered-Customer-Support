using OpenAI;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using SupportIQ.Infrastructure.Configuration;
using System.ClientModel;

namespace SupportIQ.Infrastructure.AI;

/// <summary>
/// Builds the resilience pipeline wrapped around every outbound AI provider call: retry with
/// exponential backoff for transient failures (429/5xx/timeouts), a circuit breaker so a
/// struggling provider stops being hammered, and a per-attempt timeout. Order matters here -
/// retry (outer) re-attempts through the circuit breaker (middle), and each attempt gets its
/// own timeout (inner) rather than one timeout for the whole retry sequence.
/// </summary>
public static class AiResiliencePipelineFactory
{
    public static ResiliencePipeline Create(AiOptions options)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ClientResultException>(IsTransient)
                    .Handle<TimeoutRejectedException>()
                    .Handle<HttpRequestException>(),
                MaxRetryAttempts = options.MaxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<ClientResultException>(IsTransient),
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds))
            .Build();
    }

    private static bool IsTransient(ClientResultException exception) =>
        exception.Status == 429 || exception.Status >= 500;
}
