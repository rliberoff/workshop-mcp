<#
.SYNOPSIS
    Verifica que el entorno esté configurado correctamente para el taller MCP.

.DESCRIPTION
    Este script valida:
    - .NET 10.0 SDK instalado
    - PowerShell 7+ instalado
    - Puertos 5000-5003 disponibles
    - Paquetes NuGet necesarios disponibles
    - Azure CLI instalado (opcional)
    - Terraform instalado (opcional)

.PARAMETER Json
    Devuelve el resultado en formato JSON

.PARAMETER IncludeOptional
    Incluye verificaciones opcionales (Azure CLI, Terraform)

.EXAMPLE
    .\verify-setup.ps1
    .\verify-setup.ps1 -Json
    .\verify-setup.ps1 -IncludeOptional
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$Json,

    [Parameter()]
    [switch]$IncludeOptional
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

# Verificar ModelContextProtocol package (búsqueda online)
try {
    $searchResult = dotnet nuget search ModelContextProtocol --prerelease --take 1 2>$null
    if ($searchResult -match 'ModelContextProtocol') {
        Add-Check -Name "Paquete ModelContextProtocol" -Status "PASS" -Message "Paquete disponible en NuGet.org"
    }
    else {
        Add-Check -Name "Paquete ModelContextProtocol" -Status "WARN" -Message "No se encontró en búsqueda. Verificar conectividad a NuGet.org"
    }
}
catch {
    Add-Check -Name "Paquete ModelContextProtocol" -Status "WARN" -Message "No se pudo verificar disponibilidad del paquete"
}

# Verificaciones opcionales
if ($IncludeOptional) {
    # Verificar Azure CLI
    try {
        $azVersion = az version --output json 2>$null | ConvertFrom-Json
        if ($azVersion.'azure-cli') {
            Add-Check -Name "Azure CLI" -Status "PASS" -Message "Azure CLI instalado" -Version $azVersion.'azure-cli' -Required $false
        }
        else {
            Add-Check -Name "Azure CLI" -Status "WARN" -Message "Azure CLI no encontrado" -Required $false
        }
    }
    catch {
        Add-Check -Name "Azure CLI" -Status "WARN" -Message "Azure CLI no instalado (opcional para despliegue)" -Required $false
    }

    # Verificar Terraform
    try {
        $tfVersion = terraform version -json 2>$null | ConvertFrom-Json
        if ($tfVersion.terraform_version) {
            Add-Check -Name "Terraform" -Status "PASS" -Message "Terraform instalado" -Version $tfVersion.terraform_version -Required $false
        }
        else {
            Add-Check -Name "Terraform" -Status "WARN" -Message "Terraform no encontrado" -Required $false
        }
    }
    catch {
        Add-Check -Name "Terraform" -Status "WARN" -Message "Terraform no instalado (opcional para infraestructura)" -Required $false
    }

    # Verificar Git
    try {
        $gitVersion = git --version 2>$null
        if ($gitVersion -match '\d+\.\d+\.\d+') {
            $version = $gitVersion -replace 'git version ', ''
            Add-Check -Name "Git" -Status "PASS" -Message "Git instalado" -Version $version -Required $false
        }
        else {
            Add-Check -Name "Git" -Status "WARN" -Message "Git no encontrado" -Required $false
        }
    }
    catch {
        Add-Check -Name "Git" -Status "WARN" -Message "Git no instalado (opcional)" -Required $false
    }
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
