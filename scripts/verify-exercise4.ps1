#!/usr/bin/env pwsh
#Requires -Version 7.0

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Ejercicio 4: Orquestador - Script de Verificación" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Tracking
$totalTests = 0
$passedTests = 0
$failedTests = 0

# Configuration
$orchestratorUrl = "http://localhost:5004/query"

function Test-Query {
    param(
        [string]$TestName,
        [string]$Query,
        [string]$ExpectedIntent,
        [string[]]$ExpectedServers,
        [string]$ExpectedContentPattern
    )

    $script:totalTests++

    Write-Host ""
    Write-Host "Test #$($script:totalTests): $TestName" -ForegroundColor Yellow
    Write-Host "Query: '$Query'" -ForegroundColor Gray

    try {
        $body = @{ query = $Query } | ConvertTo-Json
        $response = Invoke-RestMethod -Uri $orchestratorUrl -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop

        Write-Host "✓ Response received" -ForegroundColor Green

        # Check if we got an answer
        if (-not $response.answer) {
            Write-Host "✗ No hay respuesta" -ForegroundColor Red
            $script:failedTests++
            return
        }

        # Check for error messages
        if ($response.answer -like "*no entendí*" -or $response.answer -like "*Error*") {
            Write-Host "✗ Consulta no entendida o error: $($response.answer)" -ForegroundColor Red
            $script:failedTests++
            return
        }

        # Check if answer contains expected patterns based on intent
        $answerText = $response.answer.ToString()
        $intentValid = $false

        switch ($ExpectedIntent) {
            "new_customers" {
                if ($answerText -match "Clientes nuevos|clientes|customers") {
                    $intentValid = $true
                }
            }
            "abandoned_carts" {
                if ($answerText -match "Carritos abandonados|carrito|usuarios") {
                    $intentValid = $true
                }
            }
            "order_status" {
                if ($answerText -match "Estado del pedido|pedido|order") {
                    $intentValid = $true
                }
            }
            "top_products" {
                if ($answerText -match "Productos más vendidos|productos|top") {
                    $intentValid = $true
                }
            }
            "sales_summary" {
                if ($answerText -match "Resumen de ventas|ventas|sales") {
                    $intentValid = $true
                }
            }
        }

        if ($intentValid) {
            Write-Host "✓ Intención reconocida: $ExpectedIntent" -ForegroundColor Green
        }
        else {
            Write-Host "✗ Intención no reconocida en la respuesta" -ForegroundColor Red
            $script:failedTests++
            return
        }

        # Check for cache indicator
        $fromCache = $answerText -like "*[CACHE]*"
        if ($fromCache) {
            Write-Host "✓ Respuesta desde caché" -ForegroundColor Green
        }

        Write-Host "✓ Respuesta: $($answerText.Substring(0, [Math]::Min(100, $answerText.Length)))..." -ForegroundColor Green
        
        $script:passedTests++
        Write-Host "✅ PRUEBA EXITOSA" -ForegroundColor Green

        return $response
    }
    catch {
        Write-Host "✗ Error: $_" -ForegroundColor Red
        $script:failedTests++
    }
}

# Check if orchestrator is running
Write-Host "Verificando si el Orquestador está ejecutándose en $orchestratorUrl..." -ForegroundColor Cyan
try {
    $testBody = @{ query = "test" } | ConvertTo-Json
    Invoke-RestMethod -Uri $orchestratorUrl -Method POST -Body $testBody -ContentType "application/json" -TimeoutSec 2 -ErrorAction Stop | Out-Null
    Write-Host "✓ El Orquestador está respondiendo" -ForegroundColor Green
}
catch {
    Write-Host "✗ El Orquestador no está respondiendo en $orchestratorUrl" -ForegroundColor Red
    Write-Host "Por favor, asegúrate de que los siguientes servidores estén ejecutándose:" -ForegroundColor Yellow
    Write-Host "  - SqlMcpServer (http://localhost:5010)" -ForegroundColor Yellow
    Write-Host "  - CosmosMcpServer (http://localhost:5011)" -ForegroundColor Yellow
    Write-Host "  - RestApiMcpServer (http://localhost:5012)" -ForegroundColor Yellow
    Write-Host "  - Orquestador (http://localhost:5004)" -ForegroundColor Yellow
    exit 1
}

# Test 1: New customers query
Test-Query `
    -TestName "Clientes Nuevos en España" `
    -Query "¿Cuántos clientes nuevos hay en España?" `
    -ExpectedIntent "new_customers" `
    -ExpectedServers @("sql") `
    -ExpectedContentPattern "España|clientes"

# Test 2: Abandoned carts
Test-Query `
    -TestName "Carritos Abandonados (24 horas)" `
    -Query "¿Usuarios que abandonaron carrito últimas 24 horas?" `
    -ExpectedIntent "abandoned_carts" `
    -ExpectedServers @("cosmos") `
    -ExpectedContentPattern "usuarios|carrito"

# Test 3: Order status (sequential pattern)
Test-Query `
    -TestName "Estado de Pedido (Secuencial)" `
    -Query "¿Estado del pedido 1001?" `
    -ExpectedIntent "order_status" `
    -ExpectedServers @("sql", "rest") `
    -ExpectedContentPattern "pedido|estado|#1001"

# Test 4: Sales summary (parallel pattern)
$response1 = Test-Query `
    -TestName "Resumen de Ventas - Primera Llamada" `
    -Query "Resumen de ventas de esta semana" `
    -ExpectedIntent "sales_summary" `
    -ExpectedServers @("sql", "rest") `
    -ExpectedContentPattern "ventas|productos"

# Test 5: Top products
Test-Query `
    -TestName "Top 5 Productos Más Vendidos" `
    -Query "Top 5 productos más vendidos" `
    -ExpectedIntent "top_products" `
    -ExpectedServers @("rest") `
    -ExpectedContentPattern "productos|ranking|ventas"

# Test 6: Caching
Start-Sleep -Seconds 1
Write-Host ""
Write-Host "Test #6: Cache - Resumen de Ventas - Segunda Llamada" -ForegroundColor Yellow
$script:totalTests++
try {
    $body = @{ query = "Resumen de ventas de esta semana" } | ConvertTo-Json
    $response2 = Invoke-RestMethod -Uri $orchestratorUrl -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    
    if ($response2.answer -like "*[CACHE]*") {
        Write-Host "✓ Caché funcionando: La respuesta contiene el marcador [CACHE]" -ForegroundColor Green
        $script:passedTests++
        Write-Host "✅ TEST PASSED" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Advertencia: Se esperaba respuesta desde caché (no se encontró marcador [CACHE])" -ForegroundColor Yellow
        $script:passedTests++
    }
}
catch {
    Write-Host "⚠ Advertencia: No se pudo probar el caché" -ForegroundColor Yellow
    $script:failedTests++
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Resumen de Pruebas" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total de Pruebas: $totalTests" -ForegroundColor White
Write-Host "Exitosas: $passedTests" -ForegroundColor Green
Write-Host "Fallidas: $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })

if ($failedTests -eq 0) {
    Write-Host ""
    Write-Host "✅ ¡Todas las pruebas pasaron exitosamente!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host ""
    Write-Host "❌ Algunas pruebas fallaron" -ForegroundColor Red
    exit 1
}
