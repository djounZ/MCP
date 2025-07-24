# Architecture Overview

## Clean Architecture

This project follows Clean Architecture principles with four main layers:

### 1. Domain Layer (`MCP.Domain`)

The innermost layer containing:
- **Entities**: Core business objects with identity
- **Value Objects**: Objects without identity that describe characteristics
- **Domain Services**: Business logic that doesn't naturally fit in entities
- **Repository Interfaces**: Abstractions for data access
- **Domain Exceptions**: Business-specific exceptions

**Dependencies**: None (Pure business logic)

### 2. Application Layer (`MCP.Application`)

Contains use cases and orchestrates the domain:
- **Use Cases/Services**: Application-specific business rules
- **DTOs**: Data Transfer Objects for API contracts
- **Interfaces**: Abstractions for external services
- **Commands/Queries**: CQRS pattern implementation
- **Validators**: Input validation logic

**Dependencies**: Domain Layer only

### 3. Infrastructure Layer (`MCP.Infrastructure`)

Implements external concerns:
- **Repositories**: Data access implementations
- **External Services**: API clients, email services, etc.
- **Database Context**: Entity Framework or other ORM setup
- **Configuration**: Dependency injection setup

**Dependencies**: Domain and Application layers

### 4. Presentation Layer (`MCP.WebApi`)

The entry point and API interface:
- **Controllers**: HTTP endpoints
- **Middleware**: Cross-cutting concerns (logging, error handling)
- **Filters**: Request/response processing
- **Configuration**: Startup and program setup

**Dependencies**: Application and Infrastructure layers

## Key Principles

1. **Dependency Inversion**: Higher-level modules don't depend on lower-level modules
2. **Single Responsibility**: Each class has one reason to change
3. **Interface Segregation**: Depend on abstractions, not concretions
4. **Don't Repeat Yourself (DRY)**: Reduce code duplication

## Data Flow

```
Request → Controller → Application Service → Domain → Repository Interface
   ↓                                                         ↓
Response ← DTO ← Domain Object ← Infrastructure Repository Implementation
```

This architecture ensures:
- **Testability**: Easy to unit test with mocked dependencies
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Easy to swap implementations
- **Scalability**: Well-organized for team development
