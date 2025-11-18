# Ejercicio 1: Servidor de Recursos Estáticos - Starter Template

Este es el template de inicio para el Ejercicio 1. Debes implementar:

1. **Modelos**: `Customer` y `Product` en carpeta `Models/`
2. **Endpoint MCP**: `/mcp` que acepta POST con JSON-RPC 2.0
3. **Handler initialize**: Responder con serverInfo y capabilities
4. **Handler resources/list**: Devolver lista de recursos disponibles
5. **Handler resources/read**: Leer archivos JSON de `Data/`

## Ejecutar

```powershell
dotnet run
```

## Probar

```powershell
# Initialize
$body = @{ jsonrpc = "2.0"; method = "initialize"; params = @{}; id = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"

# Resources list
$body = @{ jsonrpc = "2.0"; method = "resources/list"; params = @{}; id = 2 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"

# Resources read
$body = @{ jsonrpc = "2.0"; method = "resources/read"; params = @{ uri = "customers" }; id = 3 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"
```
