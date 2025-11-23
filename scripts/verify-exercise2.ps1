# Script de Verificación: Ejercicio 2 - Herramientas Paramétricas
# Basado en: contracts/exercise-2-parametric-query.json

param(
    [string]$ServerUrl = "http://localhost:5002"
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
    } | ConvertTo-Json -Depth 10

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
    
    $response = Invoke-McpRequest -Method "tools/call" -Params @{
        name      = "search_customers"
        arguments = @{ name = "Test" }
    }

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
            name    = "verify-exercise2"
            version = "1.0.0"
        }
    }

    if ($response -and $response.result.serverInfo.name -like "Exercise2*") {
        Write-Host "✅ Initialize: PASS" -ForegroundColor Green
        Write-Host "   Server: $($response.result.serverInfo.name) v$($response.result.serverInfo.version)" -ForegroundColor Gray
        return $true
    }
    else {
        Write-Host "❌ Initialize: FAIL" -ForegroundColor Red
        return $false
    }
}

function Test-ToolsList {
    Write-Host "`n🧪 Test 3: Tools/List" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "tools/list"

    if ($response -and $response.result.tools) {
        $toolCount = $response.result.tools.Count
        
        if ($toolCount -ge 3) {
            Write-Host "✅ Tools/List: PASS" -ForegroundColor Green
            Write-Host "   Found $toolCount tools:" -ForegroundColor Gray
            
            foreach ($tool in $response.result.tools) {
                Write-Host "   - $($tool.name): $($tool.description)" -ForegroundColor Gray
            }
            
            return $true
        }
        else {
            Write-Host "❌ Tools/List: FAIL (expected ≥3 tools, got $toolCount)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ Tools/List: FAIL (no tools returned)" -ForegroundColor Red
        return $false
    }
}

function Test-SearchCustomers {
    Write-Host "`n🧪 Test 4: Tool Call - search_customers" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "tools/call" -Params @{
        name      = "search_customers"
        arguments = @{
            name = "a"
        }
    }

    if ($response -and $response.result -and $response.result.content) {
        $contentText = $response.result.content[0].text
        
        if ($contentText -match "Se encontraron \d+ cliente\(s\)") {
            Write-Host "✅ search_customers: PASS" -ForegroundColor Green
            Write-Host "   Response: $($contentText.Substring(0, [Math]::Min(100, $contentText.Length)))..." -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ search_customers: FAIL (unexpected response format)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ search_customers: FAIL (no response)" -ForegroundColor Red
        return $false
    }
}

function Test-GetOrderDetails {
    Write-Host "`n🧪 Test 5: Tool Call - get_order_details" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "tools/call" -Params @{
        name      = "get_order_details"
        arguments = @{
            orderId = 1001
        }
    }

    if ($response -and $response.result -and $response.result.content) {
        $contentText = $response.result.content[0].text
        
        if ($contentText -match "Detalles del pedido #\d+") {
            Write-Host "✅ get_order_details: PASS" -ForegroundColor Green
            Write-Host "   Response: $($contentText.Substring(0, [Math]::Min(100, $contentText.Length)))..." -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ get_order_details: FAIL (unexpected response format)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ get_order_details: FAIL (no response)" -ForegroundColor Red
        return $false
    }
}

function Test-CalculateMetrics {
    Write-Host "`n🧪 Test 6: Tool Call - calculate_metrics" -ForegroundColor Cyan
    
    $response = Invoke-McpRequest -Method "tools/call" -Params @{
        name      = "calculate_metrics"
        arguments = @{
            metricType = "sales"
        }
    }

    if ($response -and $response.result -and $response.result.content) {
        $contentText = $response.result.content[0].text
        
        if ($contentText -match "Total de ventas:") {
            Write-Host "✅ calculate_metrics: PASS" -ForegroundColor Green
            Write-Host "   Result: $contentText" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ calculate_metrics: FAIL (unexpected response format)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "❌ calculate_metrics: FAIL (no response)" -ForegroundColor Red
        return $false
    }
}

function Test-ParameterValidation {
    Write-Host "`n🧪 Test 7: Parameter Validation - Invalid Metric Type" -ForegroundColor Cyan
    
    $body = @{
        jsonrpc = "2.0"
        method  = "tools/call"
        params  = @{
            name      = "calculate_metrics"
            arguments = @{
                metricType = "invalid_metric"
            }
        }
        id      = 99
    } | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "$ServerUrl/mcp" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        
        if ($response.error -and $response.error.code -eq -32603) {
            Write-Host "✅ Parameter Validation: PASS (error returned correctly)" -ForegroundColor Green
            Write-Host "   Error: $($response.error.message)" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ Parameter Validation: FAIL (should return error for invalid metric type)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "✅ Parameter Validation: PASS (request rejected)" -ForegroundColor Green
        return $true
    }
}

# Ejecutar tests
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " 🧮 Verificación: Ejercicio 2 - Parametric Query" -ForegroundColor Cyan
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
if (Test-ToolsList) { $testsPassed++ } else { $testsFailed++ }
if (Test-SearchCustomers) { $testsPassed++ } else { $testsFailed++ }
if (Test-GetOrderDetails) { $testsPassed++ } else { $testsFailed++ }
if (Test-CalculateMetrics) { $testsPassed++ } else { $testsFailed++ }
if (Test-ParameterValidation) { $testsPassed++ } else { $testsFailed++ }

# Resumen
Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Resumen de Tests" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Passed: $testsPassed" -ForegroundColor Green
Write-Host "❌ Failed: $testsFailed" -ForegroundColor Red
Write-Host "Total: $($testsPassed + $testsFailed)" -ForegroundColor Gray

if ($testsFailed -eq 0) {
    Write-Host "`n🎉 ¡Todos los tests pasaron! Ejercicio 2 completado." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n⚠️  Algunos tests fallaron. Revisa la implementación." -ForegroundColor Yellow
    exit 1
}
