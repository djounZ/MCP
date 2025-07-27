using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Full copilot reference with metadata
/// </summary>
public record CopilotReferenceWithMetadata(
    [property: JsonPropertyName("metadata")] CopilotReferenceMetadata? Metadata
);