# MCP - .NET 9 Clean Architecture with Functional Error Handling

A well-structured .NET 9 solution following Clean Architecture principles with modern functional programming patterns, featuring Result monad for robust error handling.

## 🏗️ Architecture

This project implements Clean Architecture with functional programming patterns:

- **Domain** (`MCP.Domain`) - Core business logic, entities, and Result monad
- **Application** (`MCP.Application`) - Use cases and business rules
- **Infrastructure** (`MCP.Infrastructure`) - External concerns with monadic error handling
- **WebApi** (`MCP.WebApi`) - REST API endpoints and presentation layer

## ✨ Key Features

- **🎯 Clean Architecture** - Separation of concerns with clear dependency boundaries
- **🔄 Result Monad Pattern** - Functional error handling without exceptions
- **📊 Structured Logging** - Comprehensive logging with ILogger<T>
- **⚙️ Configuration-Driven** - Options pattern with appsettings.json
- **🧪 Comprehensive Testing** - Unit and integration tests with high coverage
- **🔒 Type Safety** - Strong typing with nullable reference types
- **🚀 .NET 9** - Latest .NET features and performance improvements

## 📁 Project Structure

```
MCP/
├── src/
│   ├── MCP.Domain/           # Core business logic, entities, and Result monad
│   │   ├── Common/           # Result<T> monad and shared abstractions
│   │   ├── Interfaces/       # Domain service interfaces
│   │   └── Models/           # Domain entities and value objects
│   ├── MCP.Application/      # Use cases and application services
│   ├── MCP.Infrastructure/   # External dependencies with monadic error handling
│   │   ├── Constants/        # Application constants (HeaderKeys, ContentTypes)
│   │   ├── Options/          # Configuration options (CopilotServiceOptions)
│   │   └── Services/         # Service implementations with Result<T>
│   └── MCP.WebApi/          # Presentation layer (REST API)
├── tests/
│   ├── MCP.Domain.Tests/           # Unit tests for Domain
│   ├── MCP.Application.Tests/      # Unit tests for Application
│   ├── MCP.Infrastructure.Tests/   # Unit tests for Infrastructure (Result pattern)
│   └── MCP.WebApi.IntegrationTests/ # Integration tests for WebApi
├── docs/                    # Documentation
└── scripts/                 # Build and deployment scripts
```

### 🔄 Dependency Flow

```
WebApi → Infrastructure
  ↓           ↓
Application → Domain (Result<T>)
```

- **Domain**: Contains business entities, Result monad, and domain services. No external dependencies.
- **Application**: Contains use cases and interfaces. Depends only on Domain.
- **Infrastructure**: Contains implementations using Result<T> pattern. Depends on Domain and Application.
- **WebApi**: The entry point of the application. Depends on Application and Infrastructure.

## 🎯 Functional Programming Features

### Result Monad Pattern

The project uses a custom `Result<T>` monad for error handling:

```csharp
// Service methods return Result<T> instead of throwing exceptions
public async Task<Result<string>> GetCompletionAsync(string prompt, string language = "python")
{
    if (_token == null || IsTokenInvalid(_token))
    {
        var tokenResult = await GetTokenInternalAsync();
        if (tokenResult.IsFailure)
            return Result.Failure<string>($"Failed to get token: {tokenResult.Error}");
    }
    
    // ... implementation
    return Result.Success(completion);
}

// Consumers handle both success and failure cases explicitly
var result = await copilotService.GetCompletionAsync("def fibonacci(n):");
return result.Match(
    onSuccess: completion => Ok(completion),
    onFailure: error => BadRequest(error)
);
```

### Benefits of Result Pattern

- **🚫 No Exceptions**: Eliminates exception-based control flow for business logic
- **📝 Explicit Error Handling**: Callers must handle both success and failure cases
- **🔗 Composability**: Results can be chained using `Bind`, `Map`, etc.
- **🧪 Testability**: More predictable and easier to test
- **📊 Better Error Information**: Descriptive error messages with context

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Building the Solution

```bash
# Clone the repository
git clone <repository-url>
cd MCP

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run --project src/MCP.WebApi
```

