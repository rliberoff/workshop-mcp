# Ejercicio 2: Herramientas Paramétricas - Starter Template

Este es el template de inicio para el Ejercicio 2. Debes implementar:

1. **3 Herramientas**:

    - `search_customers`: Buscar clientes con parámetros opcionales (name, country)
    - `filter_products`: Filtrar productos (category, minPrice, maxPrice, inStockOnly)
    - `aggregate_sales`: Agregar ventas (period: "daily", "weekly", "monthly")

2. **JSON Schema**: Definir inputSchema para cada herramienta
3. **Handler tools/list**: Devolver GetDefinition() de cada tool
4. **Handler tools/call**: Ejecutar tool con argumentos

## Ejecutar

```powershell
dotnet run
```

## Probar

```powershell
# Tools list
$body = @{ jsonrpc = "2.0"; method = "tools/list"; params = @{}; id = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"

# Search customers
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "search_customers"
        arguments = @{ country = "España" }
    }
    id = 2
} | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```
