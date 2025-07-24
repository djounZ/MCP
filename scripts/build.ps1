#!/usr/bin/env pwsh

# Build script for MCP project

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "artifacts",
    [switch]$SkipTests,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "🏗️  Building MCP Solution" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning solution..." -ForegroundColor Yellow
    dotnet clean
    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Recurse -Force
    }
}

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

# Run tests unless skipped
if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --collect:"XPlat Code Coverage"
}

# Publish API
Write-Host "📦 Publishing Web API..." -ForegroundColor Yellow
dotnet publish src/MCP.WebApi/MCP.WebApi.csproj --configuration $Configuration --output "$OutputPath/webapi" --no-build

Write-Host "✅ Build completed successfully!" -ForegroundColor Green
