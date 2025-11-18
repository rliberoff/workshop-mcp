# Ejercicio 1: Servidor MCP con Recursos Estáticos (15 minutos)

**Tipo**: Ejercicio guiado  
**Duración**: 15 minutos  
**Nivel**: Básico - Fundacional  
**Objetivo**: Crear tu primer servidor MCP que expone recursos JSON estáticos

---

## 🎯 Objetivos del Ejercicio

Al completar este ejercicio, habrás:

1. ✅ Creado un proyecto de servidor MCP desde cero
2. ✅ Implementado los métodos `initialize`, `resources/list`, `resources/read`
3. ✅ Expuesto al menos 2 recursos estáticos (clientes y productos)
4. ✅ Probado el servidor con solicitudes HTTP
5. ✅ Comprendido el flujo JSON-RPC 2.0 básico

---

## 📋 Prerrequisitos

Antes de comenzar, verifica que tienes:

-   [x] .NET 10.0 SDK instalado (`dotnet --version`)
-   [x] Visual Studio Code abierto
-   [x] Terminal PowerShell disponible
-   [x] Datos de muestra generados (`.\scripts\create-sample-data.ps1`)
-   [x] Repositorio clonado y McpWorkshop.sln cargado

---

## 📂 Estructura del Servidor a Crear

```
src/McpWorkshop.Servers/
└── Exercise1Server/
    ├── Program.cs              # Servidor principal
    ├── Exercise1Server.csproj  # Archivo de proyecto
    └── Models/
        ├── Customer.cs         # Modelo de cliente
        └── Product.cs          # Modelo de producto
```

---

## 🚀 Paso a Paso

### Paso 1: Crear el Proyecto (3 minutos)

#### 1.1 Crear proyecto web

```powershell
# Navegar a carpeta de servidores
cd src/McpWorkshop.Servers

# Crear proyecto
dotnet new web -n Exercise1Server -f net10.0
cd Exercise1Server
```

#### 1.2 Agregar referencias

```powershell
# Referenciar librería compartida
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj

# Agregar a solución
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise1Server/Exercise1Server.csproj

# Verificar compilación
dotnet build
```

**✅ Checkpoint**: Debe compilar sin errores.

---

### Paso 2: Crear Modelos (2 minutos)

#### 2.1 Modelo Customer

Crea `Models/Customer.cs`:

```csharp
namespace Exercise1Server.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime Created { get; set; }
}
```

#### 2.2 Modelo Product

Crea `Models/Product.cs`:

```csharp
namespace Exercise1Server.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InStock { get; set; }
}
```

**✅ Checkpoint**: Dos modelos creados.

---

### Paso 3: Implementar Servidor (7 minutos)

Abre `Program.cs` y reemplaza todo el contenido con:

```csharp
using System.Text.Json;
using Exercise1Server.Models;
using McpWorkshop.Shared.Logging;
using McpWorkshop.Shared.Mcp;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services.AddSingleton<IStructuredLogger, StructuredLogger>();
builder.Services.Configure<McpWorkshop.Shared.Configuration.WorkshopSettings>(options =>
{
    options.Server.Name = "Exercise1Server";
    options.Server.Version = "1.0.0";
    options.Server.ProtocolVersion = "2024-11-05";
    options.Server.Port = 5001; // Diferente al DemoServer
});

var app = builder.Build();

// Cargar datos de muestra
var customers = LoadData<Customer>("../../../Data/customers.json");
var products = LoadData<Product>("../../../Data/products.json");

// Endpoint MCP
app.MapPost("/mcp", async (
    JsonRpcRequest request,
    IStructuredLogger logger,
    IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings) =>
{
    var requestId = request.Id?.ToString() ?? "unknown";
    logger.LogRequest(request.Method, requestId, request.Params);

    try
    {
        var response = request.Method switch
        {
            "initialize" => HandleInitialize(settings),
            "resources/list" => HandleResourcesList(),
            "resources/read" => HandleResourcesRead(request.Params, customers, products),
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

app.Run("http://localhost:5001");

// Métodos Helper
static JsonRpcResponse HandleInitialize(IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                resources = new { },
                tools = new { }
            },
            serverInfo = new
            {
                name = settings.Value.Server.Name,
                version = settings.Value.Server.Version
            }
        },
        Id = "init"
    };
}

static JsonRpcResponse HandleResourcesList()
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            resources = new[]
            {
                new
                {
                    uri = "mcp://customers",
                    name = "Customers Database",
                    description = "Lista completa de clientes registrados",
                    mimeType = "application/json"
                },
                new
                {
                    uri = "mcp://products",
                    name = "Products Catalog",
                    description = "Catálogo de productos disponibles",
                    mimeType = "application/json"
                }
            }
        },
        Id = "list"
    };
}

static JsonRpcResponse HandleResourcesRead(
    object? parameters,
    List<Customer> customers,
    List<Product> products)
{
    var paramsJson = JsonSerializer.Serialize(parameters);
    var paramsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(paramsJson);
    var uri = paramsDict?["uri"].GetString();

    var content = uri switch
    {
        "mcp://customers" => JsonSerializer.Serialize(customers, new JsonSerializerOptions { WriteIndented = true }),
        "mcp://products" => JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true }),
        _ => throw new ArgumentException($"Unknown resource URI: {uri}")
    };

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            contents = new[]
            {
                new
                {
                    uri,
                    mimeType = "application/json",
                    text = content
                }
            }
        },
        Id = "read"
    };
}

static JsonRpcResponse CreateErrorResponse(int code, string message, object? data, object? id)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Error = new JsonRpcError
        {
            Code = code,
            Message = message,
            Data = data
        },
        Id = id
    };
}

static List<T> LoadData<T>(string path)
{
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
}
```

**✅ Checkpoint**: El código compila sin errores.

---

### Paso 4: Ejecutar y Probar (3 minutos)

#### 4.1 Iniciar el servidor

```powershell
cd src/McpWorkshop.Servers/Exercise1Server
dotnet run
```

Deberías ver:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

#### 4.2 Prueba 1: Initialize

Abre una **segunda terminal** y ejecuta:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{ name = "Exercise1Client"; version = "1.0.0" }
    }
    id = "init-001"
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:5001/mcp" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Resultado esperado**:

```json
{
    "jsonrpc": "2.0",
    "result": {
        "protocolVersion": "2024-11-05",
        "capabilities": { "resources": {}, "tools": {} },
        "serverInfo": {
            "name": "Exercise1Server",
            "version": "1.0.0"
        }
    },
    "id": "init-001"
}
```

✅ **PASS**: Initialize funciona.

#### 4.3 Prueba 2: Resources/List

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/list"
    params = @{}
    id = "list-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Resultado esperado**: Array con 2 recursos (`mcp://customers` y `mcp://products`).

✅ **PASS**: Listado de recursos funciona.

#### 4.4 Prueba 3: Resources/Read (Customers)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://customers" }
    id = "read-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Resultado esperado**: JSON con array de clientes.

✅ **PASS**: Lectura de clientes funciona.

#### 4.5 Prueba 4: Resources/Read (Products)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://products" }
    id = "read-002"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Resultado esperado**: JSON con array de productos.

✅ **PASS**: Lectura de productos funciona.

---

## ✅ Criterios de Éxito

Has completado el ejercicio exitosamente si:

-   [ ] El servidor compila sin errores
-   [ ] El servidor se ejecuta en `http://localhost:5001`
-   [ ] `initialize` devuelve serverInfo correcto
-   [ ] `resources/list` muestra 2 recursos (customers y products)
-   [ ] `resources/read` devuelve datos de customers
-   [ ] `resources/read` devuelve datos de products
-   [ ] Los logs estructurados aparecen en la terminal del servidor

**Validación rápida**: Si todos los checkboxes están marcados, ¡has terminado! 🎉

---

## 🐛 Solución de Problemas

### Error: "Port 5001 already in use"

