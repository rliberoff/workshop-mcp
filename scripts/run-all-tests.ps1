<#
.SYNOPSIS
    Executes all xUnit tests for the MCP Workshop project with coverage reporting.

.DESCRIPTION
    This script runs all test projects in the solution, generates code coverage reports,
    and provides a summary of test results. Used for CI/CD validation and local development.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER Coverage
    Enable code coverage collection and reporting. Default: true

.PARAMETER Filter
    Test filter expression (e.g., "FullyQualifiedName~Exercise1"). Default: run all tests

.PARAMETER Verbose
    Show detailed test output. Default: false

.PARAMETER NoBuild
    Skip building before running tests. Default: false

.EXAMPLE
    .\run-all-tests.ps1
    # Runs all tests with default settings

.EXAMPLE
    .\run-all-tests.ps1 -Configuration Release -Coverage $true
    # Runs all tests in Release mode with coverage

.EXAMPLE
    .\run-all-tests.ps1 -Filter "FullyQualifiedName~Exercise1" -Verbose
    # Runs only Exercise 1 tests with detailed output

.EXAMPLE
    .\run-all-tests.ps1 -NoBuild
    # Runs tests without rebuilding (faster for repeated runs)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter()]
    [bool]$Coverage = $true,

    [Parameter()]
    [string]$Filter = "",

    [Parameter()]
    [switch]$Verbose,

    [Parameter()]
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

# Get script directory and solution root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptDir
$TestProject = Join-Path $SolutionRoot "tests\McpWorkshop.Tests\McpWorkshop.Tests.csproj"
$CoverageDir = Join-Path $SolutionRoot "coverage"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host " 🧪 MCP Workshop - Test Execution Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verify test project exists
if (-not (Test-Path $TestProject)) {
    Write-Error "Test project not found: $TestProject"
    exit 1
}

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Solution Root: $SolutionRoot"
Write-Host "  Test Project:  $TestProject"
Write-Host "  Configuration: $Configuration"
Write-Host "  Coverage:      $Coverage"
Write-Host "  Filter:        $(if ($Filter) { $Filter } else { 'None (run all)' })"
Write-Host "  Verbose:       $Verbose"
Write-Host "  No Build:      $NoBuild"
Write-Host ""

# Build solution if not skipped
if (-not $NoBuild) {
    Write-Host "Building solution..." -ForegroundColor Yellow
    
    try {
        dotnet build $SolutionRoot -c $Configuration --nologo
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE"
            exit $LASTEXITCODE
        }
        
        Write-Host "✓ Build succeeded" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Error "Build failed: $_"
        exit 1
    }
}

# Prepare test command
$TestArgs = @(
    "test"
    $TestProject
    "-c", $Configuration
    "--no-restore"
    "--nologo"
)

if ($NoBuild) {
    $TestArgs += "--no-build"
}

if ($Filter) {
    $TestArgs += "--filter", $Filter
}

if ($Verbose) {
    $TestArgs += "--verbosity", "detailed"
}
else {
    $TestArgs += "--verbosity", "normal"
}

# Add coverage collection if enabled
if ($Coverage) {
    Write-Host "Code coverage collection enabled" -ForegroundColor Yellow
    Write-Host ""
    
    # Ensure coverage directory exists
    if (-not (Test-Path $CoverageDir)) {
        New-Item -ItemType Directory -Path $CoverageDir | Out-Null
    }
    
    $CoverageFile = Join-Path $CoverageDir "coverage.opencover.xml"
    
    $TestArgs += @(
        "--collect:XPlat Code Coverage"
        "--results-directory", $CoverageDir
        "--"
        "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"
    )
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
Write-Host ""

$TestStartTime = Get-Date

try {
    & dotnet $TestArgs
    
    $TestExitCode = $LASTEXITCODE
    $TestEndTime = Get-Date
    $TestDuration = $TestEndTime - $TestStartTime
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    
    if ($TestExitCode -eq 0) {
        Write-Host "✓ All tests passed" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Some tests failed (exit code: $TestExitCode)" -ForegroundColor Red
    }
    
    Write-Host "Test execution time: $($TestDuration.ToString('mm\:ss\.fff'))" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Generate coverage report if enabled
    if ($Coverage -and $TestExitCode -eq 0) {
        Write-Host "Generating coverage report..." -ForegroundColor Yellow
        
        # Find coverage files (they're in subdirectories with GUIDs)
        $CoverageFiles = Get-ChildItem -Path $CoverageDir -Filter "coverage.opencover.xml" -Recurse
        
        if ($CoverageFiles.Count -gt 0) {
            Write-Host "Found $($CoverageFiles.Count) coverage file(s)" -ForegroundColor Cyan
            
            # Check if reportgenerator is installed
            $ReportGeneratorInstalled = $null -ne (Get-Command reportgenerator -ErrorAction SilentlyContinue)
            
            if ($ReportGeneratorInstalled) {
                $ReportDir = Join-Path $CoverageDir "report"
                
                $CoverageFilePath = $CoverageFiles[0].FullName
                
                & reportgenerator `
                    "-reports:$CoverageFilePath" `
                    "-targetdir:$ReportDir" `
                    "-reporttypes:Html;TextSummary" `
                    "-title:MCP Workshop Coverage"
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Coverage report generated: $ReportDir\index.html" -ForegroundColor Green
                    
                    # Display text summary if available
                    $SummaryFile = Join-Path $ReportDir "Summary.txt"
                    if (Test-Path $SummaryFile) {
                        Write-Host ""
                        Write-Host "Coverage Summary:" -ForegroundColor Yellow
                        Get-Content $SummaryFile | Write-Host
                    }
                }
                else {
                    Write-Warning "Failed to generate coverage report"
                }
            }
            else {
                Write-Warning "reportgenerator not installed. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
                Write-Host "Coverage data available at: $($CoverageFiles[0].FullName)" -ForegroundColor Cyan
            }
        }
        else {
            Write-Warning "No coverage files found in $CoverageDir"
        }
    }
    
    # Summary
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  • Review test results above"
    
    if ($Coverage) {
        Write-Host "  • Open coverage report: $CoverageDir\report\index.html"
    }
    
    Write-Host "  • Fix failing tests if any"
    Write-Host "  • Run verification scripts: .\scripts\verify-exercise*.ps1"
    Write-Host ""
    
    exit $TestExitCode
}
catch {
    Write-Error "Test execution failed: $_"
    exit 1
}
