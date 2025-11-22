# Bloque 5: Ejercicio 3 - Servidor MCP con Seguridad (20 minutos)

**Tipo**: Ejercicio de implementación enfocada  
**Duración**: 20 minutos  
**Nivel**: Intermedio-Avanzado  
**Objetivo**: Implementar autenticación JWT, autorización basada en scopes, rate limiting y logging estructurado

---

## 🎯 Objetivos del Ejercicio

Al completar este ejercicio, habrás:

1. ✅ Implementado autenticación con tokens JWT
2. ✅ Configurado autorización basada en scopes (read, write, admin)
3. ✅ Aplicado rate limiting por usuario (10 req/min base, 50 req/min premium)
4. ✅ Integrado logging estructurado de eventos de seguridad
5. ✅ Probado escenarios de acceso autorizado y denegado

---

## 📋 Prerrequisitos

Antes de comenzar, verifica que:

-   [x] Completaste los Ejercicios 1 y 2 exitosamente
-   [x] Tienes `Exercise2Server` funcionando
-   [x] Conoces los conceptos de JWT (JSON Web Tokens)
-   [x] Entiendes el modelo de autorización basada en roles/scopes

---

## 🔐 Conceptos de Seguridad

### 1. Autenticación vs Autorización

| Concepto          | Definición                         | Pregunta que responde |
| ----------------- | ---------------------------------- | --------------------- |
| **Autenticación** | Verificar la identidad del usuario | "¿Quién eres?"        |
| **Autorización**  | Verificar permisos del usuario     | "¿Qué puedes hacer?"  |

**Ejemplo**:

-   **Autenticación**: El usuario presenta un token JWT válido → "Eres Ana García"
-   **Autorización**: El token contiene scope `read` → "Puedes leer, pero no modificar"

### 2. Scopes (Alcances)

Los scopes definen el nivel de acceso:

| Scope   | Permisos                                | Ejemplo de uso              |
| ------- | --------------------------------------- | --------------------------- |
| `read`  | Solo lectura de recursos y herramientas | Consultores externos        |
| `write` | Lectura + modificación de datos         | Empleados internos          |
| `admin` | Lectura + modificación + configuración  | Administradores del sistema |

### 3. JWT (JSON Web Token)

Un JWT tiene 3 partes separadas por `.`:

```
Header.Payload.Signature
```

**Estructura del Payload** (para este ejercicio):

```json
{
    "sub": "user-123", // ID del usuario
    "name": "Ana García", // Nombre del usuario
    "scopes": ["read", "write"], // Permisos
    "tier": "premium", // Nivel de servicio
    "exp": 1735689600 // Expiración (timestamp)
}
```

### 4. Rate Limiting

Limita las solicitudes por usuario para prevenir abuso:

| Tier        | Límite                |
| ----------- | --------------------- |
| **Base**    | 10 solicitudes/minuto |
| **Premium** | 50 solicitudes/minuto |

---

## 📂 Estructura del Servidor a Crear

```
src/McpWorkshop.Servers/
└── Exercise3Server/
    ├── Program.cs                     # Servidor principal con seguridad
    ├── Exercise3Server.csproj         # Archivo de proyecto
    ├── Security/
    │   ├── JwtAuthenticationService.cs    # Validación de JWT
    │   ├── ScopeAuthorizationService.cs   # Autorización por scopes
    │   └── RateLimitingService.cs         # Control de tasa de solicitudes
    ├── Middleware/
    │   ├── AuthenticationMiddleware.cs    # Middleware de autenticación
    │   └── RateLimitingMiddleware.cs      # Middleware de rate limiting
    └── Models/
        ├── AuthenticatedUser.cs           # Usuario autenticado
        └── RateLimitInfo.cs               # Información de rate limit
```

---

## 🚀 Paso a Paso

### Paso 1: Crear el Proyecto (2 minutos)

#### 1.1 Crear estructura

