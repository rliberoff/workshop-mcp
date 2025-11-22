# MCP Workshop - Guía de Resolución de Problemas

Guía consolidada para resolver problemas comunes durante el workshop. Organizada por categoría y gravedad.

---

## 🚨 Problemas Críticos (Bloquean el progreso)

### 1. .NET SDK 10.0 No Instalado o No Reconocido

**Síntomas**:

```
'dotnet' is not recognized as an internal or external command
```

**Diagnóstico**:

```powershell
dotnet --version
# Esperado: 10.0.x
```

**Soluciones**:

**Opción A - Instalar/Reinstalar**:

```powershell
# Via winget (recomendado)
winget install Microsoft.DotNet.SDK.10

# Via chocolatey
choco install dotnet-sdk

# Manual: https://dotnet.microsoft.com/download/dotnet/10.0
```

**Opción B - Verificar PATH**:

```powershell
$env:PATH -split ';' | Select-String 'dotnet'
# Si no aparece, agregar manualmente:
$env:PATH += ";C:\Program Files\dotnet\"
```

**Opción C - Workaround**: Pair programming con compañero que tenga .NET instalado.

---

### 2. Puertos 5000-5004 ya en uso

**Síntomas**:

```
System.IO.IOException: Failed to bind to address http://localhost:5000: address already in use.
```

**Diagnóstico**:

```powershell
netstat -ano | findstr :5000
# Muestra PID del proceso usando el puerto
```

**Soluciones**:

**Opción A - Cambiar Puerto**:

```powershell
$env:ASPNETCORE_URLS="http://localhost:5010"
dotnet run
```

**Opción B - Matar Proceso**:

```powershell
# Obtener PID del comando anterior, luego:
taskkill /PID <PID> /F
```

**Opción C - Configurar en launchSettings.json**:

```json
{
    "profiles": {
        "http": {
            "commandName": "Project",
            "applicationUrl": "http://localhost:5010",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    }
}
```

---

### 3. Paquete 'ModelContextProtocol' no encontrado

**Síntomas**:

```
error NU1102: Unable to find package 'ModelContextProtocol'
```

**Diagnóstico**:

```powershell
dotnet nuget locals all --list
# Verificar caches de NuGet
```

**Soluciones**:

**Opción A - Flag Prerelease Olvidado**:

```powershell
dotnet add package ModelContextProtocol --prerelease
```

**Opción B - Especificar Source**:

```powershell
dotnet add package ModelContextProtocol --source https://api.nuget.org/v3/index.json --prerelease
```

**Opción C - Limpiar Cache**:

```powershell
dotnet nuget locals all --clear
dotnet restore --force
```

**Opción D - Offline Mode** (si internet está caído):

```powershell
# Usar packages pre-descargados (del instructor)
dotnet restore --source ./offline-packages
```

---

## ⚠️ Problemas Frecuentes (Retrasan Ejercicios)

### 4. Archivo JSON no encontrado (Ejercicio 1)

**Síntomas**:

```
System.IO.FileNotFoundException: Could not find file 'customers.json'
```

**Diagnóstico**:

```powershell
Get-ChildItem -Recurse *.json
# Verificar ubicación de archivos
```

**Soluciones**:

**Opción A - Build Action Incorrecta**:

```xml
<!-- En .csproj, asegurar: -->
<ItemGroup>
  <Content Include="Data\**\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**Opción B - Path Relativo Incorrecto**:

```csharp
// Cambiar de:
var json = await File.ReadAllTextAsync("customers.json");

// A:
var basePath = AppContext.BaseDirectory;
var json = await File.ReadAllTextAsync(Path.Combine(basePath, "Data", "customers.json"));
```

**Opción C - Regenerar Sample Data**:

```powershell
.\scripts\create-sample-data.ps1
# Crea todos los JSON necesarios
```

---

### 5. Token JWT inválido (Ejercicio 3)

**Síntomas**:

```text
HTTP 401 Unauthorized
SecurityTokenException: IDX10214: Audience validation failed
```

**Diagnóstico**:

```powershell
# Decodificar token en jwt.io o con PowerShell:
$parts = $token.Split('.')
[System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($parts[1]))
```

**Soluciones**:

**Opción A - Validar Issuer/Audience**:

```csharp
// En appsettings.json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-here-minimum-32-characters",
    "Issuer": "mcp-workshop",
    "Audience": "mcp-servers"
  }
}

// En Program.cs, asegurar coincidencia exacta:
ValidIssuer = Configuration["Jwt:Issuer"],
ValidAudience = Configuration["Jwt:Audience"]
```

**Opción B - Secret Incorrecto**:

```csharp
// Verificar que el secret tenga al menos 32 caracteres
var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]);
if (key.Length < 32) throw new Exception("JWT secret debe tener mínimo 32 caracteres");
```

**Opción C - Token Expirado**:

```csharp
// Generar token nuevo con expiración larga (para testing)
var token = new JwtSecurityToken(
    issuer: "mcp-workshop",
    audience: "mcp-servers",
    claims: new[] { new Claim("role", "admin") },
    expires: DateTime.UtcNow.AddHours(2), // ← 2 horas
    signingCredentials: credentials
);
```

**Opción D - Usar Tokens Pre-Generados**:

```powershell
# Tokens de demostración (válidos 1 hora desde el workshop)
$adminToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
$viewerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 6. Rate Limiting no funciona (Ejercicio 3)

