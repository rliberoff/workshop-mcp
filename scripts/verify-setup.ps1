<#
.SYNOPSIS
    Verifica que el entorno esté configurado correctamente para el taller MCP.

.DESCRIPTION
    Este script valida:
    - .NET 10.0 SDK instalado
    - PowerShell 7+ instalado
    - Puertos 5000-5003 disponibles
    - Paquetes NuGet necesarios disponibles
    - Azure CLI instalado
    - Terraform instalado
    - Git instalado

.PARAMETER Json
    Devuelve el resultado en formato JSON

.EXAMPLE
    .\verify-setup.ps1
    .\verify-setup.ps1 -Json
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$Json
)

$results = @{
    timestamp     = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    checks        = @()
    overallStatus = "PASS"
}

function Add-Check {
    param(
        [string]$Name,
        [string]$Status,
        [string]$Message,
        [string]$Version = $null,
        [bool]$Required = $true
    )

    $check = @{
        name     = $Name
        status   = $Status
        message  = $Message
        required = $Required
    }

    if ($Version) {
        $check.version = $Version
    }

    $results.checks += $check

    if ($Status -eq "FAIL" -and $Required) {
        $results.overallStatus = "FAIL"
    }

    if (-not $Json) {
        $icon = switch ($Status) {
            "PASS" { "✓" }
            "FAIL" { "✗" }
            "WARN" { "⚠" }
            default { "?" }
        }

        $color = switch ($Status) {
            "PASS" { "Green" }
            "FAIL" { "Red" }
            "WARN" { "Yellow" }
            default { "Gray" }
        }

        $reqLabel = if ($Required) { "[REQUERIDO]" } else { "[OPCIONAL]" }
        Write-Host "$icon $reqLabel $Name - $Message" -ForegroundColor $color
    }
}

# Verificar .NET SDK
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion -match '^10\.') {
        Add-Check -Name ".NET SDK" -Status "PASS" -Message "Versión correcta instalada" -Version $dotnetVersion
    }
    elseif ($dotnetVersion) {
        Add-Check -Name ".NET SDK" -Status "WARN" -Message "Versión instalada: $dotnetVersion. Se recomienda .NET 10.0" -Version $dotnetVersion
    }
    else {
        Add-Check -Name ".NET SDK" -Status "FAIL" -Message "No se encontró .NET SDK instalado"
    }
}
catch {
    Add-Check -Name ".NET SDK" -Status "FAIL" -Message "Error al verificar .NET SDK: $_"
}

# Verificar PowerShell
try {
    $psVersion = $PSVersionTable.PSVersion.ToString()
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        Add-Check -Name "PowerShell" -Status "PASS" -Message "PowerShell 7+ instalado" -Version $psVersion
    }
    else {
        Add-Check -Name "PowerShell" -Status "WARN" -Message "PowerShell $psVersion. Se recomienda PowerShell 7+" -Version $psVersion
    }
}
catch {
    Add-Check -Name "PowerShell" -Status "FAIL" -Message "Error al verificar PowerShell: $_"
}

# Verificar puertos disponibles
$portsToCheck = 5000..5003
$portsAvailable = $true
$busyPorts = @()

foreach ($port in $portsToCheck) {
    try {
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($connection) {
            $busyPorts += $port
            $portsAvailable = $false
        }
    }
    catch {
        # Puerto disponible (no hay conexiones)
    }
}

if ($portsAvailable) {
    Add-Check -Name "Puertos TCP" -Status "PASS" -Message "Puertos 5000-5003 disponibles"
}
else {
    $busyList = $busyPorts -join ", "
    Add-Check -Name "Puertos TCP" -Status "FAIL" -Message "Puertos ocupados: $busyList"
}

# Verificar NuGet packages
$packageSources = @()
try {
    $nugetSources = dotnet nuget list source 2>$null
    if ($nugetSources -match 'nuget.org') {
        Add-Check -Name "NuGet Sources" -Status "PASS" -Message "NuGet.org configurado correctamente"
        $packageSources += "nuget.org"
    }
    else {
        Add-Check -Name "NuGet Sources" -Status "WARN" -Message "Verificar configuración de NuGet.org"
    }
}
catch {
    Add-Check -Name "NuGet Sources" -Status "WARN" -Message "No se pudo verificar fuentes NuGet: $_"
}

