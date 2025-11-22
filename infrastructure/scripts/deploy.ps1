<#
.SYNOPSIS
    Deploy MCP Workshop infrastructure to Azure using Terraform
.DESCRIPTION
    Orchestrates Terraform deployment with validation checks and automated secrets generation
.PARAMETER Environment
    Target environment (defaults to 'workshop')
.PARAMETER AutoApprove
    Skip interactive approval of Terraform plan
.PARAMETER SkipValidation
    Skip pre-deployment validation checks
.EXAMPLE
    .\deploy.ps1
    .\deploy.ps1 -AutoApprove
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("workshop")]
    [string]$Environment = "workshop",

    [switch]$AutoApprove,
    [switch]$SkipValidation
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MCP Workshop - Azure Infrastructure Deployment" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Validation Checks
if (-not $SkipValidation) {
    Write-Information "Running pre-deployment validation..."
    
    # Check Terraform installation
    try {
        $tfVersion = terraform version
        Write-Information "✓ Terraform found: $($tfVersion[0])"
    }
    catch {
        Write-Error "Terraform not found. Install from https://www.terraform.io/downloads"
        exit 1
    }
    
    # Check Azure CLI installation
    try {
        $azVersion = az version --output json | ConvertFrom-Json
        Write-Information "✓ Azure CLI found: $($azVersion.'azure-cli')"
    }
    catch {
        Write-Error "Azure CLI not found. Install from https://docs.microsoft.com/cli/azure/install-azure-cli"
        exit 1
    }
    
    # Check Azure authentication
    try {
        $account = az account show | ConvertFrom-Json
        Write-Information "✓ Logged in to Azure as: $($account.user.name)"
        Write-Information "  Subscription: $($account.name) ($($account.id))"
    }
    catch {
        Write-Error "Not authenticated to Azure. Run: az login"
        exit 1
    }
}

# Navigate to Terraform directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$terraformDir = Join-Path $scriptDir "..\terraform"
Push-Location $terraformDir

try {
    # Generate secrets if not provided
    $secretsFile = "environments\$Environment\secrets.auto.tfvars"
    if (-not (Test-Path $secretsFile)) {
        Write-Information "Generating secrets file: $secretsFile"
        
        $jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object { [char]$_ })
        $sqlPassword = -join ((65..90) + (97..122) + (48..57) + (33, 35, 36, 37, 38, 42, 43, 45, 61) | Get-Random -Count 20 | ForEach-Object { [char]$_ })
        
        $secretsContent = @"
# Auto-generated secrets - DO NOT COMMIT TO GIT
# Generated on: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

jwt_secret          = "$jwtSecret"
sql_admin_password  = "$sqlPassword"

# Azure AD Configuration (optional - uncomment and update with your values if needed)
# azuread_admin_login     = "your-email@domain.com"
# azuread_admin_object_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
"@
        
        $secretsContent | Out-File -FilePath $secretsFile -Encoding UTF8
        Write-Information "✓ Secrets generated. Update Azure AD values in: $secretsFile"
    }
    
    # Terraform Init
    Write-Information "`nInitializing Terraform..."
    terraform init -upgrade
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform init failed"
    }
    
    # Terraform Validate
    Write-Information "`nValidating Terraform configuration..."
    terraform validate
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform validation failed"
    }
    
    # Terraform Plan
    Write-Information "`nGenerating Terraform plan..."
    $planFile = "terraform-$Environment.tfplan"
    terraform plan "-var-file=environments\$Environment\terraform.tfvars" "-var-file=environments\$Environment\secrets.auto.tfvars" "-out=$planFile"
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform plan failed"
    }
    
    # Terraform Apply
    if ($AutoApprove) {
        Write-Information "`nApplying Terraform plan (auto-approved)..."
        terraform apply -auto-approve $planFile
    }
    else {
        Write-Host "`nReview the plan above. Continue with deployment?" -ForegroundColor Yellow
        $confirmation = Read-Host "Type 'yes' to proceed"
        if ($confirmation -eq 'yes') {
            terraform apply $planFile
        }
        else {
            Write-Warning "Deployment cancelled by user"
            exit 0
        }
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "Terraform apply failed"
    }
    
    # Output deployment summary
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Deployment Successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    Write-Information "`nRetrieving deployment outputs..."
    terraform output -json | ConvertFrom-Json | ConvertTo-Json -Depth 10
    
    # Save outputs to file
    $outputsFile = "outputs-$Environment.json"
    terraform output -json | Out-File -FilePath $outputsFile -Encoding UTF8
    Write-Information "✓ Outputs saved to: $outputsFile"
    
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Review outputs above or in $outputsFile" -ForegroundColor White
    Write-Host "2. Build and push container images to Azure Container Registry" -ForegroundColor White
    Write-Host "3. Update Container Apps with new image versions" -ForegroundColor White
    Write-Host "4. Test MCP servers with verification scripts" -ForegroundColor White
}
catch {
    Write-Error "Deployment failed: $_"
    exit 1
}
finally {
    Pop-Location
}
