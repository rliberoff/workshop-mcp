# Ejercicio 4: Virtual Analyst - Orquestación MCP

## Objetivo

Implementar un orquestador que coordina 3 servidores MCP independientes para responder consultas en español.

## Arquitectura

```
VirtualAnalyst (puerto 5004)
├── SqlMcpServer (puerto 5010) - Datos transaccionales
├── CosmosMcpServer (puerto 5011) - Analítica de comportamiento
└── RestApiMcpServer (puerto 5012) - APIs externas
```

## Tareas

### 1. SpanishQueryParser

-   Detectar intención de consultas en español
-   Extraer parámetros (ciudad, fecha, orderId, etc.)
-   Mapear intención → servidores requeridos

### 2. McpServerClient

-   Implementar `CallToolAsync<T>` para invocar tools en servidores MCP
-   Implementar `GetResourceAsync` para leer resources
-   Timeout de 5 segundos

### 3. OrchestratorService

-   Implementar ejecución **paralela** para consultas independientes (Task.WhenAll)
-   Implementar ejecución **secuencial** para consultas dependientes (await)
-   Implementar cache con TTL 5 minutos
-   Formatear resultados

## Intenciones Soportadas

### new_customers

```
Query: "¿Cuántos clientes nuevos hay en Madrid?"
Servidores: sql
Herramienta: query_customers_by_country(country="España", city="Madrid")
```

### abandoned_carts

```
Query: "¿Qué usuarios abandonaron el carrito en las últimas 24 horas?"
Servidores: cosmos
Herramienta: get_abandoned_carts(hours=24)
```

### order_status

```
Query: "¿Cuál es el estado del pedido 1234?"
Servidores: sql, rest (secuencial)
1. sql: obtener detalles del pedido
2. rest: verificar inventario y envío
```

### sales_summary

```
Query: "Resumen de ventas de esta semana"
Servidores: sql, rest (PARALELO)
- sql: get_sales_summary(period="weekly")
- rest: get_top_products(period="week", limit=5)
Combinar resultados
```

## Patrones de Ejecución

### Paralelo (Task.WhenAll)

Cuando las consultas son **independientes** y no comparten datos:

```csharp
var sqlTask = sqlClient.CallToolAsync("get_sales_summary", args);
var restTask = restClient.CallToolAsync("get_top_products", args);
await Task.WhenAll(sqlTask, restTask);
var sqlResult = sqlTask.Result;
var restResult = restTask.Result;
```

### Secuencial (await)

Cuando una consulta **depende** del resultado de otra:

```csharp
var order = await sqlClient.CallToolAsync("get_order_details", args);
var inventory = await restClient.CallToolAsync("check_inventory",
    new { productId = order.ProductId });
```

## Pruebas

### Ejecutar servidores

```powershell
# Terminal 1: SqlMcpServer
dotnet run --project src/McpWorkshop.Servers/Exercise4SqlMcpServer

# Terminal 2: CosmosMcpServer
dotnet run --project src/McpWorkshop.Servers/Exercise4CosmosMcpServer

# Terminal 3: RestApiMcpServer
dotnet run --project src/McpWorkshop.Servers/Exercise4RestApiMcpServer

# Terminal 4: VirtualAnalyst
dotnet run --project templates/exercise4-starter
```

### Consultas de prueba

```powershell
# New customers
Invoke-RestMethod -Uri "http://localhost:5004/query" -Method POST -Body (@{
    query = "¿Cuántos clientes nuevos hay en Madrid?"
} | ConvertTo-Json) -ContentType "application/json"

# Abandoned carts
Invoke-RestMethod -Uri "http://localhost:5004/query" -Method POST -Body (@{
    query = "¿Usuarios con carrito abandonado últimas 24 horas?"
} | ConvertTo-Json) -ContentType "application/json"

# Order status
Invoke-RestMethod -Uri "http://localhost:5004/query" -Method POST -Body (@{
    query = "¿Estado del pedido 1234?"
} | ConvertTo-Json) -ContentType "application/json"

# Sales summary
Invoke-RestMethod -Uri "http://localhost:5004/query" -Method POST -Body (@{
    query = "Resumen de ventas de esta semana"
} | ConvertTo-Json) -ContentType "application/json"
```

## Verificación

```powershell
.\scripts\verify-exercise4.ps1
```
