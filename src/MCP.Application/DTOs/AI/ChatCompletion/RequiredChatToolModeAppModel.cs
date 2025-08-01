using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public sealed record RequiredChatToolModeAppModel(
    [property: JsonPropertyName("required_function_name")] string? RequiredFunctionName
) : ChatToolModeAppModel;
