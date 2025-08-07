using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MCP.Tools;

[McpServerToolType]
public sealed class DocInfoTools
{
    private static readonly Dictionary<string, string> DocTypeNames = new()
    {
        { "typescript", "TSDoc comment" },
        { "typescriptreact", "TSDoc comment" },
        { "javascript", "JSDoc comment" },
        { "javascriptreact", "JSDoc comment" },
        { "python", "docstring" }
    };

    [McpServerTool,
     Description(
         "Suggests the correct documentation comment type for a set of files based on their language. ONLY adds documentation, does not change code.")]
    public static string DocInfo(
        [Description("The absolute paths of the files to analyze.")]
        string[] filePaths)
    {
        var docNames = new HashSet<string>();

        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                continue;
            }

            var languageId = GetLanguageIdFromExtension(filePath);
            var docName = DocTypeNames.GetValueOrDefault(languageId, "documentation comment");
            docNames.Add(docName);
        }

        return docNames.Count == 0 ? "No valid files provided." : $"Please generate {string.Join(", ", docNames)} for the respective files. ONLY add documentation and do not change the code.";
    }

    // Simple extension-to-languageId mapping for demo purposes
    private static string GetLanguageIdFromExtension(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".ts" => "typescript",
            ".tsx" => "typescriptreact",
            ".js" => "javascript",
            ".jsx" => "javascriptreact",
            ".py" => "python",
            _ => ""
        };
    }
}
