#!/usr/bin/env pwsh

# Test script for MCP project

param(
    [string]$Configuration = "Release",
    [switch]$Coverage,
    [string]$Filter = "*"
)

$ErrorActionPreference = "Stop"

Write-Host "🧪 Running Tests for MCP Solution" -ForegroundColor Green

# Base test command
$testCommand = "dotnet test --configuration $Configuration --verbosity normal"

# Add coverage collection if requested
if ($Coverage) {
    $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory TestResults"
    Write-Host "📊 Code coverage enabled" -ForegroundColor Yellow
}

# Add filter if specified
if ($Filter -ne "*") {
    $testCommand += " --filter $Filter"
    Write-Host "🔍 Test filter: $Filter" -ForegroundColor Yellow
}

# Run tests
Write-Host "🏃 Executing tests..." -ForegroundColor Yellow
Invoke-Expression $testCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
    
    if ($Coverage) {
        Write-Host "📊 Coverage reports generated in TestResults folder" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ Some tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
