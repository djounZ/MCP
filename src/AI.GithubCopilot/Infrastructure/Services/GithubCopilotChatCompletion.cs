using System.Text.Json;
using System.Text.Json.Serialization;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotChatCompletion(
    ILogger<GithubCopilotChatCompletion> logger,
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
            cancellationToken,
            logger
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
            cancellationToken,
            logger
        );
    }

     public async IAsyncEnumerable<ChatCompletionResponse?> GetChatCompletionStreamAsync(
        ChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {

        // Ensure streaming is enabled
        var streamingRequest = request with { Stream = true };
        ChatCompletionResponse? previousResponse = null;

        await foreach (var item in httpClientRunner.SendAndReadStreamAsync(
                           streamingRequest,
                           httpClient,
                           HttpMethod.Post,
                           Options.CopilotChatCompletionsUrl,
                           Options.CopilotChatCompletionsHeaders,
                           HttpCompletionOption.ResponseContentRead,
                           JsonOptions,
                           cancellationToken,
                           ReadItemAsync,
                           logger))
        {
            if (item.IsIgnored)
            {
                continue;
            }

            if (item.IsEnded)
            {
                if(previousResponse is not null)
                {
                    // If we have a previous response, yield it before ending

                    logger.LogInformation("Tool call detected in response: {Response}", previousResponse);
                    yield return previousResponse;
                }
                break;
            }

            var itemValue = item.Value;
            if(itemValue!=null && IsToolCall(itemValue))
            {
                if (previousResponse == null)
                {
                    previousResponse = itemValue;
                }
                else if(previousResponse.Id!= itemValue.Id)
                {
                    // If the ID has changed, yield the previous response
                    yield return previousResponse;

                    logger.LogInformation("Tool call detected in response: {Response}", previousResponse);
                    previousResponse = itemValue;
                }
                else
                {
                    // If the ID is the same, we are still processing the same response
                    previousResponse = AccumulateToolCallArguments(previousResponse, itemValue);
                }
                continue;
            }
            yield return  itemValue;
        }
    }

    private ChatCompletionResponse AccumulateToolCallArguments(ChatCompletionResponse previousResponse, ChatCompletionResponse itemValue)
    {
        // If the ID is the same, we are still processing the same response
        var functionArguments = itemValue.Choices.Single().Delta?.ToolCalls?.Single().Function?.Arguments ?? string.Empty;

        // Get the existing function arguments and append the new ones
        var existingChoice = previousResponse.Choices.Single();
        var existingToolCall = existingChoice.Delta?.ToolCalls?.Single();
        var existingArguments = existingToolCall?.Function?.Arguments ?? string.Empty;

        if (existingToolCall?.Function != null && existingChoice.Delta != null)
        {
            // Create updated tool call with accumulated arguments
            var updatedFunction = existingToolCall.Function with
            {
                Arguments = existingArguments + functionArguments
            };

            var updatedToolCall = existingToolCall with { Function = updatedFunction };
            var updatedDelta = existingChoice.Delta with
            {
                ToolCalls = [updatedToolCall]
            };
            var updatedChoice = existingChoice with { Delta = updatedDelta };

            return previousResponse with
            {
                Choices = [updatedChoice]
            };
        }
        return previousResponse;
    }
    private bool IsToolCall(ChatCompletionResponse itemValue)
    {
        if(itemValue.Choices.Count == 0)
        {
            return false;
        }

        var choice = itemValue.Choices[0];
        if(choice.Delta is null)
        {
            return false;
        }

        return choice.Delta.ToolCalls != null && choice.Delta.ToolCalls.Count != 0;
    }

    private async Task<StreamItem<ChatCompletionResponse>> ReadItemAsync(StreamReader reader, CancellationToken o)
    {
        var line = await reader.ReadLineAsync(o);
        logger.LogTrace(line);
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("event:") || line.StartsWith(":"))
        {
            return StreamItem<ChatCompletionResponse>.BuildIgnored(line);
        }

        var jsonLine = line.StartsWith("data: ") ? line[6..] : line;

        if (jsonLine == "[DONE]")
        {
            return StreamItem<ChatCompletionResponse>.BuildEnded(line);
        }

        return StreamItem<ChatCompletionResponse>.BuildContent(
            JsonSerializer.Deserialize<ChatCompletionResponse>(jsonLine, JsonOptions)!);
    }
}
