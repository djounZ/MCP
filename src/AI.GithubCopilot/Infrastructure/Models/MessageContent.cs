using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Message content that can be either text or multipart content
/// </summary>
[JsonConverter(typeof(MessageContentConverter))]
public abstract record MessageContent
{
    public static implicit operator MessageContent(string text) => new TextContent(text);
    public static MessageContent FromText(string text) => new TextContent(text);
    public static MessageContent Multipart(params ContentPart[] parts) => new MultipartContent(parts);

    /// <summary>
    /// Converts content to text string if possible
    /// </summary>
    public string? AsText() => this switch
    {
        TextContent text => text.Text,
        MultipartContent multipart => string.Join(" ", multipart.Parts.OfType<TextPart>().Select(p => p.Text)),
        _ => null
    };
}

/// <summary>
/// Simple text content
/// </summary>
public record TextContent(string Text) : MessageContent;

/// <summary>
/// Multipart content with text and images
/// </summary>
public record MultipartContent(ContentPart[] Parts) : MessageContent;

/// <summary>
/// Part of multipart content
/// </summary>

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextPart), typeDiscriminator: "text")]
[JsonDerivedType(typeof(ImagePart), typeDiscriminator: "image_url")]
public abstract record ContentPart;

/// <summary>
/// Text part of multipart content
/// </summary>
public sealed record TextPart(
    [property: JsonPropertyName("text")] string Text
) : ContentPart;

/// <summary>
/// Image part of multipart content
/// </summary>
public sealed record ImagePart(
    [property: JsonPropertyName("image_url")] ImageUrl ImageUrl
) : ContentPart;

/// <summary>
/// Image URL specification
/// </summary>
public record ImageUrl(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("detail")] string? Detail = null
);

/// <summary>
/// Converter for MessageContent to handle both string and object formats
/// </summary>
public class MessageContentConverter : JsonConverter<MessageContent>
{
    public override MessageContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new TextContent(reader.GetString()!);
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var parts = JsonSerializer.Deserialize<ContentPart[]>(ref reader, options);
            return new MultipartContent(parts ?? []);
        }

        throw new JsonException("MessageContent must be a string or array");
    }

    public override void Write(Utf8JsonWriter writer, MessageContent value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case TextContent text:
                writer.WriteStringValue(text.Text);
                break;
            case MultipartContent multipart:
                JsonSerializer.Serialize(writer, multipart.Parts, options);
                break;
            default:
                throw new JsonException($"Unknown MessageContent type: {value.GetType()}");
        }
    }
}
