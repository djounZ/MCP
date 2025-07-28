using System.Text.Json;
using System.Text.Json.Serialization;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotChat(
    ILogger<GithubCopilotChat> logger,
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


    public async Task<ModelsResponse> GetModelsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClientRunner.SendAndDeserializeAsync<ModelsResponse>(
            httpClient,
            HttpMethod.Get,
            Options.CopilotModelsUrl,
            Options.CopilotChatCompletionsHeaders,
            HttpCompletionOption.ResponseContentRead,
            JsonOptions,
            cancellationToken
        );

        return response;
    }

    public async Task<ChatCompletionResponse> GetChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        // Ensure streaming is disabled
        var streamingRequest = request with { Stream = false };
        return await httpClientRunner.SendAndDeserializeAsync<ChatCompletionRequest,ChatCompletionResponse>(
            streamingRequest,
            httpClient,
            HttpMethod.Post,
            Options.CopilotChatCompletionsUrl,
            Options.CopilotChatCompletionsHeaders,
            HttpCompletionOption.ResponseContentRead,
            JsonOptions,
            cancellationToken
        );
    }

     public async IAsyncEnumerable<ChatCompletionResponse?> GetChatCompletionStreamAsync(
        ChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {

        // Ensure streaming is enabled
        var streamingRequest = request with { Stream = true };


        await foreach (var item in httpClientRunner.SendAndReadStreamAsync(
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
            if (item.IsIgnored)
            {
                logger.LogInformation("Ignored Item Received: {@RawMessage}",item.Ignored);
                continue;
            }

            if (item.IsEnded)
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
            return StreamItem<ChatCompletionResponse>.BuildIgnored(line);
        }

        var jsonLine = line.StartsWith("data: ") ? line[6..] : line;

        if (jsonLine == "[DONE]")
        {
            return StreamItem<ChatCompletionResponse>.BuildEnded(line!);
        }

        return StreamItem<ChatCompletionResponse>.BuildContent(
            JsonSerializer.Deserialize<ChatCompletionResponse>(jsonLine, JsonOptions)!);
    }
}