**Síntomas**:

-   Requests no se rechazan aunque exceden límite
-   No hay headers `X-RateLimit-*` en response

**Diagnóstico**:

```powershell
# Hacer 15 requests rápidas (debería fallar en 11 para viewer)
1..15 | ForEach-Object {
    Invoke-RestMethod -Uri http://localhost:5001 -Method Post -Body $body -Headers $headers
}
```

**Soluciones**:

**Opción A - Orden del Pipeline**:

```csharp
// CORRECTO (rate limiter ANTES de auth):
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// INCORRECTO:
app.UseAuthentication();
app.MapControllers();
app.UseRateLimiter(); // ← Demasiado tarde
```

**Opción B - Policy No Aplicada**:

```csharp
// Aplicar policy a endpoints:
app.MapPost("/", async (HttpContext context) => { })
   .RequireRateLimiting("ByRole"); // ← Agregar esto
```

**Opción C - Partition Key Incorrecto**:

```csharp
// Asegurar que el partition key sea único por usuario:
RateLimitPartition.GetFixedWindowLimiter(
    partitionKey: context.User?.Identity?.Name ?? "anonymous", // ← Debe ser único
    factory: _ => new FixedWindowRateLimiterOptions { }
);
```

---

### 7. Error de CORS en el navegador (Ejercicio 2/4)

**Síntomas**:

```text
Access to fetch at 'http://localhost:5000' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Soluciones**:

**Opción A - Configurar CORS Permisivo (dev only)**:

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

**Opción B - CORS Específico (producción)**:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://yourdomain.com")
              .WithMethods("POST")
              .WithHeaders("Content-Type", "Authorization"));
});
```

**Opción C - Verificar Orden Pipeline**:

```csharp
// CORS debe ir ANTES de Authorization:
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
```

---

### 8. Error de deserialización de JSON

**Síntomas**:

```text
System.Text.Json.JsonException: The JSON value could not be converted to...
```

**Soluciones**:

**Opción A - Case Sensitivity**:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true // ← Esencial
};
var request = JsonSerializer.Deserialize<JsonRpcRequest>(json, options);
```

**Opción B - Nullability**:

```csharp
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

**Opción C - Custom Converter**:

```csharp
// Para propiedades dinámicas como 'params':
public class JsonElementParamsConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonDocument.ParseValue(ref reader).RootElement.Clone();
    }
}
```

---

### 9. Errores de migración de Entity Framework (Ejercicio 4)

**Síntomas**:

```text
No DbContext was found in assembly...
```

**Soluciones**:

**Opción A - Especificar Context**:

```powershell
dotnet ef migrations add Initial --context SalesDbContext
dotnet ef database update --context SalesDbContext
```

