<#
.SYNOPSIS
    Verifica la implementación del Ejercicio 5 - Agente con Microsoft Agent Framework

.DESCRIPTION
    Este script verifica que:
    - El proyecto Exercise5AgentServer compila correctamente
    - La configuración está presente
    - El código conecta a los servidores MCP
    - Las dependencias están instaladas

.EXAMPLE
    .\verify-exercise5.ps1
#>

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"
$VerbosePreference = if ($Verbose) { "Continue" } else { "SilentlyContinue" }

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Verificación Ejercicio 5: Agente MAF" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$testsPassed = 0
$testsFailed = 0
$warnings = @()

# ====================================================================================
# Test 1: Verificar que el proyecto existe
# ====================================================================================

Write-Host "[Test 1] Verificando que el proyecto existe..." -NoNewline

$projectPath = "src/McpWorkshop.Servers/Exercise5AgentServer/Exercise5AgentServer.csproj"

if (Test-Path $projectPath) {
    Write-Host " ✅ PASS" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host " ❌ FAIL" -ForegroundColor Red
    Write-Host "  El proyecto no existe en: $projectPath" -ForegroundColor Red
    $testsFailed++
}

# ====================================================================================
# Test 2: Verificar dependencias NuGet
# ====================================================================================

Write-Host "[Test 2] Verificando dependencias NuGet..." -NoNewline

$requiredPackages = @(
    "Azure.AI.OpenAI",
    "Azure.Identity",
    "Microsoft.Agents.AI.OpenAI",
    "ModelContextProtocol",
    "Microsoft.Extensions.Configuration",
    "Microsoft.Extensions.Configuration.Json"
)

$csprojContent = Get-Content $projectPath -Raw
$allPackagesPresent = $true

foreach ($package in $requiredPackages) {
    if ($csprojContent -notmatch $package) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta el paquete: $package" -ForegroundColor Red
        $allPackagesPresent = $false
        break
    }
}

if ($allPackagesPresent) {
    Write-Host " ✅ PASS" -ForegroundColor Green
    $testsPassed++
} else {
    $testsFailed++
}

# ====================================================================================
# Test 3: Verificar archivos clave
# ====================================================================================

Write-Host "[Test 3] Verificando archivos clave..." -NoNewline

$requiredFiles = @(
    "src/McpWorkshop.Servers/Exercise5AgentServer/Program.cs",
    "src/McpWorkshop.Servers/Exercise5AgentServer/McpClientHelper.cs",
    "src/McpWorkshop.Servers/Exercise5AgentServer/appsettings.json"
)

$allFilesPresent = $true

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta el archivo: $file" -ForegroundColor Red
        $allFilesPresent = $false
        break
    }
}

if ($allFilesPresent) {
    Write-Host " ✅ PASS" -ForegroundColor Green
    $testsPassed++
} else {
    $testsFailed++
}

# ====================================================================================
# Test 4: Verificar configuración en appsettings.json
# ====================================================================================

Write-Host "[Test 4] Verificando configuración..." -NoNewline

$appsettingsPath = "src/McpWorkshop.Servers/Exercise5AgentServer/appsettings.json"

if (Test-Path $appsettingsPath) {
    $config = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    
    $configValid = $true
    
    if (-not $config.AzureOpenAI) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta configuración de AzureOpenAI" -ForegroundColor Red
        $configValid = $false
    }
    
    if (-not $config.McpServers) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta configuración de McpServers" -ForegroundColor Red
        $configValid = $false
    }
    
    if (-not $config.Agent) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta configuración de Agent" -ForegroundColor Red
        $configValid = $false
    }
    
    if ($configValid) {
        Write-Host " ✅ PASS" -ForegroundColor Green
        $testsPassed++
        
        # Verificar si el endpoint está configurado
        if ($config.AzureOpenAI.Endpoint -match "<your-resource>") {
            $warnings += "⚠️  Necesitas configurar tu Azure OpenAI endpoint en appsettings.json"
        }
    } else {
        $testsFailed++
    }
} else {
    Write-Host " ❌ FAIL" -ForegroundColor Red
    Write-Host "  No se encontró appsettings.json" -ForegroundColor Red
    $testsFailed++
}

# ====================================================================================
# Test 5: Verificar que el código compila
# ====================================================================================

Write-Host "[Test 5] Verificando que el proyecto compila..." -NoNewline

Push-Location "src/McpWorkshop.Servers/Exercise5AgentServer"