```powershell
cd src/McpWorkshop.Servers
dotnet new web -n Exercise3Server -f net10.0
cd Exercise3Server

# Agregar referencias
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj

# Agregar paquete JWT
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.15.0

# Agregar a solución
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise3Server/Exercise3Server.csproj
```

#### 1.2 Crear carpetas

```powershell
cd src/McpWorkshop.Servers/Exercise3Server
mkdir Security
mkdir Middleware
mkdir Models
```

**✅ Checkpoint**: Proyecto creado con estructura.

---

### Paso 2: Implementar Modelos (2 minutos)

#### 2.1 Modelo AuthenticatedUser

Crea `Models/AuthenticatedUser.cs`:

```csharp
namespace Exercise3Server.Models;

public class AuthenticatedUser
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public string Tier { get; set; } = "base"; // "base" o "premium"
}
```

#### 2.2 Modelo RateLimitInfo

Crea `Models/RateLimitInfo.cs`:

```csharp
namespace Exercise3Server.Models;

public class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public int Limit { get; set; }
}
```

**✅ Checkpoint**: Dos modelos creados.

---

### Paso 3: Implementar Servicios de Seguridad (8 minutos)

#### 3.1 JwtAuthenticationService

Crea `Security/JwtAuthenticationService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Exercise3Server.Models;
using Microsoft.IdentityModel.Tokens;

namespace Exercise3Server.Security;

public class JwtAuthenticationService
{
    private const string SecretKey = "MCP-Workshop-2025-Super-Secret-Key-DO-NOT-USE-IN-PRODUCTION";
    private readonly SymmetricSecurityKey _signingKey;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtAuthenticationService()
    {
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public AuthenticatedUser? ValidateToken(string token)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;
            var scopesClaim = principal.FindFirst("scopes")?.Value;
            var tier = principal.FindFirst("tier")?.Value ?? "base";

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var scopes = scopesClaim?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            return new AuthenticatedUser
            {
                UserId = userId,
                Name = name ?? "Unknown",
                Scopes = scopes,
                Tier = tier
            };
        }
        catch
        {
            return null;
        }
    }

    public string GenerateToken(string userId, string name, List<string> scopes, string tier, int expirationMinutes = 60)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim("scopes", string.Join(",", scopes)),
            new Claim("tier", tier)
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }
}
```

#### 3.2 ScopeAuthorizationService

Crea `Security/ScopeAuthorizationService.cs`:

```csharp
using Exercise3Server.Models;

namespace Exercise3Server.Security;

public class ScopeAuthorizationService
{
    public bool HasScope(AuthenticatedUser user, string requiredScope)
    {
        return user.Scopes.Contains(requiredScope, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyScope(AuthenticatedUser user, params string[] requiredScopes)
    {
        return requiredScopes.Any(scope => HasScope(user, scope));
    }

    public string GetRequiredScopeForMethod(string method)
    {
        return method switch
        {
            "initialize" => "",              // Público
            "resources/list" => "",          // Público
            "resources/read" => "read",      // Requiere read
            "tools/list" => "",              // Público
            "tools/call" => "write",         // Requiere write
            _ => "admin"                     // Otros requieren admin
        };
    }

    public bool IsAuthorized(AuthenticatedUser user, string method)
    {
        var requiredScope = GetRequiredScopeForMethod(method);

        if (string.IsNullOrEmpty(requiredScope))
        {
            return true; // Método público
        }

        return HasScope(user, requiredScope);
    }
}
```

#### 3.3 RateLimitingService

Crea `Security/RateLimitingService.cs`:

```csharp
using System.Collections.Concurrent;
using Exercise3Server.Models;

namespace Exercise3Server.Security;

public class RateLimitingService
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _userLimits = new();

    public bool IsAllowed(AuthenticatedUser user)
    {
        var limit = user.Tier == "premium" ? 50 : 10;
        var userId = user.UserId;
        var now = DateTime.UtcNow;

        var limitInfo = _userLimits.GetOrAdd(userId, _ => new RateLimitInfo
        {
            RequestCount = 0,
            WindowStart = now,
            Limit = limit
        });

        // Resetear ventana si pasó 1 minuto
        if ((now - limitInfo.WindowStart).TotalMinutes >= 1)
        {
            limitInfo.RequestCount = 0;
            limitInfo.WindowStart = now;
        }

        // Verificar límite
        if (limitInfo.RequestCount >= limit)
        {
            return false;
        }

        limitInfo.RequestCount++;
        return true;
    }

    public RateLimitInfo GetLimitInfo(string userId)
    {
        return _userLimits.GetValueOrDefault(userId) ?? new RateLimitInfo { Limit = 10 };
    }
}
```

**✅ Checkpoint**: Tres servicios de seguridad creados.

---

### Paso 4: Implementar Middlewares (4 minutos)

#### 4.1 AuthenticationMiddleware

Crea `Middleware/AuthenticationMiddleware.cs`:

```csharp
using Exercise3Server.Security;

namespace Exercise3Server.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtAuthenticationService _authService;

    public AuthenticationMiddleware(RequestDelegate next, JwtAuthenticationService authService)
    {
        _next = next;
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var user = _authService.ValidateToken(token);

            if (user != null)
            {
                context.Items["User"] = user;
            }
        }

        await _next(context);
    }
}
```

#### 4.2 RateLimitingMiddleware

Crea `Middleware/RateLimitingMiddleware.cs`:

```csharp
using Exercise3Server.Security;
using Exercise3Server.Models;

namespace Exercise3Server.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingService _rateLimitService;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitingService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.Items["User"] as AuthenticatedUser;

        if (user == null)
        {
            // Sin autenticación, pasar al siguiente middleware
            await _next(context);
            return;
        }

        if (!_rateLimitService.IsAllowed(user))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsJsonAsync(new
            {
                jsonrpc = "2.0",
                error = new
                {
                    code = -32003,
                    message = "Rate limit exceeded",
                    data = new
                    {
                        userId = user.UserId,
                        limit = _rateLimitService.GetLimitInfo(user.UserId).Limit,
                        tier = user.Tier
                    }
                },
                id = (object?)null
            });
            return;
        }

        await _next(context);
    }
}
```

**✅ Checkpoint**: Dos middlewares creados.

---

### Paso 5: Implementar Program.cs (4 minutos)

Reemplaza todo el contenido de `Program.cs`:

