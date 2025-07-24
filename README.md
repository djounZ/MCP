# MCP - .NET 9 Clean Architecture Template

A well-structured .NET 9 solution following Clean Architecture principles and industry best practices.

## 🏗️ Architecture

This project implements Clean Architecture with the following layers:

- **Domain** (`MCP.Domain`) - Core business logic and entities
- **Application** (`MCP.Application`) - Use cases and business rules
- **Infrastructure** (`MCP.Infrastructure`) - External concerns (data access, APIs)
- **WebApi** (`MCP.WebApi`) - REST API endpoints and presentation layer

## 📁 Project Structure

```
MCP/
├── src/
│   ├── MCP.Domain/           # Core business logic and entities
│   ├── MCP.Application/      # Use cases and application services
│   ├── MCP.Infrastructure/   # External dependencies (DB, APIs, etc.)
│   └── MCP.WebApi/          # Presentation layer (REST API)
├── tests/
│   ├── MCP.Domain.Tests/           # Unit tests for Domain
│   ├── MCP.Application.Tests/      # Unit tests for Application
│   ├── MCP.Infrastructure.Tests/   # Unit tests for Infrastructure
│   └── MCP.WebApi.IntegrationTests/ # Integration tests for WebApi
├── docs/                    # Documentation
└── scripts/                 # Build and deployment scripts
```

### 🔄 Dependency Flow

```
WebApi → Infrastructure
  ↓           ↓
Application → Domain
```

- **Domain**: Contains business entities, value objects, and domain services. No dependencies on other layers.
- **Application**: Contains use cases, interfaces, and application services. Depends only on Domain.
- **Infrastructure**: Contains implementations of external concerns. Depends on Domain and Application.
- **WebApi**: The entry point of the application. Depends on Application and Infrastructure.

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
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web framework for APIs

### Testing
- **xUnit** - Testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

## 🛠️ Development Guidelines

### Code Organization

1. **Domain Layer**
   - Entities and value objects
   - Domain services and specifications
   - Repository interfaces
   - Domain events

2. **Application Layer**
   - Use cases (commands/queries)
   - DTOs and mapping profiles
   - Validation rules
   - Application services

3. **Infrastructure Layer**
   - Data access implementations
   - External service integrations
   - Configuration and dependency injection

4. **WebApi Layer**
   - Controllers
   - Middleware
   - Configuration
   - Program.cs setup

### Naming Conventions

- Use PascalCase for classes, methods, and properties
- Use camelCase for local variables and parameters
- Use meaningful and descriptive names
- Prefix interfaces with 'I'

### Testing Strategy

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test API endpoints and database interactions
- **Test Coverage**: Aim for >80% code coverage
- **Test Naming**: Use descriptive test method names

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