try {
    $buildOutput = dotnet build --configuration Release --nologo 2>&1
    $buildExitCode = $LASTEXITCODE
    
    if ($buildExitCode -eq 0) {
        Write-Host " ✅ PASS" -ForegroundColor Green
        $testsPassed++
    } else {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Error de compilación:" -ForegroundColor Red
        Write-Host $buildOutput -ForegroundColor Red
        $testsFailed++
    }
} finally {
    Pop-Location
}

# ====================================================================================
# Test 6: Verificar que los servidores MCP están disponibles
# ====================================================================================

Write-Host "[Test 6] Verificando disponibilidad de servidores MCP..." -NoNewline

$mcpServers = @(
    @{ Name = "SQL Server"; Port = 5010 },
    @{ Name = "Cosmos DB"; Port = 5011 },
    @{ Name = "REST API"; Port = 5012 }
)

$allServersRunning = $true

foreach ($server in $mcpServers) {
    try {
        $connection = Test-NetConnection -ComputerName localhost -Port $server.Port -WarningAction SilentlyContinue -ErrorAction Stop
        if (-not $connection.TcpTestSucceeded) {
            $warnings += "⚠️  Servidor MCP no está corriendo: $($server.Name) en puerto $($server.Port)"
            $allServersRunning = $false
        }
    } catch {
        $warnings += "⚠️  No se pudo verificar servidor MCP: $($server.Name) en puerto $($server.Port)"
        $allServersRunning = $false
    }
}

if ($allServersRunning) {
    Write-Host " ✅ PASS" -ForegroundColor Green
    $testsPassed++
} else {
    Write-Host " ⚠️  WARNING" -ForegroundColor Yellow
    Write-Host "  No todos los servidores MCP están corriendo" -ForegroundColor Yellow
    # No contamos esto como fallo porque pueden estar apagados intencionalmente
    $testsPassed++
}

# ====================================================================================
# Test 7: Verificar estructura del código
# ====================================================================================

Write-Host "[Test 7] Verificando estructura del código..." -NoNewline

$programContent = Get-Content "src/McpWorkshop.Servers/Exercise5AgentServer/Program.cs" -Raw

$codeChecks = @(
    "McpClientHelper.CreateHttpClientAsync",
    "ListToolsAsync",
    "AzureOpenAI",
    "Microsoft.Agents.AI"
)

$allChecksPass = $true

foreach ($check in $codeChecks) {
    if ($programContent -notmatch [regex]::Escape($check)) {
        Write-Host " ❌ FAIL" -ForegroundColor Red
        Write-Host "  Falta código esperado: $check" -ForegroundColor Red
        $allChecksPass = $false
        break
    }
}

if ($allChecksPass) {
    Write-Host " ✅ PASS" -ForegroundColor Green
    $testsPassed++
} else {
    $testsFailed++
}

# ====================================================================================
# RESUMEN
# ====================================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Tests pasados: $testsPassed" -ForegroundColor Green
Write-Host "Tests fallidos: $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })

if ($warnings.Count -gt 0) {
    Write-Host "`nAdvertencias:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  $warning" -ForegroundColor Yellow
    }
}

Write-Host "`n----------------------------------------`n"

if ($testsFailed -eq 0) {
    Write-Host "✅ ¡Ejercicio 5 verificado exitosamente!" -ForegroundColor Green
    Write-Host "`nPróximos pasos:" -ForegroundColor Cyan
    Write-Host "  1. Configura tu Azure OpenAI endpoint en appsettings.json" -ForegroundColor White
    Write-Host "  2. Asegúrate de que los 3 servidores MCP estén corriendo:" -ForegroundColor White
    Write-Host "     - dotnet run --project src/McpWorkshop.Servers/Exercise1SqlMcpServer" -ForegroundColor Gray
    Write-Host "     - dotnet run --project src/McpWorkshop.Servers/Exercise2CosmosMcpServer" -ForegroundColor Gray
    Write-Host "     - dotnet run --project src/McpWorkshop.Servers/Exercise3RestApiMcpServer" -ForegroundColor Gray
    Write-Host "  3. Ejecuta el agente:" -ForegroundColor White
    Write-Host "     dotnet run --project src/McpWorkshop.Servers/Exercise5AgentServer" -ForegroundColor Gray
    Write-Host ""
    exit 0
} else {
    Write-Host "❌ Algunos tests fallaron. Revisa los errores arriba." -ForegroundColor Red
    Write-Host "`nConsulta la documentación:" -ForegroundColor Cyan
    Write-Host "  docs/modules/11-ejercicio-5-agente-maf.md" -ForegroundColor White
    Write-Host ""
    exit 1
}