```csharp
using System.Text.Json;
using Exercise3Server.Models;
using Exercise3Server.Middleware;
using Exercise3Server.Security;
using McpWorkshop.Shared.Logging;
using McpWorkshop.Shared.Mcp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddSingleton<IStructuredLogger, StructuredLogger>();
builder.Services.AddSingleton<JwtAuthenticationService>();
builder.Services.AddSingleton<ScopeAuthorizationService>();
builder.Services.AddSingleton<RateLimitingService>();

builder.Services.Configure<McpWorkshop.Shared.Configuration.WorkshopSettings>(options =>
{
    options.Server.Name = "Exercise3Server";
    options.Server.Version = "1.0.0";
    options.Server.ProtocolVersion = "2024-11-05";
    options.Server.Port = 5003;
});

var app = builder.Build();

// Middlewares de seguridad
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

// Health check endpoint
app.MapGet("/", (IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings) => Results.Ok(new
{
    status = "healthy",
    server = settings.Value.Server.Name,
    version = settings.Value.Server.Version,
    timestamp = DateTime.UtcNow
}));

// Endpoint para generar tokens (solo para testing)
app.MapPost("/auth/token", (
    [FromBody] TokenRequest request,
    JwtAuthenticationService authService) =>
{
    var token = authService.GenerateToken(
        request.UserId,
        request.Name,
        request.Scopes,
        request.Tier,
        60
    );

    return Results.Ok(new { token });
});

// Endpoint MCP
app.MapPost("/mcp", async (
    HttpContext httpContext,
    JsonRpcRequest request,
    IStructuredLogger logger,
    ScopeAuthorizationService authz,
    IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings) =>
{
    var requestId = request.Id?.ToString() ?? "unknown";
    var user = httpContext.Items["User"] as AuthenticatedUser;

    logger.LogRequest(request.Method, requestId, new Dictionary<string, object>
    {
        ["method"] = request.Method,
        ["userId"] = user?.UserId ?? "anonymous",
        ["scopes"] = user?.Scopes ?? new List<string>()
    });

    try
    {
        // Verificar autorización
        if (user != null && !authz.IsAuthorized(user, request.Method))
        {
            logger.LogError(request.Method, requestId, new Exception("Unauthorized"));
            return Results.Ok(CreateErrorResponse(-32004, "Insufficient permissions", new
            {
                requiredScope = authz.GetRequiredScopeForMethod(request.Method),
                userScopes = user.Scopes
            }, request.Id));
        }

        var response = request.Method switch
        {
            "initialize" => HandleInitialize(request.Id, settings),
            "resources/list" => HandleResourcesList(request.Id),
            "resources/read" => HandleResourcesRead(request.Id, user, authz),
            "tools/list" => HandleToolsList(request.Id),
            "tools/call" => HandleToolsCall(request.Id, user, authz),
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

app.Run("http://localhost:5003");

// Handlers
static JsonRpcResponse HandleInitialize(object? requestId, IOptions<McpWorkshop.Shared.Configuration.WorkshopSettings> settings)
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
        Id = requestId
    };
}

static JsonRpcResponse HandleResourcesList(object? requestId)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            resources = new[]
            {
                new { uri = "mcp://secure-data", name = "Secure Data", description = "Datos protegidos (requiere scope 'read')", mimeType = "application/json" }
            }
        },
        Id = requestId
    };
}

static JsonRpcResponse HandleResourcesRead(object? requestId, AuthenticatedUser? user, ScopeAuthorizationService authz)
{
    if (user == null || !authz.HasScope(user, "read"))
    {
        throw new UnauthorizedAccessException("Scope 'read' required");
    }

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            contents = new[]
            {
                new
                {
                    uri = "mcp://secure-data",
                    mimeType = "application/json",
                    text = JsonSerializer.Serialize(new { message = "Datos sensibles", user = user.Name }, new JsonSerializerOptions { WriteIndented = true })
                }
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
                new
                {
                    name = "secure_action",
                    description = "Acción protegida (requiere scope 'write')",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new { action = new { type = "string" } },
                        required = new[] { "action" }
                    }
                }
            }
        },
        Id = requestId
    };
}

static JsonRpcResponse HandleToolsCall(object? requestId, AuthenticatedUser? user, ScopeAuthorizationService authz)
{
    if (user == null || !authz.HasScope(user, "write"))
    {
        throw new UnauthorizedAccessException("Scope 'write' required");
    }

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"Acción ejecutada por {user.Name} (scopes: {string.Join(", ", user.Scopes)})"
                }
            }
        },
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

record TokenRequest(string UserId, string Name, List<string> Scopes, string Tier);
```

**✅ Checkpoint**: Compilación sin errores (`dotnet build`).

---

## 🔬 Paso 6: Probar el Servidor (5 minutos)

### 6.1 Ejecutar servidor

```powershell
cd src/McpWorkshop.Servers/Exercise3Server
dotnet run
```

Deberías ver:

```text
info: Now listening on: http://localhost:5003
```

### 6.2 Verificar Health Check

```powershell
Invoke-WebRequest -Uri "http://localhost:5003" -Method GET
```

**Respuesta esperada**: Status 200 con JSON `{"status": "healthy", "server": "Exercise3Server", ...}`

### 6.2 Prueba 1: Generar token (usuario con scope `read`)

Terminal 2:

