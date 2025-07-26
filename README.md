# MCP - .NET 9 Clean Architecture with Functional Error Handling & Security

A production-ready .NET 9 solution following Clean Architecture principles with modern functional programming patterns,
comprehensive security, and minimal APIs for high performance. Updated: July 26, 2025.

## 🏗️ Architecture

This project implements Clean Architecture with functional programming patterns and modern security:

- **Domain** (`MCP.Domain`) - Core business logic, entities, and Result monad
- **Application** (`MCP.Application`) - Use cases, business rules, and service interfaces
- **Infrastructure** (`MCP.Infrastructure`) - External concerns with monadic error handling
- **WebApi** (`MCP.WebApi`) - Minimal API endpoints with JWT authentication

## ✨ Key Features

- **🎯 Clean Architecture** - Separation of concerns with clear dependency boundaries
- **🔄 Result Monad Pattern** - Functional error handling without exceptions
- **🔐 Comprehensive Security** - JWT Bearer + API Key authentication, role-based authorization
- **⚡ Minimal APIs** - High-performance endpoint routing for better throughput
- **🛡️ Rate Limiting** - Built-in protection with tiered policies (global, API, admin)
- **📊 Structured Logging** - Comprehensive logging with ILogger<T>
- **⚙️ Configuration-Driven** - Options pattern with centralized package versioning
- **🧪 Comprehensive Testing** - Unit and integration tests with high coverage
- **🔒 Type Safety** - Strong typing with nullable reference types
- **🚀 .NET 9** - Latest .NET features and performance improvements
- **🔍 API Documentation** - OpenAPI/Swagger with security definitions

## 📁 Project Structure

```
MCP/
├── src/
│   ├── MCP.Domain/           # Core business logic, entities, and Result monad
│   │   ├── Common/           # Result<T> monad and shared abstractions
│   │   ├── Interfaces/       # Domain service interfaces
│   │   └── Models/           # Domain entities and value objects
│   ├── MCP.Application/      # Use cases and application services
│   │   ├── Interfaces/       # Service contracts (IJwtTokenService, etc.)
│   │   ├── Commands/         # CQRS command handlers
│   │   └── Queries/          # CQRS query handlers
│   ├── MCP.Infrastructure/   # External dependencies with monadic error handling
│   │   ├── Configuration/    # Centralized DI extensions
│   │   ├── Services/         # Service implementations (JWT, Copilot)
│   │   ├── Authentication/   # API Key authentication handlers
│   │   └── Options/          # Configuration options classes
│   └── MCP.WebApi/          # Minimal API endpoints with security
│       ├── Extensions/       # Security, auth, and endpoint extensions
│       └── Authentication/   # JWT and API key authentication
├── tests/
│   ├── MCP.Domain.Tests/           # Unit tests for Domain
│   ├── MCP.Application.Tests/      # Unit tests for Application
│   ├── MCP.Infrastructure.Tests/   # Unit tests for Infrastructure (Result pattern)
│   └── MCP.WebApi.IntegrationTests/ # Integration tests for WebApi
├── docs/                    # Documentation (including security.md)
├── scripts/                 # Build and deployment scripts
└── Directory.Build.props    # Centralized package version management
```

### 🔄 Dependency Flow

```
WebApi → Application + Infrastructure
Infrastructure → Domain + Application
Application → Domain
Domain → (no dependencies)
```

- **Domain**: Contains business entities, Result monad, and domain services. No external dependencies.
- **Application**: Contains use cases, service interfaces (IJwtTokenService), and business rules. Depends only on
  Domain.
- **Infrastructure**: Contains service implementations using Result<T> pattern. Depends on Domain and Application.
- **WebApi**: Minimal API endpoints with security. Depends on Application and Infrastructure.

## 🔐 Security Features

### Authentication & Authorization

- **JWT Bearer Authentication** - Stateless token-based authentication
- **API Key Authentication** - Header-based authentication for service-to-service calls
- **Role-Based Authorization** - Fine-grained access control with Admin/User/Guest roles
- **Rate Limiting** - Built-in protection with configurable policies:
    - Global: 100 requests/minute per IP
    - API endpoints: 60 requests/minute per user
    - Admin endpoints: 20 requests/minute per user

### Security Configuration

```csharp
// JWT Authentication
services.AddJwtAuthentication(configuration);

// API Key Authentication  
services.AddApiKeyAuthentication(configuration);

// Role-based Authorization
services.AddCustomAuthorization();

// Rate Limiting with tiered policies
services.AddCustomRateLimiting();
```

### Authentication Endpoints

- `POST /api/auth/login` - Login with username/password → JWT token
- `POST /api/auth/validate` - Validate JWT token
- `POST /api/auth/refresh` - Refresh JWT token

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

## ⚡ Minimal APIs

The project uses .NET 9 minimal APIs for high performance:

