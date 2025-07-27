namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Builder class to help construct chat completion requests
/// </summary>
public class ChatCompletionRequestBuilder
{
    private readonly List<ChatMessage> _messages = new();
    private string _model = string.Empty;
    private int? _n;
    private double? _topP;
    private bool? _stream;
    private double? _temperature;
    private int? _maxTokens;

    public ChatCompletionRequestBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public ChatCompletionRequestBuilder AddSystemMessage(string content)
    {
        _messages.Add(new ChatMessage("system", content));
        return this;
    }

    public ChatCompletionRequestBuilder AddUserMessage(string content)
    {
        _messages.Add(new ChatMessage("user", content));
        return this;
    }

    public ChatCompletionRequestBuilder AddAssistantMessage(string content)
    {
        _messages.Add(new ChatMessage("assistant", content));
        return this;
    }

    public ChatCompletionRequestBuilder WithN(int n)
    {
        _n = n;
        return this;
    }

    public ChatCompletionRequestBuilder WithTopP(double topP)
    {
        _topP = topP;
        return this;
    }

    public ChatCompletionRequestBuilder WithStreaming(bool stream)
    {
        _stream = stream;
        return this;
    }

    public ChatCompletionRequestBuilder WithTemperature(double temperature)
    {
        _temperature = temperature;
        return this;
    }

    public ChatCompletionRequestBuilder WithMaxTokens(int maxTokens)
    {
        _maxTokens = maxTokens;
        return this;
    }

    /// <summary>
    /// Builds the request with O1 model settings (no streaming, temperature, etc.)
    /// </summary>
    public ChatCompletionRequest BuildForO1Model()
    {
        if (string.IsNullOrEmpty(_model))
            throw new InvalidOperationException("Model must be specified");

        return new ChatCompletionRequest(
            Messages: _messages.AsReadOnly(),
            Model: _model
        );
    }

    /// <summary>
    /// Builds the request with standard model settings
    /// </summary>
    public ChatCompletionRequest BuildForStandardModel()
    {
        if (string.IsNullOrEmpty(_model))
            throw new InvalidOperationException("Model must be specified");

        return new ChatCompletionRequest(
            Messages: _messages.AsReadOnly(),
            Model: _model,
            N: _n ?? 1,
            TopP: _topP ?? 1.0,
            Stream: _stream ?? true,
            Temperature: _temperature,
            MaxTokens: _maxTokens
        );
    }

    /// <summary>
    /// Builds the request automatically choosing settings based on model type
    /// </summary>
    public ChatCompletionRequest Build()
    {
        if (_model.StartsWith("o1", StringComparison.OrdinalIgnoreCase))
        {
            return BuildForO1Model();
        }
        else
        {
            return BuildForStandardModel();
        }
    }
}