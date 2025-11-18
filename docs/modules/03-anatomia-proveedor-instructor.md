# Bloque 3: Anatomía de un Proveedor - Notas para el Instructor

**Duración objetivo**: 20 minutos  
**Estilo**: Live coding en vivo, con explicaciones claras y pruebas inmediatas

---

## ⏱️ Cronometraje Detallado

| Sección                           | Tiempo | Acumulado | Checkpoint          |
| --------------------------------- | ------ | --------- | ------------------- |
| Crear proyecto y estructura       | 3 min  | 3 min     | Proyecto compilando |
| Implementar modelo Customer       | 2 min  | 5 min     | Clase creada        |
| Implementar Program.cs (handlers) | 8 min  | 13 min    | Código completo     |
| Ejecutar servidor                 | 2 min  | 15 min    | Servidor corriendo  |
| Pruebas con Invoke-RestMethod     | 5 min  | 20 min    | 3 tests exitosos    |

**⚠️ Alerta de tiempo**: Si llegas a minuto 15 sin tener el servidor corriendo, abrevia las explicaciones de código y enfócate en las pruebas.

---

## 🎬 Setup Previo (5 minutos antes del bloque)

### Checklist Pre-Live Coding

-   [ ] Terminal PowerShell abierta y visible (fuente 16-18pt)
-   [ ] Visual Studio Code con solución `McpWorkshop.sln` cargada
-   [ ] Script `create-sample-data.ps1` ya ejecutado (Data/customers.json existe)
-   [ ] Segunda terminal preparada para pruebas (split screen)
-   [ ] Snippets de código preparados (por si hay problemas de tipeo)
-   [ ] Backup: Carpeta `DemoServer-backup/` con código completo

**CRÍTICO**: Ten el código completo en un snippet accesible. Si hay problemas técnicos, pega y explica.

---

## 🎤 Script de Apertura (1 minuto)

> "Perfecto. Ahora vamos a crear un servidor MCP desde cero. No voy a usar plantillas ni magia - van a ver cada línea de código. Al final tendremos un servidor funcional que responde solicitudes MCP reales.
>
> Voy a ir rápido pero pausado. Si alguien se pierde, no se preocupen: todo el código estará en GitHub y lo repasaremos en el Ejercicio 1. ¿Listos? Allá vamos."

**Acción**: Abrir terminal en modo pantalla completa.

---

## 📝 Paso 1: Crear Proyecto (3 minutos)

### Comandos a Ejecutar

```powershell
# Navegar a carpeta de servidores
cd src/McpWorkshop.Servers

# Crear proyecto web minimal
dotnet new web -n DemoServer -f net10.0
```

**Narración**:

> "Usamos `dotnet new web` porque es la plantilla más ligera. No necesitamos controllers, solo un endpoint HTTP simple."

```powershell
# Entrar al proyecto
cd DemoServer

# Agregar referencia a librería compartida
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj
```

**Narración**:

> "Referenciamos nuestra librería compartida donde tenemos `McpServerBase`, el logger, y las configuraciones."

```powershell
# Agregar a solución
cd ../../..
dotnet sln add src/McpWorkshop.Servers/DemoServer/DemoServer.csproj

# Verificar compilación
dotnet build
```

**Checkpoint**: Deberías ver "Build succeeded" sin errores.

### Si Hay Errores

**Error común**: "The template 'web' could not be found"

**Solución**: Verificar .NET 10.0 SDK instalado con `dotnet --version`

---

## 📦 Paso 2: Modelo de Datos (2 minutos)

### Crear Carpeta y Archivo

```powershell
# Crear carpeta Models
cd src/McpWorkshop.Servers/DemoServer
mkdir Models
```

**En Visual Studio Code**: Crear archivo `Models/Customer.cs`

### Código a Escribir

```csharp
namespace DemoServer.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime Created { get; set; }
}
```

**Narración**:

> "Modelo simple de cliente. Propiedades auto-implementadas de C#. El `= string.Empty` evita warnings de nullable reference types."

**Tip de enseñanza**: No expliques cada propiedad en detalle, es obvio.

---

## 💻 Paso 3: Implementar Servidor (8 minutos)

