# Ejercicio 1: Guía del Instructor (15 minutos)

**Propósito**: Primer ejercicio práctico donde los asistentes crean un servidor MCP funcional.  
**Formato**: Guiado (instructor explica, asistentes replican en vivo).  
**Nivel**: Básico - todos deben completarlo.

---

## ⏱️ Timing Detallado

| Minuto | Actividad                         | Duración  |
| ------ | --------------------------------- | --------- |
| 0-3    | Crear proyecto y estructura       | 3 min     |
| 3-5    | Crear modelos (Customer, Product) | 2 min     |
| 5-12   | Implementar Program.cs completo   | 7 min     |
| 12-15  | Ejecutar y probar (4 tests)       | 3 min     |
| **15** | **Finalizar ejercicio**           | **TOTAL** |

---

## 🎯 Objetivo del Instructor

Al terminar este bloque, los asistentes deben:

1. ✅ Haber creado su primer servidor MCP desde cero
2. ✅ Comprender el flujo initialize → list → read
3. ✅ Ver logs estructurados en acción
4. ✅ Tener confianza para el Ejercicio 2

---

## 🧩 Pre-Setup del Instructor

**Antes de comenzar el ejercicio**:

-   [ ] Cierra el DemoServer si está corriendo (`Ctrl+C` en la terminal del Bloque 3)
-   [ ] Verifica que `Data/customers.json` y `Data/products.json` existen
-   [ ] Abre Visual Studio Code en la carpeta raíz del repositorio
-   [ ] Prepara **dos terminales** en VS Code (divísion horizontal):
    -   Terminal 1: Para ejecutar el servidor
    -   Terminal 2: Para ejecutar los tests con PowerShell
-   [ ] Ten el código completo de `Program.cs` en un archivo de respaldo (por si hay problemas de tipeo)
-   [ ] Confirma que el puerto **5001** está libre:

```powershell
netstat -ano | Select-String "5001"
# No debe devolver nada
```

---

## 📋 Guion del Ejercicio

### Minutos 0-3: Crear Proyecto (Guiado)

**Script para decir**:

> "Vamos a crear nuestro primer servidor MCP desde cero. Este será un servidor de recursos estáticos que expone clientes y productos. Voy a ir paso a paso y ustedes replican en sus máquinas."

#### Paso 1: Crear proyecto

**Narración mientras escribes**:

> "Primero navegamos a la carpeta de servidores..."

```powershell
cd src/McpWorkshop.Servers
```

> "Creamos un proyecto web con .NET 10 llamado Exercise1Server..."

```powershell
dotnet new web -n Exercise1Server -f net10.0
cd Exercise1Server
```

> "Agregamos la referencia a nuestra librería compartida..."

```powershell
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj
```

> "Y lo agregamos a la solución para que Visual Studio lo reconozca..."

```powershell
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise1Server/Exercise1Server.csproj
```

**Pausa para preguntas** (30 segundos):

> "¿Todos tienen el proyecto creado? Si ven algún error rojo, levanten la mano."

**Validación rápida**:

```powershell
dotnet build
# Debe compilar sin errores
```

---

### Minutos 3-5: Crear Modelos (Guiado)

**Script para decir**:

> "Ahora creamos dos modelos simples: Customer y Product. Estos representan los datos que vamos a exponer."

#### Paso 2.1: Customer

**Narración**:

> "Creamos la carpeta Models y el archivo Customer.cs..."

```powershell
cd src/McpWorkshop.Servers/Exercise1Server
mkdir Models
code Models/Customer.cs
```

**Escribe el código mientras narras**:

> "Un cliente tiene ID, nombre, email, país y fecha de creación. Todos los strings los inicializamos vacíos para evitar nulls."

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

#### Paso 2.2: Product

**Narración**:

> "El modelo Product es similar..."

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

**Pausa para sincronización** (20 segundos):

> "¿Todos tienen los dos modelos? Guarden los archivos con Ctrl+S."

---

### Minutos 5-12: Implementar Program.cs (Explicativo)

**Script para decir**:

> "Ahora viene la parte interesante: vamos a implementar el servidor completo en Program.cs. Voy a explicar cada sección mientras escribo."

**IMPORTANTE**: Esta es la sección crítica. Si el tiempo corre, salta a la versión de respaldo (ver Contingencia B).

#### Parte 1: Configuración (2 minutos)

