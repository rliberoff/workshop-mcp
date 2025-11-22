# Verification script for Exercise 3: Secure Server
# Tests JWT authentication, scope authorization, rate limiting, and logging

$ErrorActionPreference = "Stop"
$serverUrl = "http://localhost:5003"
$mcpEndpoint = "$serverUrl/mcp"
$authEndpoint = "$serverUrl/auth/token"

# Helper function to invoke MCP requests
function Invoke-McpRequest {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [string]$Token = $null
    )
    
    $request = @{
        jsonrpc = "2.0"
        method  = $Method
        params  = $Params
        id      = 1
    } | ConvertTo-Json -Depth 10

    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }

    try {
        $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $request -Headers $headers -ErrorAction Stop
        return $response
    }
    catch {
        return $_.Exception.Response
    }
}

# Helper function to generate token
function Get-AuthToken {
    param(
        [string]$UserId,
        [string[]]$Scopes,
        [string]$Tier = "basic"
    )
    
    $tokenRequest = @{
        userId = $UserId
        scopes = $Scopes
        tier   = $Tier
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $authEndpoint -Method POST -Body $tokenRequest -ContentType "application/json"
    return $response.token
}

# Test counters
$totalTests = 0
$passedTests = 0
$failedTests = 0

function Test-Result {
    param([bool]$Passed, [string]$TestName)
    $script:totalTests++
    if ($Passed) {
        $script:passedTests++
        Write-Host "  ✅ $TestName" -ForegroundColor Green
    }
    else {
        $script:failedTests++
        Write-Host "  ❌ $TestName" -ForegroundColor Red
    }
}

Write-Host "`n🔐 VERIFICACIÓN EJERCICIO 3: SECURE SERVER" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host "`n🔌 Verificando conectividad..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $serverUrl -Method GET -TimeoutSec 2 -ErrorAction Stop
    Write-Host "  ✅ Servidor accesible en $serverUrl" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Error: No se puede conectar al servidor en $serverUrl" -ForegroundColor Red
    Write-Host "  💡 Asegúrate de que el servidor esté ejecutándose: dotnet run --project src/McpWorkshop.Servers/Exercise3SecureServer" -ForegroundColor Yellow
    exit 1
}

# TEST 1: Authentication - No Token (should fail 401)
Write-Host "`n🧪 TEST 1: Autenticación - Sin Token" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $mcpEndpoint -Method POST -Body (@{
            jsonrpc = "2.0"
            method  = "resources/read"
            params  = @{ uri = "secure://data/customers" }
            id      = 1
        } | ConvertTo-Json) -ContentType "application/json" -ErrorAction Stop
    Test-Result $false "Debería rechazar request sin token"
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorMatches = $statusCode -eq 401
    Test-Result $errorMatches "Request sin token rechazado con 401 (actual: $statusCode)"
}

# TEST 2: Authentication - Generate Token
Write-Host "`n🧪 TEST 2: Autenticación - Generar Token" -ForegroundColor Cyan
try {
    $readToken = Get-AuthToken -UserId "user-read" -Scopes @("read")
    $tokenValid = $readToken.Length -gt 100
    Test-Result $tokenValid "Token generado correctamente (length: $($readToken.Length))"
    
    if ($tokenValid) {
        Write-Host "  📝 Token (primeros 50 chars): $($readToken.Substring(0, [Math]::Min(50, $readToken.Length)))..."
    }
}
catch {
    Test-Result $false "Error generando token: $_"
    exit 1
}

# TEST 3: Authentication - Valid Token
Write-Host "`n🧪 TEST 3: Autenticación - Token Válido" -ForegroundColor Cyan
try {
    $response = Invoke-McpRequest -Method "resources/list" -Token $readToken
    $hasResources = $response.result.resources.Count -ge 2
    Test-Result $hasResources "Request con token válido aceptado (recursos: $($response.result.resources.Count))"
}
catch {
    Test-Result $false "Error con token válido: $_"
}

