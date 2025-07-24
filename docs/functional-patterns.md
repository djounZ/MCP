# Functional Programming Patterns in MCP

This document outlines the functional programming patterns used in the MCP project, particularly the Result monad pattern for error handling.

## Result Monad Pattern

### Overview

The `Result<T>` monad is a functional programming construct that represents the outcome of an operation that can either succeed with a value or fail with an error message. This pattern eliminates the need for exception-based error handling in business logic.

### Result<T> Definition

```csharp
public abstract record Result<T>
{
    public abstract bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    public abstract T Value { get; }
    public abstract string Error { get; }
    
    public static Result<T> Success(T value) => new SuccessResult<T>(value);
    public static Result<T> Failure(string error) => new FailureResult<T>(error);
    
    // Functional composition methods
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper);
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder);
}
```

### Usage Examples

#### Basic Usage

```csharp
// Creating Results
var success = Result<string>.Success("Hello, World!");
var failure = Result<string>.Failure("Something went wrong");

// Using static factory methods
var result1 = Result.Success("Hello");
var result2 = Result.Failure<string>("Error occurred");
```

#### Service Implementation

```csharp
public async Task<Result<string>> GetCompletionAsync(string prompt, string language = "python")
{
    // Validate input
    if (string.IsNullOrWhiteSpace(prompt))
        return Result.Failure<string>("Prompt cannot be empty");

    // Check token validity
    if (_token == null || IsTokenInvalid(_token))
    {
        var tokenResult = await GetTokenInternalAsync();
        if (tokenResult.IsFailure)
            return Result.Failure<string>($"Failed to get token: {tokenResult.Error}");
    }

    try
    {
        // Make HTTP request
        var responseResult = await SendPostRequestAsync(_options.CompletionUrl, requestData, headers);
        if (responseResult.IsFailure)
            return Result.Failure<string>($"Failed to get completion: {responseResult.Error}");

        // Process response
        var completion = ParseStreamingResponse(responseResult.Value);
        return Result.Success(completion);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during completion request");
        return Result.Failure<string>($"Unexpected error: {ex.Message}");
    }
}
```

#### Consumer Code

```csharp
// Explicit error handling
var result = await copilotService.GetCompletionAsync("def fibonacci(n):");

if (result.IsSuccess)
{
    Console.WriteLine($"Completion: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}

// Using Match for functional style
var output = result.Match(
    onSuccess: completion => $"Success: {completion}",
    onFailure: error => $"Failed: {error}"
);

// Using Map for transformations
var lengthResult = result.Map(completion => completion.Length);
```

## Functional Composition

### Map Operation

Transforms the value inside a successful Result without changing the Result wrapper:

```csharp
var stringResult = Result.Success("hello");
var upperResult = stringResult.Map(s => s.ToUpper()); // Result<string> with "HELLO"
var lengthResult = stringResult.Map(s => s.Length);   // Result<int> with 5
```

### Bind Operation (Flatmap)

Chains operations that return Results, preventing nested Result<Result<T>>:

```csharp
public async Task<Result<ProcessedData>> ProcessDataAsync(string input)
{
    return await ValidateInput(input)
        .BindAsync(async validInput => await FetchDataAsync(validInput))
        .BindAsync(async data => await ProcessAsync(data));
}

private Result<string> ValidateInput(string input)
{
    return string.IsNullOrWhiteSpace(input) 
        ? Result.Failure<string>("Input cannot be empty")
        : Result.Success(input.Trim());
}

private async Task<Result<RawData>> FetchDataAsync(string input)
{
    try
    {
        var data = await _dataService.GetAsync(input);
        return data != null 
            ? Result.Success(data)
            : Result.Failure<RawData>("Data not found");
    }
    catch (Exception ex)
    {
        return Result.Failure<RawData>($"Failed to fetch data: {ex.Message}");
    }
}
```

### Match Operation

Provides a way to extract values from Results safely:

```csharp
// Basic match
var message = result.Match(
    onSuccess: value => $"Got value: {value}",
    onFailure: error => $"Error occurred: {error}"
);

// Async match
var response = await result.MatchAsync(
    onSuccess: async value => await ProcessSuccessAsync(value),
    onFailure: async error => await LogErrorAsync(error)
);
```

## Error Handling Strategy

### When to Use Result<T>

✅ **Use Result<T> for:**
- Business logic operations that can fail
- External API calls
- Data validation
- File I/O operations
- Database operations
- Any operation where failure is expected and should be handled gracefully

