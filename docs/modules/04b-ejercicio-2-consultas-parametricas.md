# Bloque 4: Ejercicio 2 - Consultas Paramétricas con Herramientas (20 minutos)

**Tipo**: Ejercicio semi-independiente  
**Duración**: 20 minutos  
**Nivel**: Intermedio  
**Objetivo**: Implementar herramientas MCP con parámetros dinámicos y validación

---

## 🎯 Objetivos del Ejercicio

Al completar este ejercicio, habrás:

1. ✅ Implementado el método `tools/list` con definiciones JSON Schema
2. ✅ Implementado el método `tools/call` para invocar herramientas
3. ✅ Creado al menos 3 herramientas con parámetros:
    - `search_customers`: Búsqueda de clientes por nombre/país
    - `get_order_details`: Detalles de pedido por ID
    - `calculate_metrics`: Métricas de negocio (total de ventas, promedio de pedido)
4. ✅ Validado parámetros de entrada con JSON Schema
5. ✅ Probado las herramientas con diferentes combinaciones de parámetros

---

## 📋 Prerrequisitos

Antes de comenzar, verifica que:

-   [x] Completaste el Ejercicio 1 exitosamente
-   [x] Tienes `Exercise1Server` funcionando en puerto 5001
-   [x] Conoces la estructura de JSON-RPC 2.0
-   [x] Entiendes los conceptos de recursos vs herramientas

---

## 📂 Estructura del Servidor a Crear

```
src/McpWorkshop.Servers/
└── Exercise2Server/
    ├── Program.cs                # Servidor principal con tools
    ├── Exercise2Server.csproj    # Archivo de proyecto
    ├── Models/
    │   ├── Customer.cs           # Reutilizado del Ejercicio 1
    │   ├── Product.cs            # Reutilizado del Ejercicio 1
    │   └── Order.cs              # Nuevo: modelo de pedido
    └── Tools/
        ├── SearchCustomersTool.cs       # Herramienta de búsqueda
        ├── GetOrderDetailsTool.cs       # Herramienta de detalles
        └── CalculateMetricsTool.cs      # Herramienta de métricas
```

---

## 🆚 Diferencias: Recursos vs Herramientas

| Aspecto         | Recursos                                 | Herramientas                                   |
| --------------- | ---------------------------------------- | ---------------------------------------------- |
| **Propósito**   | Exponer datos estáticos o semi-estáticos | Ejecutar operaciones dinámicas                 |
| **Métodos MCP** | `resources/list`, `resources/read`       | `tools/list`, `tools/call`                     |
| **Parámetros**  | Opcional (solo URI)                      | Requeridos (definidos en JSON Schema)          |
| **Ejemplo**     | `mcp://customers` (lista completa)       | `search_customers(name="John", country="USA")` |
| **Uso típico**  | Catálogos, archivos, documentación       | Búsquedas, cálculos, acciones                  |

---

## 🚀 Paso a Paso

### Paso 1: Crear el Proyecto (3 minutos)

#### 1.1 Crear estructura

```powershell
cd src/McpWorkshop.Servers
dotnet new web -n Exercise2Server -f net10.0
cd Exercise2Server

# Agregar referencias
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj

# Agregar a solución
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise2Server/Exercise2Server.csproj
```

#### 1.2 Crear carpetas

```powershell
cd src/McpWorkshop.Servers/Exercise2Server
mkdir Models
mkdir Tools
```

#### 1.3 Copiar modelos del Ejercicio 1

```powershell
Copy-Item ../Exercise1Server/Models/Customer.cs Models/
Copy-Item ../Exercise1Server/Models/Product.cs Models/
```

‼️Recuerda cambiar el `namespace` en los archivos copiados a `Exercise2Server.Models`. De lo contrario, el código del Paso 2 no compilará.

#### 1.4 Crear modelo Order

Crea `Models/Order.cs`:

```csharp
namespace Exercise2Server.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}
```

**✅ Checkpoint**: Tres modelos creados (Customer, Product, Order).

