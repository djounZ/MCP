# Build Scripts

This folder contains build, test, and deployment scripts for the MCP Clean Architecture project.

## Available Scripts

- **build.ps1** - PowerShell build script for Windows (includes Result<T> validation)
- **build.sh** - Bash build script for Linux/macOS
- **test.ps1** - Run all tests with coverage (unit and integration tests)
- **test.sh** - Bash version of test script
- **deploy.ps1** - Deployment script with environment validation
- **lint.ps1** - Code quality and formatting checks

## Usage

### Windows (PowerShell)
```powershell
# Build the solution
.\scripts\build.ps1

# Run all tests (including Result<T> pattern tests)
.\scripts\test.ps1

# Run only unit tests
.\scripts\test.ps1 -Category Unit

# Run with coverage report
.\scripts\test.ps1 -Coverage

# Deploy to staging
.\scripts\deploy.ps1 -Environment Staging
```

### Linux/macOS (Bash)
```bash
# Build the solution
./scripts/build.sh

# Run all tests
./scripts/test.sh

# Run only unit tests
./scripts/test.sh --filter "Category=Unit"

# Deploy to production
./scripts/deploy.sh --env production
```

## Script Features

### Build Scripts
- Solution restore and build
- NuGet package validation
- Result<T> pattern compilation checks
- Code analysis and warnings

### Test Scripts
- Unit test execution with Result<T> scenarios
- Integration test execution
- Code coverage generation
- Test result reporting
- Parallel test execution

### Deployment Scripts
- Environment-specific configuration
- Database migration (if applicable)
- Health check validation
- Rollback capabilities
