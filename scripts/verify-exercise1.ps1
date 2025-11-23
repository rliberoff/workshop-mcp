# Script de Verificación: Ejercicio 1 - Servidor de Recursos Estáticos
# Basado en: contracts/exercise-1-static-resource.json

param(
    [string]$ServerUrl = "http://localhost:5001"
)

$ErrorActionPreference = "Stop"
$testsPassed = 0
$testsFailed = 0

function Invoke-McpRequest {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [int]$Id = 1
    )

    $body = @{
        jsonrpc = "2.0"
        method  = $Method
        params  = $Params
        id      = $Id
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$ServerUrl/mcp" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        return $response
    }
    catch {
        Write-Host "❌ Error en request $Method`: $_" -ForegroundColor Red
        return $null
    }
}

function Test-UninitializedAccess {
    Write-Host "`n🧪 Test 1: Verificar error sin Initialize" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "resources/read" -Params @{ uri = "mcp://customers" }

    if ($response -and $response.error) {
        Write-Host "✅ Uninitialized Access: PASS" -ForegroundColor Green
        Write-Host "   Error detectado correctamente: $($response.error.message)" -ForegroundColor Gray
        return $true
    }
    elseif ($response -and $response.result) {
        Write-Host "❌ Uninitialized Access: FAIL (se permitió acceso sin initialize)" -ForegroundColor Red
        Write-Host "   El servidor debería rechazar requests antes de initialize" -ForegroundColor Yellow
        return $false
    }
    else {
        Write-Host "❌ Uninitialized Access: FAIL (respuesta inesperada)" -ForegroundColor Red
        return $false
    }
}

function Test-Initialize {
    Write-Host "`n🧪 Test 2: Initialize" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "initialize" -Params @{
        protocolVersion = "2024-11-05"
        clientInfo      = @{
            name    = "verify-exercise1"
            version = "1.0.0"
        }
    }

    if ($response -and $response.result.serverInfo.name -like "Exercise1*") {
        Write-Host "✅ Initialize: PASS" -ForegroundColor Green
        Write-Host "   Server: $($response.result.serverInfo.name) v$($response.result.serverInfo.version)" -ForegroundColor Gray
        return $true
    }
    else {
        Write-Host "❌ Initialize: FAIL" -ForegroundColor Red
        return $false
    }
}

function Test-ResourcesList {
    Write-Host "`n🧪 Test 3: Resources/List" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "resources/list"

    if ($response -and $response.result.resources) {
        $resourceCount = $response.result.resources.Count
        
        if ($resourceCount -ge 2) {
            Write-Host "✅ Resources/List: PASS" -ForegroundColor Green
            Write-Host "   Found $resourceCount resources:" -ForegroundColor Gray
            
            foreach ($resource in $response.result.resources) {
                Write-Host "   - $($resource.uri): $($resource.name)" -ForegroundColor Gray
            }
            
            return $true
        }
        else {
            Write-Host "❌ Resources/List: FAIL (expected ≥2 resources, got $resourceCount)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ Resources/List: FAIL (no resources returned)" -ForegroundColor Red
        return $false
    }
}

function Test-ResourceReadCustomers {
    Write-Host "`n🧪 Test 4: Resources/Read - Customers" -ForegroundColor Cyan
    
    $startTime = Get-Date
    $response = Invoke-McpRequest -Method "resources/read" -Params @{ uri = "mcp://customers" }
    $duration = (Get-Date) - $startTime

    if ($response -and $response.result.contents) {
        $content = $response.result.contents[0]
        $data = $content.text | ConvertFrom-Json
        
        if ($data.Count -ge 3 -and $duration.TotalMilliseconds -lt 500) {
            Write-Host "✅ Resources/Read Customers: PASS" -ForegroundColor Green
            Write-Host "   Customers: $($data.Count)" -ForegroundColor Gray
            Write-Host "   Response time: $([math]::Round($duration.TotalMilliseconds, 2))ms" -ForegroundColor Gray
            Write-Host "   Sample: $($data[0].name) - $($data[0].city)" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ Resources/Read Customers: FAIL" -ForegroundColor Red
            if ($data.Count -lt 3) {
                Write-Host "   Expected ≥3 customers, got $($data.Count)" -ForegroundColor Red
            }
            if ($duration.TotalMilliseconds -ge 500) {
                Write-Host "   Response time too slow: $([math]::Round($duration.TotalMilliseconds, 2))ms" -ForegroundColor Red
            }
            return $false
        }
    }
    else {
        Write-Host "❌ Resources/Read Customers: FAIL (no content returned)" -ForegroundColor Red
        return $false
    }
}

function Test-ResourceReadProducts {
    Write-Host "`n🧪 Test 5: Resources/Read - Products" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "resources/read" -Params @{ uri = "mcp://products" }

    if ($response -and $response.result.contents) {
        $content = $response.result.contents[0]
        $data = $content.text | ConvertFrom-Json
        
        if ($data.Count -ge 3) {
            Write-Host "✅ Resources/Read Products: PASS" -ForegroundColor Green
            Write-Host "   Products: $($data.Count)" -ForegroundColor Gray
            Write-Host "   Sample: $($data[0].name) - $($data[0].price)€" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ Resources/Read Products: FAIL (expected ≥3 products, got $($data.Count))" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ Resources/Read Products: FAIL (no content returned)" -ForegroundColor Red
        return $false
    }
}

# Ejecutar tests
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " 📦 Verificación: Ejercicio 1 - Static Resources" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Server: $ServerUrl" -ForegroundColor Gray

try {
    Write-Host "`n🔍 Verificando conectividad..." -ForegroundColor Yellow
    $null = Invoke-WebRequest -Uri $ServerUrl -Method GET -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ Servidor accesible" -ForegroundColor Green
}
catch {
    Write-Host "❌ Servidor no accesible en $ServerUrl" -ForegroundColor Red
    Write-Host "   Asegúrate de que el servidor está ejecutándose con 'dotnet run'" -ForegroundColor Yellow
    exit 1
}

# Ejecutar tests
if (Test-UninitializedAccess) { $testsPassed++ } else { $testsFailed++ }
if (Test-Initialize) { $testsPassed++ } else { $testsFailed++ }
if (Test-ResourcesList) { $testsPassed++ } else { $testsFailed++ }
if (Test-ResourceReadCustomers) { $testsPassed++ } else { $testsFailed++ }
if (Test-ResourceReadProducts) { $testsPassed++ } else { $testsFailed++ }

# Resumen
Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Resumen de Tests" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Passed: $testsPassed" -ForegroundColor Green
Write-Host "❌ Failed: $testsFailed" -ForegroundColor Red
Write-Host "Total: $($testsPassed + $testsFailed)" -ForegroundColor Gray

if ($testsFailed -eq 0) {
    Write-Host "`n🎉 ¡Todos los tests pasaron! Ejercicio 1 completado." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n⚠️  Algunos tests fallaron. Revisa la implementación." -ForegroundColor Yellow
    exit 1
}
