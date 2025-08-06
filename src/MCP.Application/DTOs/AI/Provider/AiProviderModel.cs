using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Provider;

public record AiProviderAiModelAppModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);

public record AiProviderAppModel(
    [property: JsonPropertyName("name")] AiProviderEnum Name,
    [property: JsonPropertyName("models")] IList<AiProviderAiModelAppModel> Models);