# Verificar Azure CLI
try {
    $azVersion = az version --output json 2>$null | ConvertFrom-Json
    if ($azVersion.'azure-cli') {
        # Verificar que sea versión 2.80.0 o superior
        $versionParts = $azVersion.'azure-cli' -split '\.'
        $major = [int]$versionParts[0]
        $minor = [int]$versionParts[1]
        
        if ($major -gt 2 -or ($major -eq 2 -and $minor -ge 80)) {
            Add-Check -Name "Azure CLI" -Status "PASS" -Message "Azure CLI 2.80.0+ instalado" -Version $azVersion.'azure-cli'
        }
        else {
            Add-Check -Name "Azure CLI" -Status "WARN" -Message "Azure CLI $($azVersion.'azure-cli') instalado. Se recomienda 2.80.0+" -Version $azVersion.'azure-cli'
        }
    }
    else {
        Add-Check -Name "Azure CLI" -Status "FAIL" -Message "Azure CLI no encontrado"
    }
}
catch {
    Add-Check -Name "Azure CLI" -Status "FAIL" -Message "Azure CLI no instalado"
}

# Verificar Terraform
try {
    $tfVersion = terraform version -json 2>$null | ConvertFrom-Json
    if ($tfVersion.terraform_version) {
        # Verificar que sea versión 1.14.0 o superior
        $versionParts = $tfVersion.terraform_version -replace 'v', '' -split '\.'
        $major = [int]$versionParts[0]
        $minor = [int]$versionParts[1]
        
        if ($major -gt 1 -or ($major -eq 1 -and $minor -ge 14)) {
            Add-Check -Name "Terraform" -Status "PASS" -Message "Terraform 1.14.0+ instalado" -Version $tfVersion.terraform_version
        }
        else {
            Add-Check -Name "Terraform" -Status "WARN" -Message "Terraform $($tfVersion.terraform_version) instalado. Se recomienda 1.14.0+" -Version $tfVersion.terraform_version
        }
    }
    else {
        Add-Check -Name "Terraform" -Status "FAIL" -Message "Terraform no encontrado"
    }
}
catch {
    Add-Check -Name "Terraform" -Status "FAIL" -Message "Terraform no instalado"
}

# Verificar Git
try {
    $gitVersion = git --version 2>$null
    if ($gitVersion -match '\d+\.\d+\.\d+') {
        $version = $gitVersion -replace 'git version ', ''
        Add-Check -Name "Git" -Status "PASS" -Message "Git instalado" -Version $version
    }
    else {
        Add-Check -Name "Git" -Status "FAIL" -Message "Git no encontrado"
    }
}
catch {
    Add-Check -Name "Git" -Status "FAIL" -Message "Git no instalado"
}

# Salida final
if ($Json) {
    $results | ConvertTo-Json -Depth 10
}
else {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Estado general: $($results.overallStatus)" -ForegroundColor $(if ($results.overallStatus -eq "PASS") { "Green" } else { "Red" })
    Write-Host "========================================" -ForegroundColor Cyan

    $failedChecks = $results.checks | Where-Object { $_.status -eq "FAIL" -and $_.required }
    if ($failedChecks.Count -gt 0) {
        Write-Host "`n⚠️ ACCIONES REQUERIDAS:" -ForegroundColor Yellow
        foreach ($check in $failedChecks) {
            Write-Host "  • $($check.name): $($check.message)" -ForegroundColor Yellow
        }

        Write-Host "`nPara instalar componentes faltantes:" -ForegroundColor Cyan
        Write-Host "  .NET 10.0 SDK: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Gray
        Write-Host "  PowerShell 7+: https://aka.ms/powershell" -ForegroundColor Gray
        Write-Host "  Azure CLI: https://learn.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Gray
        Write-Host "  Terraform: https://www.terraform.io/downloads" -ForegroundColor Gray
        Write-Host "  Git: https://git-scm.com/downloads" -ForegroundColor Gray
    }
    else {
        Write-Host "`n✅ El entorno está listo para el taller MCP" -ForegroundColor Green
    }
}

# Exit code
if ($results.overallStatus -eq "PASS") {
    exit 0
}
else {
    exit 1
}