```powershell
$body = @{
    userId = "user-001"
    name = "Ana García"
    scopes = @("read")
    tier = "base"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/auth/token" -Method POST -Body $body -ContentType "application/json"
$tokenRead = $response.token
Write-Host "Token (read): $tokenRead"
```

**Resultado esperado**: Token JWT devuelto.

✅ **PASS**

### 6.3 Prueba 2: Generar token (usuario con scopes `read` y `write`)

```powershell
$body = @{
    userId = "user-002"
    name = "Carlos Pérez"
    scopes = @("read", "write")
    tier = "premium"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/auth/token" -Method POST -Body $body -ContentType "application/json"
$tokenWrite = $response.token
Write-Host "Token (read+write): $tokenWrite"
```

✅ **PASS**

### 6.4 Prueba 3: Acceso sin autenticación (debe fallar en resources/read)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://secure-data" }
    id = "read-001"
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -ContentType "application/json"
} catch {
    Write-Host "❌ Error esperado: Unauthorized" -ForegroundColor Yellow
}
```

**Resultado esperado**: Error de autorización.

✅ **PASS**

### 6.5 Prueba 4: Acceso con scope `read` (debe funcionar)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://secure-data" }
    id = "read-002"
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $tokenRead" }
Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json"
```

**Resultado esperado**: Datos sensibles devueltos con nombre del usuario.

✅ **PASS**

### 6.6 Prueba 5: Acceso a `tools/call` con solo `read` (debe fallar)

```powershell
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
try {
    Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json"
} catch {
    Write-Host "❌ Error esperado: Insufficient permissions (requiere 'write')" -ForegroundColor Yellow
}
```

**Resultado esperado**: Error de permisos insuficientes.

✅ **PASS**

### 6.7 Prueba 6: Acceso a `tools/call` con `write` (debe funcionar)

```powershell
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
Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json"
```

**Resultado esperado**: `"Acción ejecutada por Carlos Pérez (scopes: read, write)"`.

✅ **PASS**

### 6.8 Prueba 7: Rate Limiting (usuario base: 10 req/min)

```powershell
$headers = @{ Authorization = "Bearer $tokenRead" }

# Enviar 12 solicitudes rápidamente
1..12 | ForEach-Object {
    $body = @{
        jsonrpc = "2.0"
        method = "initialize"
        params = @{}
        id = "init-$_"
    } | ConvertTo-Json

    $result = Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json" -ErrorAction SilentlyContinue

    if ($result.error) {
        Write-Host "Request $_: ❌ Rate limit exceeded" -ForegroundColor Red
    } else {
        Write-Host "Request $_: ✅ OK" -ForegroundColor Green
    }
}
```

**Resultado esperado**: Primeras 10 solicitudes pasan, las 11 y 12 fallan con error `429 Too Many Requests`.

✅ **PASS**

### 6.9 Prueba 8: Endpoints Públicos (sin autenticación)

**IMPORTANTE**: Los métodos `initialize`, `resources/list` y `tools/list` deben ser **públicos** (no requieren token).

```powershell
# Prueba initialize sin token
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{}
    id = 1
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -ContentType "application/json"
Write-Host "✅ initialize es público: $($response.result.protocolVersion)" -ForegroundColor Green

# Prueba resources/list sin token
$body = @{
    jsonrpc = "2.0"
    method = "resources/list"
    params = @{}
    id = 2
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -ContentType "application/json"
Write-Host "✅ resources/list es público: $($response.result.resources.Count) recursos" -ForegroundColor Green

# Prueba tools/list sin token
$body = @{
    jsonrpc = "2.0"
    method = "tools/list"
    params = @{}
    id = 3
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -ContentType "application/json"
Write-Host "✅ tools/list es público: $($response.result.tools.Count) herramientas" -ForegroundColor Green
```

**Resultado esperado**: Las tres llamadas funcionan sin necesidad de token `Authorization`.

✅ **PASS**

---

## ✅ Criterios de Éxito

Has completado el ejercicio exitosamente si:

