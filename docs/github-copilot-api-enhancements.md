# GitHub Copilot API Model Enhancements

## Overview

This document summarizes the comprehensive improvements made to the GitHub Copilot Chat API models to align with the official GitHub API documentation and support advanced features.

## Enhanced Models

### 1. ChatCompletionRequest
**File**: `src/AI.GithubCopilot/Infrastructure/Models/ChatCompletionRequest.cs`

**New Properties Added**:
- `logit_bias`: Dictionary<string, double>? - Token bias control
- `logprobs`: bool? - Enable log probability output
- `top_logprobs`: int? - Number of top log probabilities to return
- `response_format`: ResponseFormat? - Structured output format specification
- `seed`: int? - Deterministic generation seed
- `tools`: IReadOnlyList<Tool>? - Function calling tools
- `tool_choice`: ToolChoice? - Tool selection control
- `parallel_tool_calls`: bool? - Enable parallel tool execution

### 2. ChatCompletionResponse
**File**: `src/AI.GithubCopilot/Infrastructure/Models/ChatCompletionResponse.cs`

**Updated Structure**:
- Now uses `ChatChoice` instead of `CompletionChoice`
- Added mandatory `object` field for OpenAI compatibility
- Updated to use `Usage` instead of `TokenUsage`
- Removed deprecated `CopilotReferences` (handled separately)

### 3. New Supporting Models

#### ChatChoice
**File**: `src/AI.GithubCopilot/Infrastructure/Models/ChatChoice.cs`
- Supports both complete responses (`message`) and streaming (`delta`)
- Includes log probability information
- Content filtering results and offsets
- Proper finish reason handling

#### ChatDelta  
**File**: `src/AI.GithubCopilot/Infrastructure/Models/ChatDelta.cs`
- Streaming response delta object
- Supports tool call streaming
- Refusal message support

#### MessageContent (Enhanced)
**File**: `src/AI.GithubCopilot/Infrastructure/Models/MessageContent.cs`
- **New Features**:
  - `AsText()` method for easy text extraction
  - Support for multipart content (text + images)
  - Proper JSON converter for string/array formats
- **Content Types**:
  - `TextContent`: Simple text messages
  - `MultipartContent`: Mixed content with text and images
  - `TextPart`: Text component of multipart content
  - `ImagePart`: Image component with URL and detail level

#### Tool System
**Files**: 
- `Tool.cs` - Function tool definition
- `ToolCall.cs` - Tool calls from assistant
- `ToolChoice.cs` - Tool selection control with converter
- `ToolCallDelta.cs` - Streaming tool call deltas
- `FunctionDefinition.cs` - Function specifications
- `FunctionCall.cs` - Function call details

#### Response Format System
**Files**:
- `ResponseFormat.cs` - Output format specification
- `JsonSchema.cs` - JSON schema for structured outputs

#### Enhanced Usage Tracking
**File**: `src/AI.GithubCopilot/Infrastructure/Models/TokenUsage.cs` (renamed to `Usage`)
- **New Details**:
  - `CompletionTokensDetails`: Reasoning tokens, prediction tokens
  - `PromptTokensDetails`: Cached tokens tracking

#### Log Probability Support
**File**: `src/AI.GithubCopilot/Infrastructure/Models/LogProbabilityInfo.cs`
- Token-level probability information
- Top alternative probabilities
- Text offset mapping

### 4. Builder Pattern Enhancement
**File**: `src/AI.GithubCopilot/Infrastructure/Builders/ChatCompletionRequestBuilder.cs`

**New Methods**:
- `WithTools()` - Add function calling tools
- `WithToolChoice()` - Control tool selection
- `WithParallelToolCalls()` - Enable parallel execution
- `WithResponseFormat()` - Set structured output format
- `WithJsonResponse()` - Convenient JSON response setup
- `WithLogprobs()` - Enable log probabilities
- `WithSeed()` - Set deterministic seed
- `WithUserMessage(ContentPart[])` - Multipart content support

### 5. Extension Methods
**File**: `src/AI.GithubCopilot/Infrastructure/Extensions/ChatCompletionResponseExtensions.cs`

**New Extensions**:
- `GetContent()` - Extract content from response
- `GetToolCalls()` - Get tool calls from response
- `IsComplete()` - Check if response is finished
- `WasFiltered()` - Check content filtering
- `GetFinishReason()` - Get completion reason
- `GetRefusal()` - Get model refusal message
- `IsFiltered()` - Check filter results
- `GetHighestFilterSeverity()` - Get worst filter severity

