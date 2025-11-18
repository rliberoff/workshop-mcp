# Ejercicio 3: Secure Server - MCP con Seguridad

## Objetivo

Implementar un servidor MCP con autenticación JWT, autorización por scopes, rate limiting y logging estructurado.

## Tareas

1. Implementar `JwtAuthMiddleware` para validar tokens Bearer
2. Implementar `ScopeAuthorizationMiddleware` para verificar permisos
3. Implementar `RateLimitMiddleware` con estrategia sliding-window
4. Implementar `LoggingMiddleware` con campos estructurados y redacción de datos sensibles
5. Implementar endpoint `/auth/token` para generar tokens de prueba
6. Configurar límites de rate en `appsettings.json`

## Scopes

-   `read`: Permite acceso a `resources/read`
-   `write`: Permite acceso a `tools/call` (incluye `read`)
-   `admin`: Acceso completo (incluye `write` y `read`)

## Rate Limits

-   Resources: 100 req/min
-   Tools: 50 req/min
-   Unauthenticated: 10 req/min

## Pruebas

### Generar token

```powershell
$tokenRequest = @{
    userId = "user123"
    scopes = @("read", "write")
    tier = "premium"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/auth/token" -Method POST -Body $tokenRequest -ContentType "application/json"
$token = $response.token
```

### Probar sin token (debe fallar 401)

```powershell
$request = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "secure://data/customers" }
    id = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $request -ContentType "application/json"
```

### Probar con token válido

```powershell
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $request -ContentType "application/json" -Headers $headers
```

### Probar rate limiting

```powershell
1..12 | ForEach-Object {
    $response = Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $request -ContentType "application/json" -Headers $headers
    Write-Host "Request $_: $($response.result.count) customers"
}
# Requests 11-12 deberían fallar con 429
```

## Verificación

Ejecutar script de verificación:

```powershell
.\scripts\verify-exercise3.ps1
```
