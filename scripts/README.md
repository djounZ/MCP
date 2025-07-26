# Build Scripts

This folder contains build, test, and deployment scripts for the MCP Clean Architecture project with security features. Last updated: July 26, 2025.

## Available Scripts

- **build.ps1** - PowerShell build script for Windows (includes Result<T> validation and security tests)
- **build.sh** - Bash build script for Linux/macOS
- **test.ps1** - Run all tests with coverage (unit, integration, and security tests)
- **test.sh** - Bash version of test script
- **deploy.ps1** - Deployment script with environment and security validation
- **lint.ps1** - Code quality and formatting checks
- **security-scan.ps1** - Security vulnerability scanning (NuGet packages, secrets)
- **version-update.ps1** - Update centralized package versions in Directory.Build.props

## Usage

### Windows (PowerShell)
```powershell
# Build the solution
.\scripts\build.ps1

# Run all tests (including Result<T> pattern and security tests)
.\scripts\test.ps1

# Run only unit tests
.\scripts\test.ps1 -Category Unit

# Run security-focused tests
.\scripts\test.ps1 -Category Security

# Run with coverage report
.\scripts\test.ps1 -Coverage

# Deploy to staging with security validation
.\scripts\deploy.ps1 -Environment Staging

# Run security vulnerability scan
.\scripts\security-scan.ps1

# Update package versions
.\scripts\version-update.ps1 -Package "System.IdentityModel.Tokens.Jwt" -Version "8.14.0"
```

### Linux/macOS (Bash)
```bash
# Build the solution
./scripts/build.sh

# Run all tests
./scripts/test.sh

# Run only unit tests
./scripts/test.sh --filter "Category=Unit"

# Run security tests
./scripts/test.sh --filter "Category=Security"

# Deploy to production
./scripts/deploy.sh --env production
```

## Script Features

### Build Scripts
- Solution restore and build with centralized package versions
- NuGet package validation and security scanning
- Result<T> pattern compilation checks
- JWT and authentication service validation
- Code analysis and warnings

### Test Scripts
- Unit test execution with Result<T> scenarios
- Integration test execution with security testing
- JWT authentication and authorization tests
- API key authentication tests
- Rate limiting tests
- Code coverage generation
- Test result reporting

### Security Scripts
- NuGet vulnerability scanning
- Secret detection and validation
- Authentication endpoint testing
- Authorization policy validation
- Rate limiting verification

### Deployment Scripts
- Environment-specific configuration validation
- Security configuration verification
- JWT secret key validation
- Database migration with Result<T> error handling
- Health check validation
- Parallel test execution
- Rollback capabilities

## Continuous Integration

These scripts are designed to be used with GitHub Actions, Azure DevOps, or other CI/CD platforms. Sample workflow configuration files are available in the `.github/workflows` directory.

## Related Resources

- [Main README](../README.md) - Project overview and getting started
- [Documentation](../docs/README.md) - Full project documentation