**File**: `src/AI.GithubCopilot/Infrastructure/Extensions/MessageContentExtensions.cs`

**Helper Methods**:
- `Text()` - Create text content part
- `Image()` - Create image content part
- `ImageHighDetail()` - High detail image
- `ImageLowDetail()` - Low detail image
- `CreateFunction()` - Create function tool
- `JsonResponseFormat()` - Create JSON response format

### 6. Enhanced ChatMessage
**File**: `src/AI.GithubCopilot/Infrastructure/Models/ChatMessage.cs`

**New Properties**:
- `tool_calls`: IReadOnlyList<ToolCall>? - Assistant tool calls
- `tool_call_id`: string? - Tool call response ID
- `refusal`: string? - Model refusal message
- `content`: Now nullable MessageContent for flexibility

## Usage Examples

### Basic Chat Request
```csharp
var request = ChatCompletionRequestBuilder.Create()
    .WithModel("gpt-4o")
    .WithSystemMessage("You are a helpful assistant.")
    .WithUserMessage("Hello!")
    .WithTemperature(0.7)
    .Build();
```

### Multipart Content with Images
```csharp
var request = ChatCompletionRequestBuilder.Create()
    .WithUserMessage(
        "What's in this image?".Text(),
        "https://example.com/image.jpg".ImageHighDetail()
    )
    .Build();
```

### Function Calling
```csharp
var weatherTool = MessageContentExtensions.CreateFunction(
    "get_weather",
    "Get weather for a location",
    new { 
        type = "object",
        properties = new {
            location = new { type = "string" }
        }
    }
);

var request = ChatCompletionRequestBuilder.Create()
    .WithUserMessage("What's the weather in London?")
    .WithTools(weatherTool)
    .WithToolChoice(ToolChoice.Auto)
    .Build();
```

### Structured JSON Output
```csharp
var request = ChatCompletionRequestBuilder.Create()
    .WithUserMessage("Extract name and age from: John is 25")
    .WithJsonResponse("person", "Person information")
    .Build();
```

### Working with Responses
```csharp
// Get content
var content = response.GetContent();

// Check tool calls
var toolCalls = response.GetToolCalls();

// Check filtering
if (response.WasFiltered())
{
    var severity = response.Choices[0].ContentFilterResults?.GetHighestFilterSeverity();
    Console.WriteLine($"Content filtered with severity: {severity}");
}

// Handle refusals
var refusal = response.GetRefusal();
if (refusal != null)
{
    Console.WriteLine($"Model refused: {refusal}");
}
```

## Testing

Comprehensive tests added in:
- `tests/AI.GithubCopilot.Tests/Infrastructure/Models/EnhancedChatCompletionRequestTests.cs`
- `tests/AI.GithubCopilot.Tests/Infrastructure/Models/EnhancedChatCompletionResponseTests.cs`

## Breaking Changes

1. **ChatCompletionResponse.Choices**: Now uses `ChatChoice` instead of `CompletionChoice`
2. **Usage Model**: Renamed from `TokenUsage` to `Usage`
3. **ChatMessage.Content**: Now nullable `MessageContent` instead of required string
4. **Removed Properties**: `CopilotReferences` removed from `ChatCompletionResponse`

## Migration Guide

### Updating Response Handling
```csharp
// Before
var content = response.Choices[0].Message?.Content;

// After  
var content = response.GetContent(); // or
var content = response.Choices[0].Message?.Content?.AsText();
```

### Updating Usage References
```csharp
// Before
TokenUsage usage = response.Usage;

// After
Usage usage = response.Usage;
```

### Building Requests
```csharp
// Before - manual construction
var request = new ChatCompletionRequest(
    messages: new[] { new ChatMessage("user", "Hello") },
    model: "gpt-4o"
);

// After - using builder
var request = ChatCompletionRequestBuilder.Create()
    .WithModel("gpt-4o")
    .WithUserMessage("Hello")
    .Build();
```

## Benefits

1. **Full API Compliance**: Matches GitHub's official Chat API specification
2. **Advanced Features**: Function calling, structured outputs, content filtering
3. **Type Safety**: Strong typing with proper nullable annotations
4. **Extensibility**: Builder pattern and extension methods for easy usage
5. **Streaming Support**: Proper delta handling for streaming responses
6. **Content Flexibility**: Support for text and multipart (image) content
7. **Comprehensive Testing**: Full test coverage for all new features