---

### Paso 2: Implementar Herramientas (10 minutos)

#### 2.1 Herramienta: SearchCustomersTool

Crea `Tools/SearchCustomersTool.cs`:

```csharp
using System.Text.Json;
using Exercise2Server.Models;

namespace Exercise2Server.Tools;

public static class SearchCustomersTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "search_customers",
            description = "Busca clientes por nombre parcial y/o país",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    name = new
                    {
                        type = "string",
                        description = "Nombre parcial del cliente (case-insensitive)"
                    },
                    country = new
                    {
                        type = "string",
                        description = "País del cliente (exacto)"
                    }
                },
                required = new string[] { } // Ambos parámetros son opcionales
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, List<Customer> customers)
    {
        var query = customers.AsEnumerable();

        // Filtrar por nombre si se proporciona
        if (arguments.TryGetValue("name", out var nameElement))
        {
            var name = nameElement.GetString();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        // Filtrar por país si se proporciona
        if (arguments.TryGetValue("country", out var countryElement))
        {
            var country = countryElement.GetString();
            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(c => c.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
            }
        }

        var results = query.ToList();

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"Se encontraron {results.Count} cliente(s):\n" +
                           JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true })
                }
            }
        };
    }
}
```

#### 2.2 Herramienta: GetOrderDetailsTool

Crea `Tools/GetOrderDetailsTool.cs`:

```csharp
using System.Text.Json;
using Exercise2Server.Models;

namespace Exercise2Server.Tools;

public static class GetOrderDetailsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "get_order_details",
            description = "Obtiene detalles completos de un pedido incluyendo cliente y producto",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    orderId = new
                    {
                        type = "integer",
                        description = "ID del pedido a consultar"
                    }
                },
                required = new[] { "orderId" }
            }
        };
    }

    public static object Execute(
        Dictionary<string, JsonElement> arguments,
        List<Order> orders,
        List<Customer> customers,
        List<Product> products)
    {
        if (!arguments.TryGetValue("orderId", out var orderIdElement))
        {
            throw new ArgumentException("El parámetro 'orderId' es requerido");
        }

        var orderId = orderIdElement.GetInt32();
        var order = orders.FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            return new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = $"No se encontró el pedido con ID {orderId}"
                    }
                }
            };
        }

        var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
        var product = products.FirstOrDefault(p => p.Id == order.ProductId);

        var details = new
        {
            order,
            customer = customer != null ? new { customer.Id, customer.Name, customer.Email, customer.Country } : null,
            product = product != null ? new { product.Id, product.Name, product.Price, product.Category } : null
        };

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"Detalles del pedido #{orderId}:\n" +
                           JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = true })
                }
            }
        };
    }
}
```

#### 2.3 Herramienta: CalculateMetricsTool

Crea `Tools/CalculateMetricsTool.cs`:

```csharp
using System.Text.Json;
using Exercise2Server.Models;

namespace Exercise2Server.Tools;

public static class CalculateMetricsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "calculate_metrics",
            description = "Calcula métricas de negocio: total de ventas, promedio de pedido, productos más vendidos",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    metricType = new
                    {
                        type = "string",
                        @enum = new[] { "sales", "average", "top_products" },
                        description = "Tipo de métrica: 'sales' (ventas totales), 'average' (promedio de pedido), 'top_products' (productos más vendidos)"
                    }
                },
                required = new[] { "metricType" }
            }
        };
    }

    public static object Execute(
        Dictionary<string, JsonElement> arguments,
        List<Order> orders,
        List<Product> products)
    {
        if (!arguments.TryGetValue("metricType", out var metricTypeElement))
        {
            throw new ArgumentException("El parámetro 'metricType' es requerido");
        }

        var metricType = metricTypeElement.GetString();
        string resultText;

        switch (metricType)
        {
            case "sales":
                var totalSales = orders.Sum(o => o.TotalAmount);
                resultText = $"Total de ventas: {totalSales:C}";
                break;

            case "average":
                var averageOrder = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
                resultText = $"Promedio de pedido: {averageOrder:C}";
                break;

            case "top_products":
                var topProducts = orders
                    .GroupBy(o => o.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        ProductName = products.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Unknown",
                        TotalQuantity = g.Sum(o => o.Quantity),
                        TotalRevenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(p => p.TotalRevenue)
                    .Take(5)
                    .ToList();

                resultText = "Top 5 productos más vendidos:\n" +
                             JsonSerializer.Serialize(topProducts, new JsonSerializerOptions { WriteIndented = true });
                break;

            default:
                throw new ArgumentException($"Tipo de métrica no válido: {metricType}");
        }

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = resultText
                }
            }
        };
    }
}
```

