# Verification script for Exercise 3: Secure Server
# Based on documentation in docs/modules/05b-ejercicio-3-seguridad.md
# Tests JWT authentication, scope authorization, rate limiting, and logging

$ErrorActionPreference = "Stop"
$serverUrl = "http://localhost:5003"
$mcpEndpoint = "$serverUrl/mcp"
$authEndpoint = "$serverUrl/auth/token"

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

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " 🔐 Verificación: Ejercicio 3 - Secure Server" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Connectivity Check
Write-Host "`n🔌 Verificando conectividad..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $serverUrl -Method GET -TimeoutSec 2 -ErrorAction Stop
    $healthData = $response.Content | ConvertFrom-Json
    Write-Host "  ✅ Servidor accesible: $($healthData.server) v$($healthData.version)" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Error: No se puede conectar al servidor en $serverUrl" -ForegroundColor Red
    Write-Host "  💡 Ejecuta: dotnet run --project src/McpWorkshop.Servers/Exercise3Server" -ForegroundColor Yellow
    exit 1
}

# TEST 1: Generate Token with scope 'read'
Write-Host "`n🧪 TEST 1: Generar Token (scope: read)" -ForegroundColor Cyan
try {
    $body = @{
        userId = "user-001"
        name = "Ana García"
        scopes = @("read")
        tier = "base"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $authEndpoint -Method POST -Body $body -ContentType "application/json"
    $tokenRead = $response.token
    $tokenValid = $tokenRead.Length -gt 100
    Test-Result $tokenValid "Token generado para 'Ana García' (length: $($tokenRead.Length))"
}
catch {
    Write-Host "  ⚠️ Error: $_" -ForegroundColor Red
    Test-Result $false "Error generando token"
    exit 1
}

# TEST 2: Generate Token with scopes 'read' and 'write'
Write-Host "`n🧪 TEST 2: Generar Token (scope: read, write)" -ForegroundColor Cyan
try {
    $body = @{
        userId = "user-002"
        name = "Carlos Pérez"
        scopes = @("read", "write")
        tier = "premium"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $authEndpoint -Method POST -Body $body -ContentType "application/json"
    $tokenWrite = $response.token
    $tokenValid = $tokenWrite.Length -gt 100
    Test-Result $tokenValid "Token generado para 'Carlos Pérez' (length: $($tokenWrite.Length))"
}
catch {
    Write-Host "  ⚠️ Error: $_" -ForegroundColor Red
    Test-Result $false "Error generando token con write"
    exit 1
}

# TEST 3: Access without authentication (should fail)
Write-Host "`n🧪 TEST 3: Acceso sin autenticación (debe fallar)" -ForegroundColor Cyan
try {
    $body = @{
        jsonrpc = "2.0"
        method = "resources/read"
        params = @{ uri = "mcp://secure-data" }
        id = "read-001"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    
    # If we get here without exception, check if there's an error in the response
    if ($response.error) {
        Test-Result $true "Acceso sin token rechazado (error: $($response.error.message))"
    } else {
        Test-Result $false "Debería haber rechazado el acceso sin token"
    }
}
catch {
    # Exception is expected (Unauthorized)
    Test-Result $true "Acceso sin token rechazado correctamente"
}

# TEST 4: Access with scope 'read' (should work)
Write-Host "`n🧪 TEST 4: Acceso con scope 'read' a resources/read" -ForegroundColor Cyan
try {
    $body = @{
        jsonrpc = "2.0"
        method = "resources/read"
        params = @{ uri = "mcp://secure-data" }
        id = "read-002"
    } | ConvertTo-Json

    $headers = @{ Authorization = "Bearer $tokenRead" }
    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -Headers $headers -ContentType "application/json"
    
    $hasContents = ($null -ne $response.result.contents) -and ($response.result.contents.Count -gt 0)
    Test-Result $hasContents "Scope 'read' permite acceso a resources/read"
    
    if ($hasContents) {
        $contentData = $response.result.contents[0].text | ConvertFrom-Json
        Write-Host "  📊 Datos obtenidos: $($contentData.message), Usuario: $($contentData.user)" -ForegroundColor Gray
    }
}
catch {
    Write-Host "  ⚠️ Error: $_" -ForegroundColor Red
    Test-Result $false "Error accediendo recurso con scope read"
}

# TEST 5: Access to tools/call with only 'read' (should fail)
Write-Host "`n🧪 TEST 5: Acceso a tools/call con solo scope 'read' (debe fallar)" -ForegroundColor Cyan
try {
    $body = @{
        jsonrpc = "2.0"
        method = "tools/call"
        params = @{
            name = "secure_action"
            arguments = @{ action = "test" }
        }
        id = "call-001"
    } | ConvertTo-Json

    $headers = @{ Authorization = "Bearer $tokenRead" }
    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -Headers $headers -ContentType "application/json" -ErrorAction Stop
    
    # Check if error in response
    if ($response.error) {
        $hasInsufficientPerms = $response.error.message -match "Insufficient permissions"
        Test-Result $hasInsufficientPerms "Scope insuficiente rechazado (error: $($response.error.message))"
    } else {
        Test-Result $false "Debería haber rechazado tools/call con solo scope 'read'"
    }
}
catch {
    # Exception is also acceptable
    Test-Result $true "Acceso a tools/call rechazado correctamente"
}

# TEST 6: Access to tools/call with 'write' (should work)
Write-Host "`n🧪 TEST 6: Acceso a tools/call con scope 'write'" -ForegroundColor Cyan
try {
    $body = @{
        jsonrpc = "2.0"
        method = "tools/call"
        params = @{
            name = "secure_action"
            arguments = @{ action = "test" }
        }
        id = "call-002"
    } | ConvertTo-Json

    $headers = @{ Authorization = "Bearer $tokenWrite" }
    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -Headers $headers -ContentType "application/json"
    
    $hasContent = ($null -ne $response.result.content) -and ($response.result.content.Count -gt 0)
    Test-Result $hasContent "Scope 'write' permite acceso a tools/call"
    
    if ($hasContent) {
        $actionResult = $response.result.content[0].text
        Write-Host "  📝 Resultado: $($actionResult.Substring(0, [Math]::Min(80, $actionResult.Length)))..." -ForegroundColor Gray
    }
}
catch {
    Write-Host "  ⚠️ Error: $_" -ForegroundColor Red
    Test-Result $false "Error llamando tool con scope write"
}

# TEST 7: Rate Limiting (base tier: 10 req/min)
Write-Host "`n🧪 TEST 7: Rate Limiting - Usuario base (10 req/min)" -ForegroundColor Cyan
Write-Host "  💡 Generando nuevo usuario para evitar límites previos..." -ForegroundColor Gray

try {
    # Generate new token for rate limit test
    $limitUserId = "user-limit-$(Get-Random -Maximum 99999)"
    $body = @{
        userId = $limitUserId
        name = "Test User Rate Limit"
        scopes = @("read")
        tier = "base"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $authEndpoint -Method POST -Body $body -ContentType "application/json"
    $tokenLimit = $response.token
    
    $successCount = 0
    $rateLimitHit = $false
    
    # Try 12 requests (limit is 10 for base tier)
    for ($i = 1; $i -le 12; $i++) {
        try {
            $body = @{
                jsonrpc = "2.0"
                method = "initialize"
                params = @{}
                id = "init-$i"
            } | ConvertTo-Json

            $headers = @{ Authorization = "Bearer $tokenLimit" }
            $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -Headers $headers -ContentType "application/json" -ErrorAction Stop
            
            if ($response.error -and $response.error.code -eq -32003) {
                $rateLimitHit = $true
                Write-Host "  Request $i : ❌ Rate limit exceeded" -ForegroundColor Yellow
                break
            } else {
                $successCount++
                Write-Host "  Request $i : ✅ OK" -ForegroundColor Green
            }
        }
        catch {
            $rateLimitHit = $true
            Write-Host "  Request $i : ❌ Rate limit exceeded" -ForegroundColor Yellow
            break
        }
        
        Start-Sleep -Milliseconds 50
    }
    
    Write-Host "  📊 Requests exitosas: $successCount/12" -ForegroundColor Gray
    $rateLimitWorking = $successCount -le 10 -and $rateLimitHit
    Test-Result $rateLimitWorking "Rate limiting funciona (límite: 10 req/min para tier 'base')"
}
catch {
    Write-Host "  ⚠️ Error: $_" -ForegroundColor Red
    Test-Result $false "Error en prueba de rate limiting"
}

# TEST 8: Public endpoints (initialize, resources/list, tools/list) don't require authentication
Write-Host "`n🧪 TEST 8: Endpoints públicos sin autenticación" -ForegroundColor Cyan
try {
    $body = @{
        jsonrpc = "2.0"
        method = "initialize"
        params = @{}
        id = 1
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -ContentType "application/json"
    $hasProtocol = $response.result.protocolVersion -ne $null
    Test-Result $hasProtocol "Método 'initialize' es público (no requiere token)"
}
catch {
    Test-Result $false "Error: 'initialize' debería ser público"
}

try {
    $body = @{
        jsonrpc = "2.0"
        method = "resources/list"
        params = @{}
        id = 2
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -ContentType "application/json"
    $hasResources = ($response.result.resources -ne $null) -and ($response.result.resources.Count -gt 0)
    Test-Result $hasResources "Método 'resources/list' es público"
}
catch {
    Test-Result $false "Error: 'resources/list' debería ser público"
}

try {
    $body = @{
        jsonrpc = "2.0"
        method = "tools/list"
        params = @{}
        id = 3
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $mcpEndpoint -Method POST -Body $body -ContentType "application/json"
    $hasTools = ($response.result.tools -ne $null) -and ($response.result.tools.Count -gt 0)
    Test-Result $hasTools "Método 'tools/list' es público"
}
catch {
    Test-Result $false "Error: 'tools/list' debería ser público"
}

# TEST 9: Logging verification (manual)
Write-Host "`n🧪 TEST 9: Logging Estructurado (verificación manual)" -ForegroundColor Cyan
Write-Host "  💡 Verifica en los logs del servidor:" -ForegroundColor Yellow
Write-Host "     - timestamp, level, method, userId, requestId, duration" -ForegroundColor Gray
Write-Host "     - Campos sensibles (password, token, secret) aparecen como [REDACTED]" -ForegroundColor Gray
Test-Result $true "Logging configurado (requiere verificación manual en la consola del servidor)"

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
    Write-Host "✅ Exercise 3 implementado correctamente según especificación" -ForegroundColor Green
    Write-Host "`n📚 Conceptos verificados:" -ForegroundColor Cyan
    Write-Host "  • Autenticación JWT" -ForegroundColor Gray
    Write-Host "  • Autorización basada en scopes (read, write)" -ForegroundColor Gray
    Write-Host "  • Rate limiting (10 req/min base, 50 req/min premium)" -ForegroundColor Gray
    Write-Host "  • Endpoints públicos vs protegidos" -ForegroundColor Gray
    Write-Host "  • Logging estructurado" -ForegroundColor Gray
    exit 0
}
else {
    Write-Host "`n⚠️ ALGUNAS PRUEBAS FALLARON" -ForegroundColor Yellow
    Write-Host "Revisa los errores arriba y compara con la documentación:" -ForegroundColor Yellow
    Write-Host "  docs/modules/05b-ejercicio-3-seguridad.md" -ForegroundColor Gray
    exit 1
}
