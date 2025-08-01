using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public sealed record ChatResponseAppModel(
    [property: JsonPropertyName("messages")] IList<ChatMessageAppModel>? Messages,
    [property: JsonPropertyName("response_id")] string? ResponseId,
    [property: JsonPropertyName("conversation_id")] string? ConversationId,
    [property: JsonPropertyName("model_id")] string? ModelId,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("finish_reason")] ChatFinishReason? FinishReason
);