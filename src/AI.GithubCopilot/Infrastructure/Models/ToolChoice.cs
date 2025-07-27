using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Tool choice specification - can be a string or object
/// </summary>
[JsonConverter(typeof(ToolChoiceConverter))]
public record ToolChoice
{
    public string? Type { get; init; }
    public ToolFunction? Function { get; init; }

    public static implicit operator ToolChoice(string value) => new() { Type = value };
    
    public static ToolChoice Auto => new() { Type = "auto" };
    public static ToolChoice None => new() { Type = "none" };
    public static ToolChoice Required => new() { Type = "required" };
    
    public static ToolChoice ForFunction(string name) => new()
    {
        Type = "function",
        Function = new ToolFunction(name)
    };
}

/// <summary>
/// Tool function specification for specific function selection
/// </summary>
public record ToolFunction(
    [property: JsonPropertyName("name")] string Name
);

/// <summary>
/// Converter for ToolChoice to handle both string and object formats
/// </summary>
public class ToolChoiceConverter : JsonConverter<ToolChoice>
{
    public override ToolChoice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new ToolChoice { Type = reader.GetString() };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            
            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
            ToolFunction? function = null;
            
            if (root.TryGetProperty("function", out var functionElement))
            {
                var name = functionElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                if (name != null)
                {
                    function = new ToolFunction(name);
                }
            }
            
            return new ToolChoice { Type = type, Function = function };
        }

        throw new JsonException("ToolChoice must be a string or object");
    }

    public override void Write(Utf8JsonWriter writer, ToolChoice value, JsonSerializerOptions options)
    {
        if (value.Function == null)
        {
            writer.WriteStringValue(value.Type);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteString("type", value.Type);
            writer.WritePropertyName("function");
            writer.WriteStartObject();
            writer.WriteString("name", value.Function.Name);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
