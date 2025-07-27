using AI.GithubCopilot.Infrastructure.Models;

namespace AI.GithubCopilot.Infrastructure.Builders;

/// <summary>
/// Builder for ChatCompletionRequest with fluent API
/// </summary>
public class ChatCompletionRequestBuilder
{
    private readonly List<ChatMessage> _messages = new();
    private string _model = "gpt-4o";
    private double? _temperature;
    private double? _topP;
    private int? _maxTokens;
    private bool? _stream;
    private string[]? _stop;
    private double? _presencePenalty;
    private double? _frequencyPenalty;
    private Dictionary<string, double>? _logitBias;
    private string? _user;
    private int? _n;
    private Tool[]? _tools;
    private ToolChoice? _toolChoice;
    private bool? _parallelToolCalls;
    private ResponseFormat? _responseFormat;
    private int? _seed;
    private double? _topLogprobs;
    private bool? _logprobs;

    public ChatCompletionRequestBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public ChatCompletionRequestBuilder WithMessage(string role, string content, string? name = null)
    {
        return WithMessage(role, MessageContent.FromText(content), name);
    }

    public ChatCompletionRequestBuilder WithMessage(string role, MessageContent content, string? name = null)
    {
        _messages.Add(new ChatMessage(role, content, name));
        return this;
    }

    public ChatCompletionRequestBuilder WithSystemMessage(string content)
    {
        return WithMessage("system", content);
    }

    public ChatCompletionRequestBuilder WithUserMessage(string content)
    {
        return WithMessage("user", content);
    }

    public ChatCompletionRequestBuilder WithUserMessage(params ContentPart[] parts)
    {
        return WithMessage("user", MessageContent.Multipart(parts));
    }

    public ChatCompletionRequestBuilder WithAssistantMessage(string content)
    {
        return WithMessage("assistant", content);
    }

    public ChatCompletionRequestBuilder WithTemperature(double temperature)
    {
        _temperature = temperature;
        return this;
    }

    public ChatCompletionRequestBuilder WithTopP(double topP)
    {
        _topP = topP;
        return this;
    }

    public ChatCompletionRequestBuilder WithMaxTokens(int maxTokens)
    {
        _maxTokens = maxTokens;
        return this;
    }

    public ChatCompletionRequestBuilder WithStream(bool stream = true)
    {
        _stream = stream;
        return this;
    }

    public ChatCompletionRequestBuilder WithStop(params string[] stop)
    {
        _stop = stop;
        return this;
    }

    public ChatCompletionRequestBuilder WithPresencePenalty(double presencePenalty)
    {
        _presencePenalty = presencePenalty;
        return this;
    }

    public ChatCompletionRequestBuilder WithFrequencyPenalty(double frequencyPenalty)
    {
        _frequencyPenalty = frequencyPenalty;
        return this;
    }

    public ChatCompletionRequestBuilder WithLogitBias(Dictionary<string, double> logitBias)
    {
        _logitBias = logitBias;
        return this;
    }

    public ChatCompletionRequestBuilder WithUser(string user)
    {
        _user = user;
        return this;
    }

    public ChatCompletionRequestBuilder WithN(int n)
    {
        _n = n;
        return this;
    }

    public ChatCompletionRequestBuilder WithTools(params Tool[] tools)
    {
        _tools = tools;
        return this;
    }

    public ChatCompletionRequestBuilder WithToolChoice(ToolChoice toolChoice)
    {
        _toolChoice = toolChoice;
        return this;
    }

    public ChatCompletionRequestBuilder WithParallelToolCalls(bool parallelToolCalls = true)
    {
        _parallelToolCalls = parallelToolCalls;
        return this;
    }

    public ChatCompletionRequestBuilder WithResponseFormat(ResponseFormat responseFormat)
    {
        _responseFormat = responseFormat;
        return this;
    }

    public ChatCompletionRequestBuilder WithJsonResponse(string? name = null, string? description = null)
    {
        _responseFormat = new ResponseFormat("json_object");
        if (name != null)
        {
            _responseFormat = _responseFormat with 
            { 
                JsonSchema = new JsonSchema(name, description) 
            };
        }
        return this;
    }

    public ChatCompletionRequestBuilder WithSeed(int seed)
    {
        _seed = seed;
        return this;
    }

    public ChatCompletionRequestBuilder WithLogprobs(bool logprobs = true, double? topLogprobs = null)
    {
        _logprobs = logprobs;
        if (topLogprobs.HasValue)
        {
            _topLogprobs = topLogprobs.Value;
        }
        return this;
    }

    public ChatCompletionRequest Build()
    {
        if (_messages.Count == 0)
        {
            throw new InvalidOperationException("At least one message is required");
        }

        return new ChatCompletionRequest(
            Messages: _messages.ToArray(),
            Model: _model,
            Temperature: _temperature,
            MaxTokens: _maxTokens,
            Stream: _stream ?? false,
            TopP: _topP,
            FrequencyPenalty: _frequencyPenalty,
            PresencePenalty: _presencePenalty,
            Stop: _stop,
            User: _user,
            N: _n,
            LogitBias: _logitBias,
            LogProbs: _logprobs,
            TopLogProbs: _topLogprobs.HasValue ? (int)_topLogprobs.Value : null,
            ResponseFormat: _responseFormat,
            Seed: _seed,
            Tools: _tools,
            ToolChoice: _toolChoice,
            ParallelToolCalls: _parallelToolCalls
        );
    }

    public static ChatCompletionRequestBuilder Create() => new();
}
