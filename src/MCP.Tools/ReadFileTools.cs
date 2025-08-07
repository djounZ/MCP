using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
namespace MCP.Tools;

[McpServerToolType]
public sealed class ReadFileTools
{
    [McpServerTool, Description("Read the contents of a file. Optionally specify offset (1-based line number) and limit (number of lines) to read a chunk. Output is truncated at 2000 lines.")]
    public static async Task<string> ReadFileAsync(
        [Description("The absolute path of the file to read.")] string filePath,
        [Description("Optional: 1-based line number to start reading from. Defaults to 1.")] int? offset = null,
        [Description("Optional: maximum number of lines to read. Defaults to 2000.")] int? limit = null)
    {
        const int maxLinesPerRead = 2000;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Error: filePath is required.";
        }

        if (!File.Exists(filePath))
        {
            return $"Error: File not found: {filePath}";
        }

        var startLine = Math.Max(offset ?? 1, 1);
        var linesToRead = Math.Min(limit ?? maxLinesPerRead, maxLinesPerRead);

        var lines = new List<string>();
        var truncated = false;

        try
        {
            (truncated, var totalLines) = await ReadLinesAsync(filePath, startLine, lines, linesToRead, truncated);
            return BuildResult(filePath, startLine, lines, totalLines, truncated);
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    private static async Task<(bool truncated, int totalLines)> ReadLinesAsync(string filePath, int startLine, List<string> lines, int linesToRead, bool truncated)
    {
        using var reader = new StreamReader(filePath);
        var currentLine = 1;
        while (await reader.ReadLineAsync() is { } line)
        {
            if (currentLine >= startLine && lines.Count < linesToRead)
            {
                lines.Add(line);
            }
            currentLine++;
            if (lines.Count < linesToRead)
            {
                continue;
            }
            truncated = true;
            break;
        }
        var totalLines = currentLine - 1;
        return (truncated, totalLines);
    }

    private static string BuildResult(string filePath, int startLine, List<string> lines, int totalLines, bool truncated)
    {
        if (lines.Count == 0)
        {
            return $"The file `{filePath}` exists, but is empty or contains only whitespace.";
        }
        var sb = new StringBuilder();
        sb.AppendLine($"File: `{filePath}`. Lines {startLine} to {startLine + lines.Count - 1} ({totalLines} lines total):");
        sb.AppendLine("```");
        foreach (var l in lines)
        {
            sb.AppendLine(l);
        }
        sb.AppendLine("```");

        if (truncated)
        {
            sb.AppendLine($"[File content truncated at line {startLine + lines.Count - 1}. Use ReadFileAsync with offset/limit parameters to view more.]");
        }

        return sb.ToString();
    }
}