### Estrategia de Live Coding

**IMPORTANTE**: No escribas todo el archivo de golpe. Divídelo en 4 partes:

#### Parte 1: Setup y Configuración (2 min)

Abre `Program.cs` y reemplaza con:

```csharp
using System.Text.Json;
using DemoServer.Models;
using McpWorkshop.Shared.Logging;
using McpWorkshop.Shared.Mcp;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services.AddSingleton<IStructuredLogger, StructuredLogger>();
builder.Services.Configure<McpWorkshop.Shared.Configuration.WorkshopSettings>(options =>
{
    options.Server.Name = "DemoServer";
    options.Server.Version = "1.0.0";
    options.Server.ProtocolVersion = "2024-11-05";
});

var app = builder.Build();
```

**Narración durante tipeo**:

> "Configuramos inyección de dependencias. Registramos el logger estructurado y la configuración del servidor. Esto es ASP.NET Core estándar."

**Pausa**: "¿Alguna pregunta hasta aquí?"

#### Parte 2: Endpoint MCP (2 min)

Continúa en `Program.cs`:

```csharp
// Cargar datos de muestra
var customers = LoadCustomers();

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
            "resources/read" => HandleResourcesRead(request.Params, customers),
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

app.Run("http://localhost:5000");
```

**Narración**:

> "Este es el corazón. Un endpoint POST en `/mcp`. Usamos pattern matching con `switch` para rutear. Si el método no existe, devolvemos error -32601 'Method not found', que es el estándar JSON-RPC."

#### Parte 3: Handlers (3 min)

Continúa agregando los métodos helper:

```csharp
// Métodos helper
static JsonRpcResponse HandleInitialize(IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { resources = new { }, tools = new { } },
            serverInfo = new
            {
                name = settings.Value.Server.Name,
                version = settings.Value.Server.Version
            }
        },
        Id = "init"
    };
}
```

**Narración**:

> "`initialize` es el handshake. Devolvemos versión de protocolo, capabilities, y info del servidor. Capabilities dice que tenemos recursos y herramientas."

```csharp
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
                    description = "Lista de clientes del sistema",
                    mimeType = "application/json"
                }
            }
        },
        Id = "list"
    };
}
```

**Narración**:

> "`resources/list` devuelve un array de recursos disponibles. Solo uno: `mcp://customers`. El URI es nuestro esquema personalizado."

```csharp
static JsonRpcResponse HandleResourcesRead(object? parameters, List<Customer> customers)
{
    var paramsJson = JsonSerializer.Serialize(parameters);
    var paramsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(paramsJson);
    var uri = paramsDict?["uri"].GetString();

    if (uri == "mcp://customers")
    {
        return new JsonRpcResponse
        {
            JsonRpc = "2.0",
            Result = new
            {
                contents = new[]
                {
                    new
                    {
                        uri = "mcp://customers",
                        mimeType = "application/json",
                        text = JsonSerializer.Serialize(customers, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            },
            Id = "read"
        };
    }

    throw new ArgumentException($"Unknown resource URI: {uri}");
}
```

**Narración**:

> "`resources/read` recibe el URI a leer. Validamos que sea `mcp://customers` y devolvemos el JSON de clientes. Si el URI no existe, lanzamos excepción."

#### Parte 4: Helpers Finales (1 min)

```csharp
static JsonRpcResponse CreateErrorResponse(int code, string message, object? data, object? id)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Error = new JsonRpcError { Code = code, Message = message, Data = data },
        Id = id
    };
}

static List<Customer> LoadCustomers()
{
    var json = File.ReadAllText("../../../Data/customers.json");
    return JsonSerializer.Deserialize<List<Customer>>(json) ?? new List<Customer>();
}
```

**Narración**:

> "Helper para errores y carga de datos desde JSON. En producción `LoadCustomers` sería una consulta SQL."

**Guardar archivo**: Ctrl+S

---

## 🚀 Paso 4: Ejecutar Servidor (2 minutos)

### Compilar y Ejecutar

```powershell
# En terminal
cd src/McpWorkshop.Servers/DemoServer
dotnet run
```

**Resultado esperado**:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Narración**:

