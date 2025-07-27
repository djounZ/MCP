using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Metadata for copilot references
/// </summary>
public record CopilotReferenceMetadata(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("display_url")] string? DisplayUrl
);