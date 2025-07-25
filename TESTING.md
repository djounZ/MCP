# Testing Guide

This project separates unit tests from integration tests using test categories.

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only (Fast, no external dependencies)

```bash
dotnet test --filter "Category=Unit"
```

### Run Integration Tests Only (Requires GitHub Copilot authentication)

```bash
dotnet test --filter "Category=Integration"
```

## Integration Test Requirements

The CopilotService integration tests require:

1. **GitHub Copilot subscription** - You need an active GitHub Copilot subscription
2. **Authentication setup** - The first time you run integration tests, you'll need to:
    - Visit the GitHub authentication URL shown in the console
    - Enter the device code to authenticate
    - The authentication token will be saved locally

### Example Integration Test

The `GetCompletionAsync_WithNapoleonPrompt_ShouldReturnNonEmptyResponse` test:

- Sends the prompt: "Hello, who was Napoleon?"
- Verifies the response is not empty
- Displays the response in the test output
- Validates the response contains relevant content

## Test Structure

```
tests/
├── MCP.Infrastructure.Tests/
│   ├── CopilotServiceIntegrationTests.cs  # Integration tests (Category=Integration)
│   └── UnitTest1.cs                       # Unit tests (Category=Unit)
└── Other test projects...
```

## Important Notes

- **Unit tests** run quickly and don't require external services
- **Integration tests** require network access and GitHub Copilot authentication
- Integration tests may take longer to complete
- Always run unit tests first during development
- Run integration tests before major releases or deployments
