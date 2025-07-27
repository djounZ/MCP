using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Content filter offsets indicating where filtering was applied
/// </summary>
public record ContentFilterOffsets(
    [property: JsonPropertyName("check_offset")] int CheckOffset,
    [property: JsonPropertyName("start_offset")] int StartOffset,
    [property: JsonPropertyName("end_offset")] int EndOffset
);