**Narración**:

> "Abrimos Program.cs y reemplazamos todo..."

```csharp
using System.Text.Json;
using Exercise1Server.Models;
using McpWorkshop.Shared.Logging;
using McpWorkshop.Shared.Mcp;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
```

> "Registramos el logger estructurado y configuramos el servidor con nombre, versión y puerto 5001..."

```csharp
builder.Services.AddSingleton<IStructuredLogger, StructuredLogger>();
builder.Services.Configure<McpWorkshop.Shared.Configuration.WorkshopSettings>(options =>
{
    options.Server.Name = "Exercise1Server";
    options.Server.Version = "1.0.0";
    options.Server.ProtocolVersion = "2024-11-05";
    options.Server.Port = 5001;
});

var app = builder.Build();
```

> "Cargamos los datos de muestra que generamos al inicio del taller..."

```csharp
var customers = LoadData<Customer>("../../../Data/customers.json");
var products = LoadData<Product>("../../../Data/products.json");
```

**Pausa** (10 segundos):

> "¿Todos van siguiendo? Si alguien se perdió, no pasa nada, luego sincronizamos."

#### Parte 2: Endpoint MCP (3 minutos)

**Narración**:

> "Ahora el endpoint principal. Recibe solicitudes JSON-RPC y usa pattern matching para enrutar..."

```csharp
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
```

> "Esto es lo que vimos en el Bloque 3: mismo patrón, pero ahora con recursos reales."

#### Parte 3: Handlers (3 minutos)

**Narración rápida**:

> "Ahora los tres handlers. Initialize devuelve las capacidades del servidor..."

```csharp
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

> "ResourcesList enumera los recursos disponibles con URIs mcp://..."

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
```

> "Y ResourcesRead devuelve el contenido según la URI solicitada..."

```csharp
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
                new { uri, mimeType = "application/json", text = content }
            }
        },
        Id = "read"
    };
}
```

> "Dos métodos auxiliares más: uno para errores y otro para cargar JSON..."

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

static List<T> LoadData<T>(string path)
{
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
}
```

**Pausa crítica** (15 segundos):

> "Guarden todo. Vamos a compilar para ver si hay errores."

```powershell
dotnet build
```

> "Si compila limpio, ¡excelente! Si hay errores rojos, miren la línea y comparen con la documentación."

---

### Minutos 12-15: Ejecutar y Probar (Validación)

**Script para decir**:

> "Momento de verdad: vamos a ejecutar el servidor y probarlo con PowerShell."

#### Paso 1: Ejecutar servidor

**Narración**:

> "En la Terminal 1, ejecutamos..."

```powershell
cd src/McpWorkshop.Servers/Exercise1Server
dotnet run
```

> "Deben ver 'Now listening on: http://localhost:5001'. Si ven ese mensaje, el servidor está vivo."

#### Paso 2: Pruebas (Terminal 2)

**Narración**:

> "Abrimos la Terminal 2 y ejecutamos los tests. Primero, initialize..."

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

Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Deben ver el serverInfo con 'Exercise1Server'. ✅ Funciona."

**Test resources/list**:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/list"
    params = @{}
    id = "list-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Deben ver dos recursos: customers y products. ✅ Perfecto."

**Test resources/read (customers)**:

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://customers" }
    id = "read-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Aquí deben ver un JSON grande con la lista de clientes. Si lo ven, ¡su servidor está funcionando al 100%! 🎉"

**Mensaje final**:

> "Felicitaciones, acaban de crear su primer servidor MCP funcional. En el siguiente ejercicio vamos a añadir herramientas con parámetros dinámicos."

---

## 🚨 Contingencias

### Contingencia A: Errores de compilación (Minuto 10+)

**Problema**: Varios asistentes tienen errores de sintaxis.

**Acción**:

1. **Pausa general** (30 segundos):

    > "Veo que varios tienen errores. Voy a compartir el código completo por chat."

2. **Copiar/Pegar**:

    - Comparte el archivo `Program.cs` completo vía chat de Teams/Zoom
    - Indica: "Copien este código completo, reemplacen todo su Program.cs"

3. **Continuar con pruebas**:

    - Verifica que compila: `dotnet build`
    - Sigue con las pruebas (minuto 12-15)

**Ganancia de tiempo**: +2 minutos recuperados.

---

### Contingencia B: Tiempo insuficiente (Minuto 8+)

**Problema**: Llevas 8 minutos y aún no terminaste de escribir Program.cs.

**Acción**:

1. **Detener tipeo**:

    > "Para ganar tiempo, voy a compartir el código completo. Ustedes cópienlo y luego lo revisamos juntos."

2. **Compartir código de respaldo**:

    - Archivo pre-preparado: `src/McpWorkshop.Servers/Exercise1Server.BACKUP/Program.cs`
    - Los asistentes copian/pegan

3. **Explicación post-mortem** (1 minuto):

    > "El código que copiaron hace tres cosas: 1) Initialize, 2) List resources, 3) Read resources. Vamos a probarlo ahora."

4. **Continuar con pruebas**:

    - Ejecutar servidor
    - Hacer los 4 tests rápido (2 minutos)

**Ganancia de tiempo**: Terminas en el minuto 15.

---

### Contingencia C: Puerto 5001 ocupado

**Problema**: Varios asistentes tienen el DemoServer aún corriendo.

**Acción rápida**:

> "Si ven 'Address already in use', presionen Ctrl+C en todas las terminales con servidores corriendo. Luego vuelvan a ejecutar `dotnet run`."

**Alternativa**:

-   Cambia el puerto a **5002** en `Program.cs`:

```csharp
app.Run("http://localhost:5002");
```

-   Actualiza las URLs de prueba a `http://localhost:5002/mcp`