-   [ ] El servidor compila sin errores
-   [ ] Puedes generar tokens JWT con diferentes scopes
-   [ ] `resources/read` requiere scope `read`
-   [ ] `tools/call` requiere scope `write`
-   [ ] Los usuarios sin token no pueden acceder a recursos protegidos
-   [ ] Los usuarios con scope insuficiente reciben error de permisos
-   [ ] El rate limiting funciona (10 req/min para `base`, 50 req/min para `premium`)
-   [ ] Los logs estructurados registran eventos de seguridad

---

## 🐛 Solución de Problemas

### Error: "Unauthorized" incluso con token válido

**Causa**: El token no se está enviando correctamente en el header.

**Solución**: Verifica el formato del header:

```powershell
$headers = @{ Authorization = "Bearer $token" }  # Debe incluir "Bearer "
```

### Error: "Insufficient permissions" con scopes correctos

**Causa**: El método `GetRequiredScopeForMethod` puede estar devolviendo el scope incorrecto.

**Solución**: Revisa la lógica en `ScopeAuthorizationService.cs`:

```csharp
"resources/read" => "read",   // resources/read requiere "read"
"tools/call" => "write",      // tools/call requiere "write"
```

### Error: Rate limit no funciona

**Causa**: La ventana de tiempo no se está reseteando correctamente.

**Solución**: Verifica la lógica en `RateLimitingService.cs`:

```csharp
if ((now - limitInfo.WindowStart).TotalMinutes >= 1)
{
    limitInfo.RequestCount = 0;
    limitInfo.WindowStart = now;
}
```

### Error: Token expirado inmediatamente

**Causa**: Reloj del sistema desincronizado.

**Solución**: Aumenta el tiempo de expiración en `GenerateToken`:

```csharp
var token = authService.GenerateToken(userId, name, scopes, tier, 120); // 120 minutos
```

---

## 🚀 Extensiones Opcionales

### Extensión 1: Refresh Tokens

Implementa tokens de refresco para renovar tokens expirados sin volver a autenticar.

### Extensión 2: Audit Logging Completo

Registra todos los eventos de seguridad en una base de datos o archivo:

```csharp
logger.LogSecurityEvent(new
{
    userId = user.UserId,
    action = "resources/read",
    authorized = true,
    timestamp = DateTime.UtcNow
});
```

### Extensión 3: IP-Based Rate Limiting

Combina rate limiting por usuario y por IP para prevenir ataques DDoS.

### Extensión 4: Scope Hierarchy

Implementa jerarquía de scopes donde `admin` incluye automáticamente `write` y `read`.

---

## 📚 Conceptos Aprendidos

### 1. JWT (JSON Web Tokens)

-   Estructura: Header.Payload.Signature
-   Claims estándar: `sub`, `exp`, `iat`
-   Claims personalizados: `scopes`, `tier`

### 2. Autorización Basada en Scopes

-   Separación de permisos (read, write, admin)
-   Validación por método MCP
-   Mensajes de error informativos

### 3. Rate Limiting

-   Ventanas de tiempo (1 minuto)
-   Límites por tier de usuario
-   Reseteo automático de ventanas

### 4. Middlewares en ASP.NET Core

-   Pipeline de procesamiento de solicitudes
-   Orden de ejecución (autenticación → rate limiting → endpoint)
-   Inyección de dependencias en middlewares

---

## 🎓 Próximo Paso

**Bloque 7**: Seguridad y Gobernanza Sesión (15 min)

En el siguiente bloque el instructor profundizará en:

-   Mejores prácticas de seguridad para producción
-   Gestión de secretos y certificados
-   Auditoría y compliance
-   Estrategias de despliegue seguro

---

## 📖 Recursos Adicionales

-   **JWT.io**: https://jwt.io/ (decodificador de tokens)
-   **OWASP Top 10**: https://owasp.org/www-project-top-ten/
-   **Rate Limiting Patterns**: https://learn.microsoft.com/en-us/azure/architecture/patterns/rate-limiting-pattern

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
