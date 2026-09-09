using System.Text.Json.Nodes;
using SupportIQ.Domain.Enums;

namespace SupportIQ.Infrastructure.AI;

/// <summary>
/// Builds the JSON Schema handed to the AI provider's structured-output mode. Enum value lists
/// are generated from the actual domain enums rather than duplicated as string literals, so the
/// schema can never drift out of sync with <see cref="TicketCategory"/>/<see cref="TicketPriority"/>/
/// <see cref="TicketSentiment"/>.
/// </summary>
public static class TicketAnalysisSchema
{
    public const string SchemaName = "ticket_analysis";

    public static BinaryData Build()
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["category"] = EnumProperty<TicketCategory>(),
                ["priority"] = EnumProperty<TicketPriority>(),
                ["sentiment"] = EnumProperty<TicketSentiment>(),
                ["summary"] = new JsonObject { ["type"] = "string" },
                ["tags"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "string" }
                },
                ["suggestedResponse"] = new JsonObject { ["type"] = "string" },
                ["confidence"] = new JsonObject { ["type"] = "number" }
            },
            ["required"] = new JsonArray(
                "category", "priority", "sentiment", "summary", "tags", "suggestedResponse", "confidence")
        };

        return BinaryData.FromString(schema.ToJsonString());
    }

    private static JsonObject EnumProperty<TEnum>() where TEnum : struct, Enum
    {
        var values = new JsonArray();
        foreach (var name in Enum.GetNames<TEnum>())
            values.Add(name);

        return new JsonObject
        {
            ["type"] = "string",
            ["enum"] = values
        };
    }
}
