#!/usr/bin/env pwsh
#Requires -Version 7.0

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Exercise 4: Virtual Analyst - Verification Script" -ForegroundColor Cyan
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
    Write-Host "Expected Intent: $ExpectedIntent" -ForegroundColor Gray
    Write-Host "Expected Servers: $($ExpectedServers -join ', ')" -ForegroundColor Gray

    try {
        $body = @{ query = $Query } | ConvertTo-Json
        $response = Invoke-RestMethod -Uri $orchestratorUrl -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop

        Write-Host "✓ Response received" -ForegroundColor Green

        # Check intent
        if ($response.Intent -eq $ExpectedIntent) {
            Write-Host "✓ Intent correct: $($response.Intent)" -ForegroundColor Green
        }
        else {
            Write-Host "✗ Intent incorrect: expected '$ExpectedIntent', got '$($response.Intent)'" -ForegroundColor Red
            $script:failedTests++
            return
        }

        # Check servers
        $serversMatch = $true
        foreach ($server in $ExpectedServers) {
            if ($response.ServersUsed -notcontains $server) {
                Write-Host "✗ Missing server: $server" -ForegroundColor Red
                $serversMatch = $false
            }
        }
        if ($serversMatch) {
            Write-Host "✓ Servers correct: $($response.ServersUsed -join ', ')" -ForegroundColor Green
        }
        else {
            $script:failedTests++
            return
        }

        # Check result contains expected pattern
        if ($ExpectedContentPattern -and $response.Result -notmatch $ExpectedContentPattern) {
            Write-Host "✗ Result doesn't match expected pattern" -ForegroundColor Red
            $script:failedTests++
            return
        }

        Write-Host "✓ Duration: $($response.DurationMs)ms" -ForegroundColor Green
        Write-Host "✓ From Cache: $($response.FromCache)" -ForegroundColor Green
        
        $script:passedTests++
        Write-Host "✅ TEST PASSED" -ForegroundColor Green

        return $response
    }
    catch {
        Write-Host "✗ Error: $_" -ForegroundColor Red
        $script:failedTests++
    }
}

# Check if orchestrator is running
Write-Host "Checking if VirtualAnalyst is running on $orchestratorUrl..." -ForegroundColor Cyan
try {
    $testBody = @{ query = "test" } | ConvertTo-Json
    Invoke-RestMethod -Uri $orchestratorUrl -Method POST -Body $testBody -ContentType "application/json" -TimeoutSec 2 -ErrorAction Stop | Out-Null
    Write-Host "✓ VirtualAnalyst is responding" -ForegroundColor Green
}
catch {
    Write-Host "✗ VirtualAnalyst is not responding at $orchestratorUrl" -ForegroundColor Red
    Write-Host "Please ensure the following servers are running:" -ForegroundColor Yellow
    Write-Host "  - SqlMcpServer (http://localhost:5010)" -ForegroundColor Yellow
    Write-Host "  - CosmosMcpServer (http://localhost:5011)" -ForegroundColor Yellow
    Write-Host "  - RestApiMcpServer (http://localhost:5012)" -ForegroundColor Yellow
    Write-Host "  - VirtualAnalyst (http://localhost:5004)" -ForegroundColor Yellow
    exit 1
}

# Test 1: New customers query
Test-Query `
    -TestName "New Customers in Madrid" `
    -Query "¿Cuántos clientes nuevos hay en Madrid?" `
    -ExpectedIntent "new_customers" `
    -ExpectedServers @("sql") `
    -ExpectedContentPattern "Madrid|clientes"

# Test 2: Abandoned carts
Test-Query `
    -TestName "Abandoned Carts (24 hours)" `
    -Query "¿Usuarios que abandonaron carrito últimas 24 horas?" `
    -ExpectedIntent "abandoned_carts" `
    -ExpectedServers @("cosmos") `
    -ExpectedContentPattern "usuarios|carrito"

# Test 3: Order status (sequential pattern)
Test-Query `
    -TestName "Order Status (Sequential)" `
    -Query "¿Estado del pedido 1001?" `
    -ExpectedIntent "order_status" `
    -ExpectedServers @("sql", "rest") `
    -ExpectedContentPattern "pedido|estado|#1001"

# Test 4: Sales summary (parallel pattern)
Write-Host ""
Write-Host "Test #4: Sales Summary (Parallel Execution)" -ForegroundColor Yellow
$response1 = Test-Query `
    -TestName "Sales Summary - First Call" `
    -Query "Resumen de ventas de esta semana" `
    -ExpectedIntent "sales_summary" `
    -ExpectedServers @("sql", "rest") `
    -ExpectedContentPattern "ventas|productos"

# Test for caching
Start-Sleep -Seconds 1
Write-Host ""
Write-Host "Test #5: Sales Summary - Cache Test" -ForegroundColor Yellow
$response2 = Test-Query `
    -TestName "Sales Summary - Second Call (Should be cached)" `
    -Query "Resumen de ventas de esta semana" `
    -ExpectedIntent "sales_summary" `
    -ExpectedServers @("sql", "rest") `
    -ExpectedContentPattern "ventas|productos"

if ($response2.FromCache -eq $true) {
    Write-Host "✓ Cache working: Second query returned from cache" -ForegroundColor Green
    if ($response2.DurationMs -lt $response1.DurationMs) {
        Write-Host "✓ Cache performance: Faster response ($($response2.DurationMs)ms vs $($response1.DurationMs)ms)" -ForegroundColor Green
    }
}
else {
    Write-Host "⚠ Warning: Expected cached response" -ForegroundColor Yellow
}

# Test 6: Top products
Test-Query `
    -TestName "Top 5 Products" `
    -Query "Top 5 productos más vendidos" `
    -ExpectedIntent "top_products" `
    -ExpectedServers @("rest") `
    -ExpectedContentPattern "productos|ranking|ventas"

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })

if ($failedTests -eq 0) {
    Write-Host ""
    Write-Host "✅ All tests passed!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host ""
    Write-Host "❌ Some tests failed" -ForegroundColor Red
    exit 1
}