**Solución**: Cambia el puerto en `Program.cs`:

```csharp
app.Run("http://localhost:5002");
```

Y actualiza las URLs de prueba a `http://localhost:5002/mcp`.

### Error: "Cannot find customers.json"

**Solución**: Verifica que ejecutaste el script de datos:

```powershell
.\scripts\create-sample-data.ps1
Get-Item Data/customers.json  # Debe existir
```

Ajusta la ruta en `LoadData` si es necesario:

```csharp
var customers = LoadData<Customer>("../../../../Data/customers.json");
```

### Error: "JsonException: The JSON value could not be converted"

**Solución**: Asegúrate de usar `-Depth 10` en `ConvertTo-Json`:

```powershell
$body | ConvertTo-Json -Depth 10
```

### Error: Compilación falla con "Type or namespace 'McpWorkshop' could not be found"

**Solución**: Verifica la referencia:

```powershell
dotnet list reference  # Debe mostrar McpWorkshop.Shared
```

Si no está, agrégala:

```powershell
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj
```

---

## 🚀 Extensiones Opcionales

Si terminas antes de los 15 minutos, prueba estas extensiones:

### Extensión 1: Agregar Recurso de Pedidos

1. Crea `Models/Order.cs`:

```csharp
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

2. Carga los datos:

```csharp
var orders = LoadData<Order>("../../../Data/orders.json");
```

3. Agrega a `HandleResourcesList`:

```csharp
new
{
    uri = "mcp://orders",
    name = "Orders",
    description = "Historial de pedidos",
    mimeType = "application/json"
}
```

4. Agrega a `HandleResourcesRead`:

```csharp
"mcp://orders" => JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true })
```

### Extensión 2: Filtrar por País

Modifica `HandleResourcesRead` para aceptar parámetros opcionales:

```csharp
var country = paramsDict?["country"]?.GetString();

if (uri == "mcp://customers" && !string.IsNullOrEmpty(country))
{
    var filtered = customers.Where(c => c.Country == country).ToList();
    content = JsonSerializer.Serialize(filtered, new JsonSerializerOptions { WriteIndented = true });
}
```

Prueba:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://customers"; country = "España" }
    id = "read-filtered"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"
```

### Extensión 3: Agregar Metadata

Agrega timestamps a las respuestas:

```csharp
Result = new
{
    contents = new[] { ... },
    metadata = new
    {
        timestamp = DateTime.UtcNow,
        count = customers.Count
    }
}
```

---

## 📚 Conceptos Aprendidos

### 1. Inicialización del Servidor MCP

-   Configuración de servicios con DI (Dependency Injection)
-   Registro de logger y settings
-   ASP.NET Core Minimal API

### 2. Manejo de Solicitudes JSON-RPC

-   Pattern matching con `switch` expressions
-   Deserialización de parámetros dinámicos
-   Generación de respuestas estructuradas

### 3. Recursos Estáticos

-   URIs como identificadores (`mcp://resource-name`)
-   Listado dinámico de recursos disponibles
-   Lectura de contenido desde fuentes locales (JSON)

### 4. Manejo de Errores

-   Códigos de error estándar JSON-RPC (-32601, -32603)
-   Try-catch para excepciones
-   Logging de errores estructurado

---

## 🎓 Próximo Paso

**Ejercicio 2**: Consultas Paramétricas con Herramientas (20 min)

En el siguiente ejercicio aprenderás a:

-   Implementar herramientas invocables (`tools/call`)
-   Validar parámetros de entrada con JSON Schema
-   Ejecutar búsquedas y filtros dinámicos
-   Combinar múltiples fuentes de datos

---

## 📖 Recursos Adicionales

-   **Contrato de referencia**: `specs/001-mcp-workshop-course/contracts/exercise-1-static-resource.json`
-   **Código de ejemplo**: `src/McpWorkshop.Servers/DemoServer/`
-   **Documentación MCP**: https://spec.modelcontextprotocol.io/specification/2024-11-05/basic/resources/

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