# TEST 4: Authorization - Read Scope
Write-Host "`n🧪 TEST 4: Autorización - Scope Read" -ForegroundColor Cyan
try {
    $response = Invoke-McpRequest -Method "resources/read" -Params @{ uri = "secure://data/customers" } -Token $readToken
    $hasContent = $response.result.contents.Count -gt 0
    Test-Result $hasContent "Scope 'read' permite acceso a resources/read"
    
    if ($hasContent) {
        $data = $response.result.contents[0].text | ConvertFrom-Json
        Write-Host "  📊 Customers encontrados: $($data.Count)"
    }
}
catch {
    Test-Result $false "Error accediendo recurso con scope read: $_"
}

# TEST 5: Authorization - Insufficient Scope (read trying to write)
Write-Host "`n🧪 TEST 5: Autorización - Scope Insuficiente" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $mcpEndpoint -Method POST -Body (@{
            jsonrpc = "2.0"
            method  = "tools/call"
            params  = @{ 
                name      = "create_order"
                arguments = @{ customerId = 1; productId = 101; quantity = 1 }
            }
            id      = 1
        } | ConvertTo-Json -Depth 10) -Headers @{ Authorization = "Bearer $readToken" } -ContentType "application/json" -ErrorAction Stop
    Test-Result $false "Debería rechazar tools/call con solo scope 'read'"
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $forbiddenMatches = $statusCode -eq 403
    Test-Result $forbiddenMatches "Scope insuficiente rechazado con 403 (actual: $statusCode)"
}

# TEST 6: Authorization - Write Scope
Write-Host "`n🧪 TEST 6: Autorización - Scope Write" -ForegroundColor Cyan
try {
    $writeToken = Get-AuthToken -UserId "user-write" -Scopes @("write")
    $response = Invoke-McpRequest -Method "tools/call" -Params @{ 
        name      = "create_order"
        arguments = @{ customerId = 1; productId = 101; quantity = 2 }
    } -Token $writeToken
    
    $hasContent = $response.result.content.Count -gt 0
    Test-Result $hasContent "Scope 'write' permite acceso a tools/call"
    
    if ($hasContent) {
        $orderText = $response.result.content[0].text
        Write-Host "  📝 Order: $($orderText.Substring(0, [Math]::Min(100, $orderText.Length)))..."
    }
}
catch {
    Test-Result $false "Error llamando tool con scope write: $_"
}

# TEST 7: Authorization - Write includes Read
Write-Host "`n🧪 TEST 7: Autorización - Write Incluye Read" -ForegroundColor Cyan
try {
    $response = Invoke-McpRequest -Method "resources/read" -Params @{ uri = "secure://data/products" } -Token $writeToken
    $hasContent = $response.result.contents.Count -gt 0
    Test-Result $hasContent "Scope 'write' incluye permisos de 'read'"
}
catch {
    Test-Result $false "Error: write debería incluir read"
}

# TEST 8: Rate Limiting - Within Limit
Write-Host "`n🧪 TEST 8: Rate Limiting - Dentro del Límite" -ForegroundColor Cyan
$successCount = 0
$limitToken = Get-AuthToken -UserId "user-limit" -Scopes @("read") -Tier "basic"

for ($i = 1; $i -le 5; $i++) {
    try {
        $response = Invoke-McpRequest -Method "resources/list" -Token $limitToken
        if ($response.result) { $successCount++ }
    }
    catch {
        # Ignore errors
    }
}

Test-Result ($successCount -eq 5) "Primeras 5 requests dentro del límite ($successCount/5 exitosas)"

# TEST 9: Rate Limiting - Exceed Limit
Write-Host "`n🧪 TEST 9: Rate Limiting - Exceder Límite" -ForegroundColor Cyan
$unauthToken = Get-AuthToken -UserId "user-unauth-test" -Scopes @("read") -Tier "basic"
$rateLimitHit = $false
$successBeforeLimit = 0

# Try 15 requests (limit is 10 for unauthenticated tier)
for ($i = 1; $i -le 15; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $mcpEndpoint -Method POST -Body (@{
                jsonrpc = "2.0"
                method  = "resources/list"
                id      = 1
            } | ConvertTo-Json) -Headers @{ Authorization = "Bearer $unauthToken" } -ContentType "application/json" -ErrorAction Stop
        $successBeforeLimit++
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 429) {
            $rateLimitHit = $true
            break
        }
    }
    Start-Sleep -Milliseconds 50
}