### Development

#### Running in Development Mode

```bash
dotnet run --project src/MCP.WebApi --launch-profile https
```

The API will be available at:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`

#### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/MCP.Domain.Tests
```

## 📦 Dependencies

### Core Dependencies
- **.NET 9.0** - Latest .NET framework with performance improvements
- **ASP.NET Core** - Web framework for APIs
- **Microsoft.Extensions.Logging** - Structured logging with ILogger<T>
- **Microsoft.Extensions.Options** - Configuration options pattern
- **System.Text.Json** - High-performance JSON serialization

### Testing
- **xUnit** - Modern testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support
- **Microsoft.Extensions.Logging.Abstractions** - NullLogger for testing

### Development Tools
- **Nullable Reference Types** - Enhanced null safety
- **Result Monad** - Custom functional error handling
- **Options Pattern** - Configuration binding with validation

## 🛠️ Development Guidelines

### Error Handling Strategy

#### ✅ Use Result<T> for Business Logic
```csharp
// ✅ Good - Return Result<T> for operations that can fail
public async Task<Result<User>> GetUserAsync(int id)
{
    if (id <= 0)
        return Result.Failure<User>("Invalid user ID");
        
    var user = await _repository.GetAsync(id);
    return user != null 
        ? Result.Success(user)
        : Result.Failure<User>("User not found");
}
```

#### ✅ Use Exceptions for Infrastructure Issues
```csharp
// ✅ Good - Constructor validation with exceptions
public CopilotService(HttpClient httpClient, CopilotServiceOptions options, ILogger<CopilotService> logger)
{
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

### Code Organization

1. **Domain Layer**
   - Result<T> monad and common abstractions
   - Business entities and value objects
   - Domain service interfaces
   - No external dependencies

2. **Application Layer**
   - Use cases returning Result<T>
   - DTOs and mapping profiles
   - Validation with Result pattern
   - Application service interfaces

3. **Infrastructure Layer**
   - Service implementations using Result<T>
   - External API integrations with error handling
   - Configuration options and DI setup
   - Logging and monitoring

4. **WebApi Layer**
   - Controllers handling Result<T> responses
   - Middleware and exception handling
   - Configuration and DI registration
   - OpenAPI/Swagger documentation

### Naming Conventions

- Use PascalCase for classes, methods, and properties
- Use camelCase for local variables and parameters
- Use meaningful and descriptive names
- Prefix interfaces with 'I'

### Testing Strategy

#### Unit Tests with Result Pattern
```csharp
[Fact]
public async Task GetCompletionAsync_WithValidPrompt_ShouldReturnSuccess()
{
    // Arrange
    var prompt = "def fibonacci(n):";
    
    // Act
    var result = await _copilotService.GetCompletionAsync(prompt);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.Value);
}

[Fact]
public async Task GetCompletionAsync_WithInvalidToken_ShouldReturnFailure()
{
    // Arrange
    // Setup service with invalid token
    
    // Act
    var result = await _copilotService.GetCompletionAsync("test");
    
    // Assert
    Assert.True(result.IsFailure);
    Assert.Contains("token", result.Error.ToLower());
}
```

#### Testing Categories
- **Unit Tests**: Test individual components with Result<T> patterns
- **Integration Tests**: Test API endpoints and external service interactions
- **Test Coverage**: Aim for >80% code coverage with focus on error paths
- **Test Naming**: Descriptive names indicating expected behavior and outcomes

### Configuration Management

#### Options Pattern with Validation
```csharp
// appsettings.json
{
  "CopilotService": {
    "ClientId": "your-client-id",
    "UserAgent": "GitHubCopilotChat/1.0",
    "EditorVersion": "vscode/1.80.0",
    "EditorPluginVersion": "copilot-chat/0.8.0",
    "AcceptEncoding": "gzip, deflate, br"
  }
}

// Options class with validation
public class CopilotServiceOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceCodeUrl { get; set; } = string.Empty;
    public string AccessTokenUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string CompletionUrl { get; set; } = string.Empty;
}
```

## 📝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write/update tests
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support

For questions or support, please open an issue on GitHub.
