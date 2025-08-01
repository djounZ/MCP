using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record UsageDetailsAppModel(
    [property: JsonPropertyName("input_token_count")] long? InputTokenCount,
    [property: JsonPropertyName("output_token_count")] long? OutputTokenCount,
    [property: JsonPropertyName("total_token_count")] long? TotalTokenCount,
    [property: JsonPropertyName("additional_counts")] IDictionary<string,long>? AdditionalCounts
);