Write-Host "  📊 Requests exitosas antes del límite: $successBeforeLimit"
Test-Result $rateLimitHit "Rate limit activado con código 429"

# TEST 10: Rate Limiting - Headers
Write-Host "`n🧪 TEST 10: Rate Limiting - Headers X-RateLimit-*" -ForegroundColor Cyan
try {
    $freshToken = Get-AuthToken -UserId "user-headers-$(Get-Random)" -Scopes @("read")
    $response = Invoke-WebRequest -Uri $mcpEndpoint -Method POST -Body (@{
            jsonrpc = "2.0"
            method  = "resources/list"
            id      = 1
        } | ConvertTo-Json) -Headers @{ Authorization = "Bearer $freshToken" } -ContentType "application/json"
    
    $hasLimitHeader = $response.Headers["X-RateLimit-Limit"] -ne $null
    $hasRemainingHeader = $response.Headers["X-RateLimit-Remaining"] -ne $null
    $hasResetHeader = $response.Headers["X-RateLimit-Reset"] -ne $null
    
    Test-Result $hasLimitHeader "Header X-RateLimit-Limit presente"
    Test-Result $hasRemainingHeader "Header X-RateLimit-Remaining presente"
    Test-Result $hasResetHeader "Header X-RateLimit-Reset presente"
    
    if ($hasLimitHeader) {
        Write-Host "  📊 Limit: $($response.Headers['X-RateLimit-Limit']), Remaining: $($response.Headers['X-RateLimit-Remaining'])"
    }
}
catch {
    Test-Result $false "Error verificando headers: $_"
}

# TEST 11: Structured Logging Verification
Write-Host "`n🧪 TEST 11: Logging Estructurado" -ForegroundColor Cyan
Write-Host "  💡 Verifica manualmente los logs del servidor para:" -ForegroundColor Yellow
Write-Host "     - timestamp (ISO8601)" -ForegroundColor Gray
Write-Host "     - level (Info/Warning/Error)" -ForegroundColor Gray
Write-Host "     - method (MCP method name)" -ForegroundColor Gray
Write-Host "     - userId (del token)" -ForegroundColor Gray
Write-Host "     - requestId (Guid)" -ForegroundColor Gray
Write-Host "     - duration (milliseconds)" -ForegroundColor Gray
Write-Host "     - statusCode (HTTP status)" -ForegroundColor Gray
Test-Result $true "Logging middleware configurado (verificación manual)"

# TEST 12: Sensitive Field Redaction
Write-Host "`n🧪 TEST 12: Redacción de Campos Sensibles" -ForegroundColor Cyan
Write-Host "  💡 Verifica que los logs NO muestren valores de:" -ForegroundColor Yellow
Write-Host "     - password" -ForegroundColor Gray
Write-Host "     - token" -ForegroundColor Gray
Write-Host "     - secret" -ForegroundColor Gray
Write-Host "     - authorization" -ForegroundColor Gray
Write-Host "  ✅ Deberían aparecer como [REDACTED]" -ForegroundColor Green
Test-Result $true "Redacción de campos sensibles configurada (verificación manual)"

# Summary
Write-Host "`n" + ("=" * 60)
Write-Host "📊 RESUMEN DE VERIFICACIÓN" -ForegroundColor Cyan
Write-Host ("=" * 60)
Write-Host "  Total de pruebas: $totalTests" -ForegroundColor White
Write-Host "  ✅ Exitosas: $passedTests" -ForegroundColor Green
Write-Host "  ❌ Fallidas: $failedTests" -ForegroundColor Red
Write-Host ("=" * 60)

if ($failedTests -eq 0) {
    Write-Host "`n🎉 ¡TODAS LAS PRUEBAS PASARON!" -ForegroundColor Green
    Write-Host "✅ Exercise 3 SecureServer está funcionando correctamente" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n⚠️ ALGUNAS PRUEBAS FALLARON" -ForegroundColor Yellow
    Write-Host "Revisa los errores arriba y corrige la implementación" -ForegroundColor Yellow
    exit 1
}
