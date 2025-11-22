# MCP Workshop - Tarjeta de Referencia Rápida

Referencia rápida para asistentes durante ejercicios prácticos. Mantener esta página abierta en otra pantalla.

---

## 📡 Guía Rápida del Protocolo MCP

### Estructura de Mensajes JSON-RPC

```json
{
    "jsonrpc": "2.0",
    "method": "method_name",
    "params": {},
    "id": 1
}
```

### Métodos Principales

| Método           | Propósito                       | Parámetros Requeridos                           | Respuesta                             |
| ---------------- | ------------------------------- | ----------------------------------------------- | ------------------------------------- |
| `initialize`     | Handshake con el cliente        | `protocolVersion`, `capabilities`, `clientInfo` | Capacidades del servidor              |
| `resources/list` | Listar recursos disponibles     | Ninguno                                         | Array de descriptores de recursos     |
| `resources/read` | Obtener un recurso específico   | `uri`                                           | Contenido del recurso (texto/binario) |
| `tools/list`     | Listar herramientas disponibles | Ninguno                                         | Array de esquemas de herramientas     |
| `tools/call`     | Ejecutar herramienta            | `name`, `arguments`                             | Resultado de la ejecución             |
| `prompts/list`   | Listar plantillas de prompts    | Ninguno                                         | Array de prompts                      |
| `prompts/get`    | Obtener prompt por nombre       | `name`, `arguments`                             | Prompt renderizado                    |

### Códigos de Error Estándar

```text
-32700  Error de parsing
-32600  Solicitud inválida
-32601  Método no encontrado
-32602  Parámetros inválidos
-32603  Error interno
-32000 a -32099  Errores definidos por el servidor
```

---

## 🔧 Patrones Comunes en C#

### 1. Servidor MCP Básico (Ejercicio 1)

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();
    var request = JsonSerializer.Deserialize<JsonRpcRequest>(json);

    var response = request.Method switch
    {
        "initialize" => HandleInitialize(),
        "resources/list" => await HandleResourcesList(),
        "resources/read" => await HandleResourcesRead(request.Params),
        _ => JsonRpcError.MethodNotFound(request.Id)
    };

    await context.Response.WriteAsJsonAsync(response);
});

await app.RunAsync();
```

### 2. Herramienta con Esquema de Entrada (Ejercicio 2)

```csharp
public class GetCustomersTool
{
    public string Name => "get_customers";

    public object InputSchema => new
    {
        type = "object",
        properties = new
        {
            region = new { type = "string", description = "Filtrar por región" },
            limit = new { type = "integer", minimum = 1, maximum = 100 }
        },
        required = new[] { "region" }
    };

    public async Task<object> ExecuteAsync(JsonElement arguments)
    {
        var region = arguments.GetProperty("region").GetString();
        var limit = arguments.TryGetProperty("limit", out var l) ? l.GetInt32() : 10;

        var customers = await _repository.GetByRegionAsync(region, limit);
        return new { customers, count = customers.Count };
    }
}
```

### 3. Middleware de Autenticación JWT (Ejercicio 3)

```csharp
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (authHeader?.StartsWith("Bearer ") == true)
    {
        var token = authHeader["Bearer ".Length..];
        var principal = ValidateToken(token);
        context.User = principal;
    }
    await next();
});

ClaimsPrincipal ValidateToken(string token)
{
    var handler = new JwtSecurityTokenHandler();
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
        ValidateIssuer = true,
        ValidIssuer = "mcp-workshop",
        ValidateAudience = true,
        ValidAudience = "mcp-servers"
    };

    return handler.ValidateToken(token, validationParams, out _);
}
```

### 4. Limitación de Velocidad (Ejercicio 3)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ByRole", context =>
    {
        var role = context.User?.FindFirst("role")?.Value ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(role, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = role switch
            {
                "admin" => int.MaxValue,
                "editor" => 50,
                "viewer" => 10,
                _ => 5
            },
            Window = TimeSpan.FromMinutes(1)
        });
    });
});

app.UseRateLimiter();
```

### 5. Orquestación Multi-Servidor (Ejercicio 4)

```csharp
public class VirtualAnalyst
{
    private readonly SqlMcpClient _sqlClient;
    private readonly CosmosMcpClient _cosmosClient;
    private readonly RestMcpClient _restClient;
    private readonly IMemoryCache _cache;

    public async Task<string> AnswerAsync(string naturalLanguageQuery)
    {
        // 1. Parsear la intención
        var intent = ParseQuery(naturalLanguageQuery);

        // 2. Determinar qué servidores llamar
        var tasks = new List<Task<object>>();
        if (intent.NeedsSqlData) tasks.Add(_sqlClient.QueryAsync(intent.SqlQuery));
        if (intent.NeedsCosmosData) tasks.Add(_cosmosClient.GetSessionAsync(intent.SessionId));
        if (intent.NeedsExternalApi) tasks.Add(_restClient.CallAsync(intent.ApiEndpoint));

        // 3. Ejecutar en paralelo
        var results = await Task.WhenAll(tasks);

        // 4. Agregar y formatear
        return FormatResponse(results, intent);
    }
}
```

