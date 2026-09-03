using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SupportIQ.API.Extensions;

/// <summary>Renders health check results as JSON (status per dependency) instead of the default plain-text "Healthy"/"Unhealthy".</summary>
public static class HealthCheckJsonWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions));
    }
}
