namespace MCP.Tools.Tests;

public class ReadFileToolsTests : IDisposable
{
    private readonly string _testDir;

    public ReadFileToolsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    private string CreateTestFile(string content)
    {
        var file = Path.Combine(_testDir, Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(file, content);
        return file;
    }

    private string CreateTestFile(string[] lines)
    {
        var file = Path.Combine(_testDir, Guid.NewGuid().ToString() + ".txt");
        File.WriteAllLines(file, lines);
        return file;
    }

    [Fact]
    public async Task ReadFileAsync_ReadsWholeFile_Defaults()
    {
        var file = CreateTestFile(new[] { "line1", "line2", "line3" });
        var result = await ReadFileTools.ReadFileAsync(file);
        Assert.Contains("line1", result);
        Assert.Contains("line2", result);
        Assert.Contains("line3", result);
        Assert.DoesNotContain("truncated", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadFileAsync_ReadsWithOffsetAndLimit()
    {
        var file = CreateTestFile(new[] { "a", "b", "c", "d", "e" });
        var result = await ReadFileTools.ReadFileAsync(file, offset: 2, limit: 2);
        // Extract lines inside the code block
        var lines = ExtractCodeBlockLines(result);
        Assert.Equal(new[] { "b", "c" }, lines);

        static string[] ExtractCodeBlockLines(string output)
        {
            var start = output.IndexOf("```", StringComparison.Ordinal);
            if (start == -1) return Array.Empty<string>();
            var end = output.IndexOf("```", start + 3, StringComparison.Ordinal);
            if (end == -1) return Array.Empty<string>();
            var content = output.Substring(start + 3, end - (start + 3));
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    [Fact]
    public async Task ReadFileAsync_TruncatesAt2000Lines()
    {
        var lines = new string[2100];
        for (int i = 0; i < 2100; i++) lines[i] = $"L{i + 1}";
        var file = CreateTestFile(lines);
        var result = await ReadFileTools.ReadFileAsync(file);
        Assert.Contains("L1", result);
        Assert.Contains("L2000", result);
        Assert.DoesNotContain("L2001", result);
        Assert.Contains("truncated", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadFileAsync_FileNotFound()
    {
        var file = Path.Combine(_testDir, "doesnotexist.txt");
        var result = await ReadFileTools.ReadFileAsync(file);
        Assert.Contains("Error: File not found", result);
    }

    [Fact]
    public async Task ReadFileAsync_EmptyPath()
    {
        var result = await ReadFileTools.ReadFileAsync("");
        Assert.Contains("Error: filePath is required", result);
    }

    [Fact]
    public async Task ReadFileAsync_EmptyFile()
    {
        var file = CreateTestFile("");
        var result = await ReadFileTools.ReadFileAsync(file);
        Assert.Contains("exists, but is empty", result);
    }

    [Fact]
    public async Task ReadFileAsync_WhitespaceOnlyFile()
    {
        var file = CreateTestFile("   \n   \n");
        var result = await ReadFileTools.ReadFileAsync(file);
        // Should still be considered as lines, not empty
        Assert.Contains("File: `", result);
        Assert.Contains("```", result);
    }

    [Fact]
    public async Task ReadFileAsync_ExceptionHandling()
    {
        // Simulate error by passing a directory as file (should return 'Error: File not found')
        var dir = Path.Combine(_testDir, "subdir");
        Directory.CreateDirectory(dir);
        var result = await ReadFileTools.ReadFileAsync(dir);
        Assert.Contains("Error: File not found", result);
    }
}