---

## ⚡ Comandos PowerShell

### Configuración y Verificación

```powershell
# Verificar todos los requisitos previos
.\scripts\check-prerequisites.ps1 -Verbose

# Verificar ejercicio específico
.\scripts\verify-exercise1.ps1
.\scripts\verify-exercise2.ps1
.\scripts\verify-exercise3.ps1 -Token "your-jwt-token"
.\scripts\verify-exercise4.ps1

# Ejecutar todas las pruebas
.\scripts\run-all-tests.ps1 -Coverage $true

# Iniciar todos los servidores del Ejercicio 4
.\scripts\start-exercise4-servers.ps1
```

### Flujo de Desarrollo

```powershell
# Crear nuevo proyecto de servidor MCP
dotnet new web -n MyMcpServer
cd MyMcpServer
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.0

# Compilar y ejecutar
dotnet build
dotnet run --urls "http://localhost:5000"

# Ejecutar con perfil específico
dotnet run --launch-profile Development

# Modo watch (recompilación automática)
dotnet watch run
```

### Prueba de Endpoints

```powershell
# Probar initialize
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{ name = "test-client"; version = "1.0.0" }
    }
    id = 1
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Probar resources/list
$body = @{ jsonrpc="2.0"; method="resources/list"; id=2 } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Probar tools/call
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "get_customers"
        arguments = @{ region = "Europe"; limit = 5 }
    }
    id = 3
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Probar con autenticación
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri http://localhost:5001 -Method Post -Body $body -ContentType "application/json" -Headers $headers
```

---

## 🐛 Soluciones Rápidas de Problemas

### Conflictos de Puerto

```powershell
# Cambiar puerto
$env:ASPNETCORE_URLS="http://localhost:5010"
dotnet run

# Buscar y cerrar proceso
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Problemas con NuGet

```powershell
# Limpiar caché de NuGet
dotnet nuget locals all --clear

# Restaurar con logging detallado
dotnet restore --verbosity detailed

# Usar fuente específica
dotnet add package ModelContextProtocol --source https://api.nuget.org/v3/index.json --prerelease
```

### Depuración de JWT

```csharp
// Registrar claims del token
var handler = new JwtSecurityTokenHandler();
var token = handler.ReadJwtToken(tokenString);
Console.WriteLine(string.Join("\n", token.Claims.Select(c => $"{c.Type}: {c.Value}")));

// Decodificar token en línea: https://jwt.io
```

### Serialización JSON

```csharp
// Deserialización sin distinguir mayúsculas/minúsculas
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

### Errores CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

---

## 📚 Enlaces Útiles

| Recurso                     | URL                                      |
| --------------------------- | ---------------------------------------- |
| Especificación MCP          | https://modelcontextprotocol.io/    |
| Documentación .NET 10       | https://learn.microsoft.com/dotnet/core/ |
| Especificación JSON-RPC 2.0 | https://www.jsonrpc.org/specification    |
| Depurador JWT               | https://jwt.io                           |
| Validador JSON Schema       | https://www.jsonschemavalidator.net/     |


---

## 🎯 Resumen de Criterios de Éxito

| Ejercicio | Criterio                      | Comando de Validación                          |
| --------- | ----------------------------- | ---------------------------------------------- |
| 1         | 4 recursos expuestos          | `.\scripts\verify-exercise1.ps1`               |
| 2         | 3 herramientas con esquemas   | `.\scripts\verify-exercise2.ps1`               |
| 3         | Auth JWT + limitación de tasa | `.\scripts\verify-exercise3.ps1 -Token $token` |
| 4         | Orquestación de 3+ servidores | `.\scripts\verify-exercise4.ps1`               |

---

## 💡 Consejos Profesionales

1. **Mantén esta página abierta** en un segundo monitor/pestaña durante los ejercicios
2. **Copia-pega con cuidado**: Entiende cada línea antes de ejecutar
3. **Lee los mensajes de error completos**: A menudo contienen la solución
4. **Prueba incrementalmente**: Valida después de cada implementación de método
5. **Usa el depurador**: Establece breakpoints en VS Code (F5) o VS (F5)
6. **Revisa los ejemplos**: Implementaciones de referencia en `src/McpWorkshop.Servers/`
7. **Pide ayuda**: Levanta la mano si estás bloqueado > 3 minutos

---

# **¡Feliz Programación!** 💻