**✅ Checkpoint**: Tres herramientas creadas con definiciones JSON Schema.

---

### Paso 3: Implementar Program.cs (5 minutos)

Reemplaza todo el contenido de `Program.cs`:

```csharp
using System.Text.Json;
using Exercise2Server.Models;
using Exercise2Server.Tools;
using McpWorkshop.Shared.Logging;
using McpWorkshop.Shared.Mcp;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStructuredLogger, StructuredLogger>();
builder.Services.Configure<McpWorkshop.Shared.Configuration.WorkshopSettings>(options =>
{
    options.Server.Name = "Exercise2Server";
    options.Server.Version = "1.0.0";
    options.Server.ProtocolVersion = "2024-11-05";
    options.Server.Port = 5002;
});

var app = builder.Build();

// Variables para almacenar los datos cargados durante initialize
List<Customer>? customers = null;
List<Product>? products = null;
List<Order>? orders = null;

// Health check endpoint
app.MapGet("/", (IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings) => Results.Ok(new
{
    status = "healthy",
    server = settings.Value.Server.Name,
    version = settings.Value.Server.Version,
    timestamp = DateTime.UtcNow
}));

// Endpoint MCP
app.MapPost("/mcp", async (
    JsonRpcRequest request,
    IStructuredLogger logger,
    IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings) =>
{
    var requestId = request.Id?.ToString() ?? "unknown";

    IDictionary<string, object>? paramsDict = null;
    if (request.Params != null)
    {
        paramsDict = JsonSerializer.Deserialize<IDictionary<string, object>>(JsonSerializer.Serialize(request.Params));
    }

    logger.LogRequest(request.Method, requestId, paramsDict);

    try
    {
        var response = request.Method switch
        {
            "initialize" => HandleInitialize(request.Id, settings, ref customers, ref products, ref orders),
            "tools/list" => HandleToolsList(request.Id),
            "tools/call" => HandleToolsCall(request.Id, paramsDict, customers, products, orders),
            _ => CreateErrorResponse(-32601, "Method not found", null, request.Id)
        };

        logger.LogResponse(request.Method, requestId, 200, 0);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(request.Method, requestId, ex);
        return Results.Ok(CreateErrorResponse(-32603, "Internal error", ex.Message, request.Id));
    }
});

app.Run("http://localhost:5002");

// Handlers
static JsonRpcResponse HandleInitialize(
    object? requestId,
    IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings,
    ref List<Customer>? customers,
    ref List<Product>? products,
    ref List<Order>? orders)
{
    // Cargar datos durante la inicialización
    customers = LoadData<Customer>("../../../data/customers.json");
    products = LoadData<Product>("../../../data/products.json");
    orders = LoadData<Order>("../../../data/orders.json");

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { tools = new { } },
            serverInfo = new
            {
                name = settings.Value.Server.Name,
                version = settings.Value.Server.Version
            }
        },
        Id = requestId
    };
}

static JsonRpcResponse HandleToolsList(object? requestId)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            tools = new[]
            {
                SearchCustomersTool.GetDefinition(),
                GetOrderDetailsTool.GetDefinition(),
                CalculateMetricsTool.GetDefinition()
            }
        },
        Id = requestId
    };
}

static JsonRpcResponse HandleToolsCall(
    object? requestId,
    IDictionary<string, object>? parameters,
    List<Customer>? customers,
    List<Product>? products,
    List<Order>? orders)
{
    // Validar que los datos estén inicializados
    if (customers == null || products == null || orders == null)
    {
        throw new InvalidOperationException("Los datos no han sido inicializados. Debe llamar a 'initialize' primero.");
    }

    // Parsear el nombre de la herramienta
    string? toolName = null;
    if (parameters != null && parameters.TryGetValue("name", out var nameValue))
    {
        if (nameValue is JsonElement nameElement)
        {
            toolName = nameElement.GetString();
        }
        else if (nameValue is string strValue)
        {
            toolName = strValue;
        }
    }

    // Parsear los argumentos
    Dictionary<string, JsonElement> arguments;
    if (parameters != null && parameters.TryGetValue("arguments", out var argsValue))
    {
        string argumentsJson;
        if (argsValue is JsonElement argsElement)
        {
            argumentsJson = argsElement.GetRawText();
        }
        else
        {
            argumentsJson = JsonSerializer.Serialize(argsValue);
        }
        arguments = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson)
                    ?? new Dictionary<string, JsonElement>();
    }
    else
    {
        arguments = new Dictionary<string, JsonElement>();
    }

    var result = toolName switch
    {
        "search_customers" => SearchCustomersTool.Execute(arguments, customers),
        "get_order_details" => GetOrderDetailsTool.Execute(arguments, orders, customers, products),
        "calculate_metrics" => CalculateMetricsTool.Execute(arguments, orders, products),
        _ => throw new ArgumentException($"Unknown tool: {toolName}")
    };

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = result,
        Id = requestId
    };
}

static JsonRpcResponse CreateErrorResponse(int code, string message, object? data, object? id)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Error = new JsonRpcError { Code = code, Message = message, Data = data },
        Id = id
    };
}

static List<T> LoadData<T>(string path)
{
    var json = File.ReadAllText(path);
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
}
```

