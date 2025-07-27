using AI.GithubCopilot.Infrastructure.Models;

namespace AI.GithubCopilot.Infrastructure.Extensions;

/// <summary>
/// Extension methods for creating message content
/// </summary>
public static class MessageContentExtensions
{
    /// <summary>
    /// Create a text content part
    /// </summary>
    public static TextPart Text(this string text) => new(text);

    /// <summary>
    /// Create an image content part from URL
    /// </summary>
    public static ImagePart Image(this string url, string? detail = null) => 
        new(new ImageUrl(url, detail));

    /// <summary>
    /// Create an image content part with high detail
    /// </summary>
    public static ImagePart ImageHighDetail(this string url) => 
        new(new ImageUrl(url, "high"));

    /// <summary>
    /// Create an image content part with low detail
    /// </summary>
    public static ImagePart ImageLowDetail(this string url) => 
        new(new ImageUrl(url, "low"));

    /// <summary>
    /// Create a function tool
    /// </summary>
    public static Tool CreateFunction(string name, string? description = null, object? parameters = null, bool? strict = null) =>
        new("function", new FunctionDefinition(name, description, parameters, strict));

    /// <summary>
    /// Create a JSON response format
    /// </summary>
    public static ResponseFormat JsonResponseFormat(string? schemaName = null, string? description = null, object? schema = null, bool? strict = null)
    {
        if (schemaName == null)
        {
            return new ResponseFormat("json_object");
        }

        return new ResponseFormat("json_object", new JsonSchema(schemaName, description, schema, strict));
    }

    /// <summary>
    /// Create a text response format
    /// </summary>
    public static ResponseFormat TextResponseFormat() => new("text");
}