```csharp
// Traditional controller approach replaced with extension methods
public static class CopilotEndpointsExtensions
{
    public static WebApplication MapCopilotEndpoints(this WebApplication app)
    {
        app.MapPost("/api/copilot/completion", 
            [Authorize(Roles = "Admin,User")] async (
                CompletionRequest request, 
                ICopilotService copilotService) =>
        {
            var result = await copilotService.GetCompletionAsync(request.Prompt, request.Language);
            return result.Match(
                onSuccess: completion => Results.Ok(new { completion }),
                onFailure: error => Results.BadRequest(new { error })
            );
        })
        .RequireRateLimiting("ApiPolicy")
        .WithName("GetCompletion")
        .WithOpenApi();
        
        return app;
    }
}
```

### Benefits of Minimal APIs

- **⚡ Higher Performance** - Reduced overhead and faster throughput
- **🎯 Focused Endpoints** - Clear, purpose-built endpoint definitions
- **🔧 Better DI Integration** - Direct parameter injection
- **📝 Simplified Testing** - Easier to test individual endpoints

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (latest version)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) for command-line operations

### Building the Solution

```bash
# Clone the repository
git clone https://github.com/djounZ/MCP.git
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
- **ASP.NET Core 9.0** - Web framework for minimal APIs
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **System.IdentityModel.Tokens.Jwt** - JWT token handling
- **Microsoft.Extensions.Logging** - Structured logging with ILogger<T>
- **Microsoft.Extensions.Options** - Configuration options pattern
- **System.Text.Json** - High-performance JSON serialization
- **Microsoft.AspNetCore.OpenApi** - OpenAPI documentation

### Security Dependencies

- **Microsoft.IdentityModel.Tokens** - Token validation and security
- **Rate Limiting** - Built-in .NET 9 rate limiting middleware
- **Authentication Middleware** - Custom API key authentication handlers
- **Microsoft.AspNetCore.Authorization** - Role-based authorization

### Testing

- **xUnit** - Modern testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support
- **Microsoft.Extensions.Logging.Abstractions** - NullLogger for testing
- **FluentAssertions** - Expressive assertion library

### Development Tools

- **Directory.Build.props** - Centralized package version management
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
    - Service interfaces (IJwtTokenService, ICopilotService)
    - DTOs and mapping profiles
    - Validation with Result pattern

3. **Infrastructure Layer**
    - Service implementations using Result<T>
    - JWT token service implementation
    - External API integrations with error handling
    - Configuration options and centralized DI setup

4. **WebApi Layer**
    - Minimal API endpoints with security
    - JWT and API key authentication
    - Rate limiting and CORS configuration
    - Extension methods for endpoint registration

### Package Management

All package versions are centralized in `Directory.Build.props`:

```xml
<PropertyGroup>
  <!-- JWT & Auth packages -->
  <SystemIdentityModelTokensJwtVersion>8.13.0</SystemIdentityModelTokensJwtVersion>
  <MicrosoftIdentityModelTokensVersion>8.13.0</MicrosoftIdentityModelTokensVersion>
  <MicrosoftAspNetCoreAuthenticationJwtBearerVersion>9.0.0</MicrosoftAspNetCoreAuthenticationJwtBearerVersion>
</PropertyGroup>
```

Projects reference versions using MSBuild properties:

```xml
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="$(SystemIdentityModelTokensJwtVersion)" />
```

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

#### Integration Tests with Security

```csharp
[Fact]
public async Task GetCompletion_WithValidJwtToken_ShouldReturnSuccess()
{
    // Arrange
    var token = GenerateTestJwtToken("testuser", ["User"]);
    _client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    
    var request = new { prompt = "def fibonacci(n):", language = "python" };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/copilot/completion", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("fibonacci", content);
}

[Fact]
public async Task GetCompletion_WithInvalidApiKey_ShouldReturnUnauthorized()
{
    // Arrange
    _client.DefaultRequestHeaders.Add("X-API-Key", "invalid-key");
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/copilot/completion", new { });
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

### Configuration Management

#### Security Configuration

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-here-must-be-long-enough",
    "Issuer": "MCP-API",
    "Audience": "MCP-Client",
    "ExpiryMinutes": 60
  },
  "ApiKeys": {
    "ValidKeys": [
      {
        "Key": "api-key-1",
        "UserName": "service-user-1",
        "Roles": ["User"]
      },
      {
        "Key": "admin-api-key",
        "UserName": "admin-service",
        "Roles": ["Admin", "User"]
      }
    ]
  },
  "CopilotService": {
    "ClientId": "your-client-id",
    "UserAgent": "GitHubCopilotChat/1.0",
    "EditorVersion": "vscode/1.80.0",
    "EditorPluginVersion": "copilot-chat/0.8.0"
  }
}
```

#### Rate Limiting Configuration

```json
{
  "RateLimiting": {
    "GlobalPolicy": {
      "PermitLimit": 100,
      "Window": "00:01:00"
    },
    "ApiPolicy": {
      "PermitLimit": 60,
      "Window": "00:01:00"
    },
    "AdminPolicy": {
      "PermitLimit": 20,
      "Window": "00:01:00"
    }
  }
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

## 📚 Documentation

For detailed documentation, see the [docs](docs/) directory:

- [Architecture Overview](docs/architecture.md)
- [Functional Programming Patterns](docs/functional-patterns.md)
- [Security Guide](docs/security.md)

## 🤝 Support

For questions or support, please open an issue on GitHub.