**✅ Checkpoint**: Compilación sin errores (`dotnet build`).

---

### Paso 4: Probar las Herramientas (2 minutos)

#### 4.1 Ejecutar servidor

```powershell
cd src/McpWorkshop.Servers/Exercise2Server
dotnet run
```

Deberías ver:

```text
info: Now listening on: http://localhost:5002
```

#### 4.2 Verificar Health Check

```powershell
Invoke-WebRequest -Uri "http://localhost:5002" -Method GET
```

**Respuesta esperada**: Status 200 con JSON `{"status": "healthy", "server": "Exercise2Server", ...}`

#### 4.2 Prueba 1: Tools/List

Terminal 2:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/list"
    params = @{}
    id = "list-tools"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Resultado esperado**: Array con 3 herramientas (`search_customers`, `get_order_details`, `calculate_metrics`).

✅ **PASS**

#### 4.3 Prueba 2: Search Customers (por nombre)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "search_customers"
        arguments = @{ name = "Carlos" }
    }
    id = "call-search"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Resultado esperado**: Clientes con "Carlos" en el nombre.

✅ **PASS**

#### 4.4 Prueba 3: Get Order Details

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "get_order_details"
        arguments = @{ orderId = 1001 }
    }
    id = "call-order"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Resultado esperado**: Detalles del pedido `1001` con información del cliente y producto.

✅ **PASS**

#### 4.5 Prueba 4: Calculate Metrics (ventas totales)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "calculate_metrics"
        arguments = @{ metricType = "sales" }
    }
    id = "call-metrics"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Resultado esperado**: `"Total de ventas: $XX,XXX.XX"`.

✅ **PASS**

---

## ✅ Criterios de Éxito

Has completado el ejercicio exitosamente si:

-   [ ] El servidor compila sin errores
-   [ ] `tools/list` devuelve 3 herramientas
-   [ ] `search_customers` filtra correctamente por nombre y/o país
-   [ ] `get_order_details` devuelve información combinada de order + customer + product
-   [ ] `calculate_metrics` calcula sales, average y top_products
-   [ ] Los parámetros se validan según JSON Schema (prueba con parámetros inválidos)