> "¡El servidor está corriendo! Puerto 5000. Ahora vamos a probarlo."

**Abrir segunda terminal** (split screen) para pruebas.

---

## 🧪 Paso 5: Pruebas (5 minutos)

### Test 1: Initialize (1.5 min)

**En segunda terminal**:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{ name = "TestClient"; version = "1.0.0" }
    }
    id = "init-001"
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:5000/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Mostrar resultado en pantalla** (JSON formateado).

**Narración**:

> "Perfecto. El servidor respondió con su serverInfo y capabilities. El handshake funcionó."

### Test 2: Resources/List (1.5 min)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/list"
    params = @{}
    id = "list-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Narración**:

> "Ahora listamos recursos. Vemos `mcp://customers` con su descripción. Esto es el descubrimiento dinámico."

### Test 3: Resources/Read (2 min)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://customers" }
    id = "read-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/mcp" -Method POST -Body $body -ContentType "application/json"
```

**Resultado**: JSON con array de clientes.

**Narración**:

> "¡Excelente! Recibimos los 5 clientes. Ana García, Carlos Méndez, etc. Esto es un servidor MCP funcional completo. Initialize, list, read - el flujo completo."

**Mostrar logs en la primera terminal** (donde corre el servidor):

> "Ven en el servidor los logs estructurados de cada request. Esto viene de nuestro `IStructuredLogger`."

---

## ⚠️ Contingencias y Plan B

### Si el Live Coding Falla Técnicamente

**Plan B**: Tener DemoServer pre-compilado en carpeta backup.

```powershell
# Copiar código funcional
cp -r backup/DemoServer src/McpWorkshop.Servers/
cd src/McpWorkshop.Servers/DemoServer
dotnet run
```

**Narración**:

> "Por temas de tiempo, voy a usar el código pre-preparado. El resultado es idéntico. Ahora las pruebas..."

### Si Invoke-RestMethod Falla

**Alternativa**: Usa `curl` (si está instalado):

```bash
curl -X POST http://localhost:5000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"initialize","params":{},"id":"init"}'
```

### Si el Puerto 5000 Está Ocupado

```csharp
// Cambiar en Program.cs
app.Run("http://localhost:5001");
```

**Re-ejecutar**: `dotnet run`

---

## 📊 Señales de Éxito del Bloque 3

Al finalizar, deberías observar:

✅ **Servidor funcional**:

-   Compiló sin errores
-   Responde en http://localhost:5000
-   Logs estructurados visibles

✅ **3 tests exitosos**:

-   Initialize devolvió serverInfo
-   Resources/list mostró `mcp://customers`
-   Resources/read devolvió JSON de clientes

✅ **Comprensión de audiencia**:

-   Entienden el flujo initialize → list → read
-   Ven la conexión entre código C# y JSON-RPC
-   Están listos para crear su propio servidor

❌ **Señales de alarma**:

-   Errores de compilación no resueltos en 2 minutos → usar backup
-   Más de 3 preguntas sobre sintaxis C# → demasiado bajo nivel
-   Confusión sobre qué es cliente vs servidor → repetir en Ejercicio 1

---

## 🔄 Transición al Bloque 4

### Frase de Cierre

> "Y eso es todo. En 20 minutos creamos un servidor MCP funcional. 120 líneas de código, tres métodos, infinitas posibilidades.
>
> Ahora es su turno. En el Ejercicio 1 van a crear su propio servidor similar, pero con su propia twist. Les daré 15 minutos, estaré circulando para ayudar. Abran Visual Studio Code y vamos."

**Acción física**:

-   Detener el servidor (Ctrl+C)
-   Proyectar las instrucciones del Ejercicio 1
-   Iniciar cronómetro visible (15 minutos)

---

## 📝 Notas Post-Live Coding

**Para mejorar en próximos talleres**:

-   ¿Cuánto tiempo real tomó? ****\_\_****
-   ¿Qué error inesperado surgió? ****\_\_****
-   ¿Qué analogía funcionó mejor? ****\_\_****
-   ¿La audiencia pudo seguir el ritmo? ****\_\_****

---

**Preparado por**: Instructor del taller MCP  
**Última revisión**: Noviembre 2025