**Opción B - Install EF Tools**:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet tool update --global dotnet-ef
```

**Opción C - Connection String Incorrecta**:

```json
// appsettings.Development.json
{
    "ConnectionStrings": {
        "SalesDb": "Server=(localdb)\\mssqllocaldb;Database=McpWorkshop;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
}
```

---

### 10. Virtual Analyst no responde (Ejercicio 4)

**Síntomas**:

-   Timeout después de 30 segundos
-   Response vacío o null

**Diagnóstico**:

```powershell
# Verificar que los 3 servidores MCP estén corriendo
Get-NetTCPConnection -LocalPort 5002,5003,5004 -State Listen
```

**Soluciones**:

**Opción A - Servidores No Iniciados**:

```powershell
# Iniciar manualmente cada servidor:
cd src\McpWorkshop.Servers\Exercise4SqlMcpServer
Start-Process powershell -ArgumentList "dotnet run"

cd ..\Exercise4CosmosMcpServer
Start-Process powershell -ArgumentList "dotnet run"

cd ..\Exercise4RestApiMcpServer
Start-Process powershell -ArgumentList "dotnet run"

# O usar script automatizado:
.\scripts\start-exercise4-servers.ps1
```

**Opción B - URLs Incorrectas en Config**:

```json
// appsettings.json del VirtualAnalyst
{
    "McpServers": {
        "SqlServer": "http://localhost:5002",
        "CosmosServer": "http://localhost:5003",
        "RestApiServer": "http://localhost:5004"
    }
}
```

**Opción C - Timeout Muy Corto**:

```csharp
// En HttpClient configuration:
builder.Services.AddHttpClient("mcp-client", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60); // Aumentar timeout
});
```

**Opción D - Firewall Bloqueando Conexiones**:

```powershell
# Agregar regla de firewall (PowerShell Admin):
New-NetFirewallRule -DisplayName "MCP Workshop" -Direction Inbound -LocalPort 5000-5004 -Protocol TCP -Action Allow
```

---

## 🔍 Problemas de rendimiento

### 11. Respuestas Lentas (>2 segundos)

**Diagnóstico**:

```csharp
// Agregar logging temporal:
var sw = Stopwatch.StartNew();
var result = await ProcessRequestAsync();
sw.Stop();
_logger.LogWarning("Request took {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

**Soluciones**:

**Opción A - Queries N+1**:

```csharp
// MALO:
var customers = await _context.Customers.ToListAsync();
foreach (var c in customers)
{
    c.Orders = await _context.Orders.Where(o => o.CustomerId == c.Id).ToListAsync();
}

// BUENO:
var customers = await _context.Customers
    .Include(c => c.Orders)
    .ToListAsync();
```

**Opción B - Sin Caching**:

```csharp
// Agregar cache en memoria:
builder.Services.AddMemoryCache();

// En servicio:
if (!_cache.TryGetValue("customers", out var customers))
{
    customers = await _repository.GetAllAsync();
    _cache.Set("customers", customers, TimeSpan.FromMinutes(5));
}
```

**Opción C - Serialización Grande**:

```csharp
// Limitar campos retornados:
var customers = await _context.Customers
    .Select(c => new { c.Id, c.Name, c.Region }) // Solo campos necesarios
    .ToListAsync();
```

---

## 🧪 Problemas de pruebas

### 12. Scripts de verificación de ejercicios fallan

**Síntomas**:

```text
❌ Test failed: Expected 200, got 500
```

**Diagnóstico**:

```powershell
# Ejecutar con verbose para ver request/response completos:
.\scripts\verify-exercise1.ps1 -Verbose
```

**Soluciones**:

**Opción A - Server No Está Corriendo**:

```powershell
# Verificar que el servidor esté activo:
Test-NetConnection localhost -Port 5000

# Si no responde, iniciar:
cd src\McpWorkshop.Servers\Exercise1StaticResources
dotnet run
```

**Opción B - Puerto Incorrecto en Script**:

```powershell
# Editar verify script para usar puerto custom:
$baseUrl = "http://localhost:5010" # Cambiar de 5000 a tu puerto
```

**Opción C - Response Format Incorrecto**:

```csharp
// Asegurar formato JSON-RPC válido:
return new
{
    jsonrpc = "2.0",
    result = data,
    id = request.Id // ← Debe coincidir con request
};
```

---

## 💾 Problemas de Datos

### 13. Los datos de muestra no se generan

**Síntomas**:

```powershell
.\scripts\create-sample-data.ps1
# No output, archivos no creados
```

**Soluciones**:

**Opción A - ExecutionPolicy Bloqueada**:

```powershell
# Verificar policy actual:
Get-ExecutionPolicy

# Si es Restricted, cambiar temporalmente:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
```

**Opción B - Crear Datos Manualmente**:

```powershell
# Crear carpeta Data si no existe:
New-Item -Path ".\src\McpWorkshop.Servers\Exercise1StaticResources\Data" -ItemType Directory -Force

# Copiar datos de referencia:
Copy-Item ".\reference-data\*.json" -Destination ".\src\McpWorkshop.Servers\Exercise1StaticResources\Data\"
```

---

## 🌐 Problemas de Red/Conectividad

### 14. No Hay Internet (Workshop Offline)

**Soluciones**:

**Opción A - Usar Packages Locales**:

```powershell
# Restaurar desde cache local:
dotnet restore --source ~/.nuget/packages
```

**Opción B - Compartir Repo Localmente**:

```powershell
# Instructor comparte via carpeta compartida de red:
\\INSTRUCTOR-PC\MCP-Workshop\

# Asistentes clonan localmente:
Copy-Item -Recurse \\INSTRUCTOR-PC\MCP-Workshop C:\Temp\
```

**Opción C - Hotspot Móvil**:

-   Usar datos móviles como backup
-   Solo para NuGet restore (pequeño bandwidth)

---

## 📊 Debugging Avanzado

### Habilitar Logging Detallado

```csharp
// En Program.cs:
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// En appsettings.Development.json:
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "System.Net.Http": "Debug"
    }
  }
}
```

### Capturar Requests/Responses

```csharp
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"REQUEST: {body}");
    context.Request.Body.Position = 0;

    await next();
});
```

---

## ☝🏻 Escalar Problema

Si después de 5 minutos no resuelves el problema:

1. **Levantar mano** para asistencia del instructor
2. **Documentar error**:
    - Screenshot del error
    - Comando que falló
    - Logs relevantes
3. **Plan B temporal**:
    - Pair programming con compañero
    - Seguir con siguiente ejercicio si posible
4. **Post-workshop**: Abrir issue en GitHub con detalles completos

---

## 📚 Recursos de Soporte

| Recurso                                           | Cuándo Usar                   |
| ------------------------------------------------- | ----------------------------- |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)        | Referencia rápida de comandos |
| [MCP Spec](https://spec.modelcontextprotocol.io/) | Dudas del protocolo           |
| [.NET Docs](https://learn.microsoft.com/dotnet/)  | Errores de C# / ASP.NET       |
| Instructor                                        | Problemas bloqueantes         |

---

### **¡No te frustres!** Los errores son oportunidades de aprendizaje. 🚀
