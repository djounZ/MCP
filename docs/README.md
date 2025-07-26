# Documentation

This folder contains all project documentation for the MCP Clean Architecture project with functional programming patterns and comprehensive security. Last updated: July 26, 2025.

## Files

- **architecture.md** - Clean Architecture overview with Result monad patterns and DDD compliance
- **api.md** - Minimal API documentation and secure endpoint specifications
- **development.md** - Development setup, Result<T> patterns, and coding guidelines
- **deployment.md** - Deployment instructions and environment configuration
- **functional-patterns.md** - Result monad usage and functional programming guidelines
- **error-handling.md** - Error handling strategies and best practices
- **security.md** - Comprehensive security guide (JWT, API Keys, Rate Limiting, Authorization)
- **versioning.md** - Package versioning strategy with Directory.Build.props

## Key Topics Covered

### Architecture
- Clean Architecture implementation with proper DDD layer separation
- Dependency injection with centralized service registration
- Result monad for error handling
- Configuration management with Options pattern and Directory.Build.props

### Security
- JWT Bearer authentication with role-based authorization
- API Key authentication for service-to-service communication
- Rate limiting with tiered policies (global, API, admin)
- Authentication endpoints and token management
- Security best practices and threat mitigation

### Development
- Functional programming with Result<T>
- Minimal APIs for high-performance endpoints
- Testing strategies for monadic patterns and security
- Logging and monitoring best practices
- Code organization and naming conventions
- Centralized package version management

### API Design
- Minimal API endpoint design with security
- Error response patterns with Result<T>
- Request/response models with validation
- OpenAPI/Swagger documentation
- Authentication and authorization examples

## Getting Started

For the complete project overview, start with [architecture.md](architecture.md) to understand the overall structure and design principles.

For information about error handling and the Result pattern, see [functional-patterns.md](functional-patterns.md).

For security implementation details, see [security.md](security.md).

## Related Resources

- [Main README](../README.md) - Project overview and getting started
- [Scripts README](../scripts/README.md) - Build and deployment scripts