---

### Contingencia D: Datos de muestra no existen

**Problema**: Error "Cannot find file customers.json".

**Acción inmediata**:

> "Parece que no se generaron los datos. Ejecuten esto ahora:"

```powershell
.\scripts\create-sample-data.ps1
```

**Si persiste el error**:

-   Verifica la ruta relativa en `LoadData`:

```csharp
var customers = LoadData<Customer>("../../../../Data/customers.json");
```

---

## ✅ Validación de Completitud

Al terminar el ejercicio, pregunta:

> "¿Cuántos pudieron ejecutar los 4 tests exitosamente?"

-   **>80% levanta la mano**: ✅ **Ejercicio exitoso**, continúa al Bloque 5.
-   **50-80% levanta la mano**: ⚠️ **Revisar problemas comunes**, da 2 minutos extra.
-   **<50% levanta la mano**: 🚨 **Contingencia crítica**, ofrece código completo funcionando.

---

## 📊 Métricas de Éxito

| Indicador                             | Objetivo | Resultado Real |
| ------------------------------------- | -------- | -------------- |
| Asistentes que compilaron sin errores | >85%     | \_\_\_ %       |
| Asistentes que ejecutaron servidor    | >80%     | \_\_\_ %       |
| Asistentes que pasaron los 4 tests    | >75%     | \_\_\_ %       |
| Tiempo total utilizado                | 15 min   | \_\_\_ min     |

---

## 🎓 Lecciones Aprendidas (Post-Ejercicio)

**Después del ejercicio, refuerza estos conceptos**:

1. **Pattern Matching en C#**: El `switch` con `=>` es poderoso para routing.
2. **Deserialización dinámica**: `JsonElement` permite parsear parámetros sin conocer la estructura exacta.
3. **Minimal APIs**: Con pocas líneas tienes un servidor HTTP completo.
4. **JSON-RPC 2.0**: El protocolo es simple: method, params, id.

**Pregunta de reflexión** (30 segundos):

> "¿Qué ventaja tiene usar URIs como `mcp://customers` en lugar de simples IDs numéricos?"

**Respuesta esperada**:

> "Las URIs son descriptivas, únicas, y permiten namespacing (por ejemplo, `mcp://tenant-a/customers` vs `mcp://tenant-b/customers`)."

---

## 🔗 Transición al Ejercicio 2

**Script de cierre** (30 segundos):

> "Excelente trabajo. Ahora tienen un servidor que expone datos estáticos. En el Ejercicio 2 vamos a añadir **herramientas** que aceptan parámetros: búsquedas, filtros, y cálculos dinámicos. Descansen 1 minuto, tomen agua, y continuamos."

**Checklist de transición**:

-   [ ] Los asistentes detienen el servidor (Ctrl+C)
-   [ ] Confirma que todos tienen el código funcionando (carpeta `Exercise1Server`)
-   [ ] Abre el documento del Ejercicio 2 en VS Code
-   [ ] Prepara la terminal para el siguiente proyecto

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