### When to Use Exceptions

✅ **Use Exceptions for:**
- Constructor parameter validation
- Programming errors (null reference, index out of range)
- System-level failures
- Infrastructure issues
- Unrecoverable errors

### Example: Hybrid Approach

```csharp
public class CopilotService : ICopilotService
{
    // Constructor uses exceptions for fail-fast validation
    public CopilotService(HttpClient httpClient, CopilotServiceOptions options, ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Business methods use Result<T> for graceful error handling
    public async Task<Result<string>> GetCompletionAsync(string prompt)
    {
        // Business logic with Result pattern
        return await ProcessRequestAsync(prompt);
    }
}
```

## Testing Result<T> Patterns

### Unit Test Examples

```csharp
[Fact]
public async Task GetCompletionAsync_WithValidPrompt_ShouldReturnSuccess()
{
    // Arrange
    var prompt = "def fibonacci(n):";
    
    // Act
    var result = await _service.GetCompletionAsync(prompt);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.Value);
}

[Fact]
public async Task GetCompletionAsync_WithEmptyPrompt_ShouldReturnFailure()
{
    // Arrange
    var emptyPrompt = "";
    
    // Act
    var result = await _service.GetCompletionAsync(emptyPrompt);
    
    // Assert
    Assert.True(result.IsFailure);
    Assert.Contains("empty", result.Error.ToLower());
}

[Fact]
public void Result_Map_ShouldTransformSuccessValue()
{
    // Arrange
    var result = Result.Success("hello");
    
    // Act
    var mappedResult = result.Map(s => s.ToUpper());
    
    // Assert
    Assert.True(mappedResult.IsSuccess);
    Assert.Equal("HELLO", mappedResult.Value);
}

[Fact]
public void Result_Bind_ShouldChainOperations()
{
    // Arrange
    var result = Result.Success("5");
    
    // Act
    var bindResult = result.Bind(s => 
        int.TryParse(s, out var number) 
            ? Result.Success(number * 2)
            : Result.Failure<int>("Invalid number"));
    
    // Assert
    Assert.True(bindResult.IsSuccess);
    Assert.Equal(10, bindResult.Value);
}
```

## Best Practices

### 1. Always Handle Both Cases

```csharp
// ✅ Good - Handle both success and failure
var result = await GetDataAsync();
if (result.IsSuccess)
{
    ProcessData(result.Value);
}
else
{
    HandleError(result.Error);
}

// ❌ Bad - Only handling success case
var result = await GetDataAsync();
if (result.IsSuccess)
{
    ProcessData(result.Value);
}
// Missing error handling
```

### 2. Use Descriptive Error Messages

```csharp
// ✅ Good - Descriptive error message
return Result.Failure<User>("User with ID 123 not found in database");

// ❌ Bad - Generic error message
return Result.Failure<User>("Error");
```

### 3. Chain Operations with Bind

```csharp
// ✅ Good - Using Bind for chaining
return await ValidateInput(request)
    .BindAsync(async validRequest => await ProcessRequestAsync(validRequest))
    .BindAsync(async processedData => await SaveResultAsync(processedData));

// ❌ Bad - Nested if statements
var validationResult = ValidateInput(request);
if (validationResult.IsSuccess)
{
    var processResult = await ProcessRequestAsync(validationResult.Value);
    if (processResult.IsSuccess)
    {
        return await SaveResultAsync(processResult.Value);
    }
    return Result.Failure<SavedData>(processResult.Error);
}
return Result.Failure<SavedData>(validationResult.Error);
```

### 4. Use Match for Clean Transformations

```csharp
// ✅ Good - Using Match
return result.Match(
    onSuccess: data => Ok(data),
    onFailure: error => BadRequest(error)
);

// ❌ Less elegant - If-else
if (result.IsSuccess)
{
    return Ok(result.Value);
}
else
{
    return BadRequest(result.Error);
}
```

## Conclusion

The Result monad pattern provides a robust, functional approach to error handling that:

- Makes error handling explicit and impossible to ignore
- Eliminates null reference exceptions in business logic
- Provides composable operations through Map and Bind
- Improves code readability and maintainability
- Enables better testing of error scenarios

By combining traditional .NET exception handling for infrastructure concerns with Result<T> for business logic, we achieve a balanced approach that leverages the best of both functional and object-oriented programming paradigms.
