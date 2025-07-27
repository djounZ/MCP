using System.Text.Json;
using System.Text.Json.Serialization;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotChat(
    HttpClient httpClient,
    IOptions<GithubOptions> options,
    HttpClientRunner httpClientRunner)

{
    private GithubOptions Options => options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


     public async IAsyncEnumerable<ChatCompletionResponse?> GetChatCompletionStreamAsync(
        ChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        // Ensure streaming is enabled
        var streamingRequest = request with { Stream = true };


        await foreach (var item in httpClientRunner.SendAsyncAndReadStream(
                           streamingRequest,
                           httpClient,
                           HttpMethod.Post,
                           Options.CopilotChatCompletionsUrl,
                           Options.CopilotChatCompletionsHeaders,
                           HttpCompletionOption.ResponseContentRead,
                           JsonOptions,
                           cancellationToken,
                           ReadItemAsync))
        {
            if (item.Ignored)
            {
                continue;
            }

            if (item.Ended)
            {
                break;
            }

            yield return  item.Value;
        }
    }

    private async Task<StreamItem<ChatCompletionResponse>> ReadItemAsync(StreamReader reader, CancellationToken o)
    {
        var line = await reader.ReadLineAsync(o);

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("event:") || line.StartsWith(":"))
        {
            return StreamItem<ChatCompletionResponse>.IgnoredItem;
        }

        var jsonLine = line.StartsWith("data: ") ? line[6..] : line;

        if (jsonLine == "[DONE]")
        {
            return StreamItem<ChatCompletionResponse>.EndedItem;
        }

        return new StreamItem<ChatCompletionResponse>(
            JsonSerializer.Deserialize<ChatCompletionResponse>(jsonLine, JsonOptions));
    }
}
