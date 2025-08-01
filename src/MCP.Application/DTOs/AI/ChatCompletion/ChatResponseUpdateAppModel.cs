using System.Text.Json.Serialization;
using MCP.Application.DTOs.AI.Contents;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public sealed record ChatResponseUpdateAppModel(
    [property: JsonPropertyName("author_name")] string? AuthorName,
    [property: JsonPropertyName("role")] ChatRoleEnumAppModel? Role,
    [property: JsonPropertyName("contents")] IList<AiContentAppModel> Contents,
    [property: JsonPropertyName("response_id")] string? ResponseId,
    [property: JsonPropertyName("message_id")] string? MessageId,
    [property: JsonPropertyName("conversation_id")] string? ConversationId,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("finish_reason")] ChatFinishReason? FinishReason,
    [property: JsonPropertyName("model_id")] string? ModelId
);