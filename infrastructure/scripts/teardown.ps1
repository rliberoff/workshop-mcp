<#
.SYNOPSIS
    Teardown MCP Workshop infrastructure from Azure
.DESCRIPTION
    Destroys all Terraform-managed resources for cleanup after workshop sessions
.PARAMETER Environment
    Target environment to teardown (dev or prod)
.PARAMETER Force
    Skip confirmation prompt
.PARAMETER KeepLogs
    Preserve Log Analytics workspace (useful for post-workshop analysis)
.EXAMPLE
    .\teardown.ps1 -Environment dev
    .\teardown.ps1 -Environment prod -Force
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "prod")]
    [string]$Environment,

    [switch]$Force,
    [switch]$KeepLogs
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Host "========================================" -ForegroundColor Red
Write-Host "MCP Workshop - Infrastructure Teardown" -ForegroundColor Red
Write-Host "Environment: $Environment" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red

if (-not $Force) {
    Write-Warning "This will DESTROY all resources in environment: $Environment"
    Write-Host "Resources to be destroyed:" -ForegroundColor Yellow
    Write-Host "  - Resource Group: rg-mcpworkshop-$Environment" -ForegroundColor Yellow
    Write-Host "  - Container Apps (4 servers)" -ForegroundColor Yellow
    Write-Host "  - Azure SQL Database" -ForegroundColor Yellow
    Write-Host "  - Cosmos DB" -ForegroundColor Yellow
    Write-Host "  - Blob Storage" -ForegroundColor Yellow
    if (-not $KeepLogs) {
        Write-Host "  - Log Analytics Workspace" -ForegroundColor Yellow
    }
    
    $confirmation = Read-Host "`nType 'destroy' to confirm"
    if ($confirmation -ne 'destroy') {
        Write-Information "Teardown cancelled"
        exit 0
    }
}

# Navigate to Terraform directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$terraformDir = Join-Path $scriptDir "..\terraform"
Push-Location $terraformDir

try {
    Write-Information "`nInitializing Terraform..."
    terraform init
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform init failed"
    }
    
    # Build destroy command
    $destroyArgs = @(
        "destroy",
        "-var-file=environments\$Environment\terraform.tfvars"
    )
    
    if (Test-Path "environments\$Environment\secrets.auto.tfvars") {
        $destroyArgs += "-var-file=environments\$Environment\secrets.auto.tfvars"
    }
    
    if ($KeepLogs) {
        Write-Information "Preserving Log Analytics workspace..."
        $destroyArgs += "-target=module.container_apps"
        $destroyArgs += "-target=module.sql_database"
        $destroyArgs += "-target=module.cosmos_db"
        $destroyArgs += "-target=module.storage"
    }
    
    if ($Force) {
        $destroyArgs += "-auto-approve"
    }
    
    Write-Information "`nDestroying infrastructure..."
    & terraform $destroyArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform destroy failed"
    }
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Teardown Successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    if ($KeepLogs) {
        Write-Information "Log Analytics workspace preserved for analysis"
    }
    
    # Clean up local files
    $planFiles = Get-ChildItem -Path . -Filter "terraform-*.tfplan"
    if ($planFiles) {
        Write-Information "`nCleaning up plan files..."
        $planFiles | Remove-Item -Force
    }
    
    Write-Host "`nCleanup complete. All resources destroyed." -ForegroundColor Green
}
catch {
    Write-Error "Teardown failed: $_"
    Write-Host "`nManual cleanup may be required. Check Azure Portal:" -ForegroundColor Yellow
    Write-Host "  Resource Group: rg-mcpworkshop-$Environment" -ForegroundColor Yellow
    exit 1
}
finally {
    Pop-Location
}