---

## 🐛 Solución de Problemas

### Error: "The parameter 'orderId' is required"

**Causa**: No se envió el parámetro requerido.

**Solución**: Verifica que el objeto `arguments` incluye el parámetro:

```powershell
arguments = @{ orderId = 1 }  # Debe estar presente
```

### Error: "Unknown tool: xxx"

**Causa**: El nombre de la herramienta no coincide.

**Solución**: Verifica que el `name` en `tools/call` es exacto:

```powershell
name = "search_customers"  # Debe ser exactamente este string
```

### Error: "No se encontró el pedido con ID X"

**Causa**: El ID no existe en `orders.json`.

**Solución**: Lista los IDs disponibles primero:

```powershell
Get-Content data/orders.json | ConvertFrom-Json | Select-Object -ExpandProperty Id
```

Usa un ID válido en la prueba.

### Error: Compilación falla con "Type 'Customer' is not defined"

**Causa**: Los modelos no se copiaron correctamente.

**Solución**: Verifica que existen los 3 archivos en `Models/`:

```powershell
Get-ChildItem src/McpWorkshop.Servers/Exercise2Server/Models/
# Debe mostrar: Customer.cs, Product.cs, Order.cs
```

---

## 🚀 Extensiones Opcionales

### Extensión 1: Herramienta de Filtro Avanzado

Crea `FilterOrdersTool.cs` que filtre pedidos por:

-   Rango de fechas (`startDate`, `endDate`)
-   Status (`pending`, `completed`, `cancelled`)
-   Monto mínimo (`minAmount`)

### Extensión 2: Validación de Parámetros

Agrega validación explícita en las herramientas:

```csharp
if (arguments.TryGetValue("orderId", out var orderIdElement))
{
    if (!orderIdElement.TryGetInt32(out var orderId) || orderId <= 0)
    {
        throw new ArgumentException("orderId debe ser un entero positivo");
    }
}
```

### Extensión 3: Paginación

Agrega parámetros `page` y `pageSize` a `search_customers`:

```csharp
var page = arguments.TryGetValue("page", out var pageElem) ? pageElem.GetInt32() : 1;
var pageSize = arguments.TryGetValue("pageSize", out var sizeElem) ? sizeElem.GetInt32() : 10;

var paginatedResults = query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

---

## 📚 Conceptos Aprendidos

### 1. JSON Schema para Definición de Herramientas

-   `inputSchema`: Define la estructura de parámetros
-   `properties`: Nombre y tipo de cada parámetro
-   `required`: Array de parámetros obligatorios
-   `enum`: Valores permitidos para un parámetro

### 2. Deserialización de Parámetros

-   `JsonElement`: Tipo dinámico para parámetros desconocidos
-   `GetString()`, `GetInt32()`: Conversión de tipos
-   `TryGetValue()`: Verificación segura de existencia de parámetros

### 3. Herramientas vs Recursos

-   **Recursos**: Datos pasivos, expuestos vía URIs
-   **Herramientas**: Operaciones activas, invocadas con parámetros
-   **Validación**: JSON Schema asegura que los parámetros son correctos

### 4. Logging Estructurado

-   Cada invocación de herramienta se registra con `LogRequest` / `LogResponse`
-   Útil para auditoría y debugging

---

## 🎓 Próximo Paso

**Ejercicio 3**: Seguridad y Autenticación (20 min)

En el siguiente ejercicio aprenderás a:

-   Implementar autenticación con JWT
-   Configurar autorización basada en scopes
-   Aplicar rate limiting por usuario
-   Registrar eventos de seguridad con logging estructurado

---

## 📖 Recursos Adicionales

-   **Documentación MCP - Tools**: https://modelcontextprotocol.io/specification/2025-06-18
-   **JSON Schema**: https://json-schema.org/understanding-json-schema/

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
