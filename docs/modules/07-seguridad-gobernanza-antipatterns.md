# Security Anti-Patterns for MCP Servers

**Purpose**: Common security mistakes to avoid when building production MCP servers  
**Audience**: Developers, DevOps, Security Engineers  
**Level**: Intermediate to Advanced

---

## 🚫 Authentication Anti-Patterns

### ❌ Anti-Pattern 1: Hardcoded Secrets

```csharp
// ❌ MAL: Secret en código fuente
public class JwtAuthMiddleware
{
    private const string SECRET_KEY = "my-super-secret-key-123";
    private const string ISSUER = "https://my-issuer.com";

    public string GenerateToken(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY));
        // ...
    }
}
```

**Problemas**:

-   Secret expuesto en Git history
-   Difícil rotación (requiere redeploy)
-   Mismo secret en dev/prod
-   Violación de compliance (GDPR, SOC2)

**✅ Solución**:

```csharp
// ✅ BIEN: Usar Azure Key Vault
public class JwtAuthMiddleware
{
    private readonly SecretClient _secretClient;

    public JwtAuthMiddleware(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public async Task<string> GenerateToken(string userId)
    {
        var secret = await _secretClient.GetSecretAsync("jwt-secret-key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret.Value.Value));
        // ...
    }
}

// Program.cs con Managed Identity
var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]);
builder.Services.AddSingleton(_ => new SecretClient(keyVaultUrl, new DefaultAzureCredential()));
```

---

### ❌ Anti-Pattern 2: Tokens Sin Expiración

```csharp
// ❌ MAL: Token eterno
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddYears(10),  // ❌ 10 años!
    SigningCredentials = credentials
};
```

**Problemas**:

-   Token robado es válido indefinidamente
-   No hay revocación efectiva
-   Mayor superficie de ataque

**✅ Solución**:

```csharp
// ✅ BIEN: Token corto + Refresh Token
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddHours(1),  // ✅ 1 hora
    SigningCredentials = credentials
};

// Refresh token almacenado en base de datos con TTL 7 días
var refreshToken = new RefreshToken
{
    Token = GenerateRefreshToken(),
    UserId = userId,
    ExpiresAt = DateTime.UtcNow.AddDays(7),
    IsRevoked = false
};
await _dbContext.RefreshTokens.AddAsync(refreshToken);
```

---

### ❌ Anti-Pattern 3: Validación Deshabilitada

```csharp
// ❌ MAL: Sin validación
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = false,  // ❌ Acepta cualquier firma
    ValidateIssuer = false,            // ❌ Acepta cualquier emisor
    ValidateAudience = false,          // ❌ Acepta cualquier audiencia
    ValidateLifetime = false,          // ❌ Acepta tokens expirados
    ClockSkew = TimeSpan.FromHours(1)  // ❌ 1 hora de tolerancia!
};
```

**Problemas**:

-   Acepta tokens falsificados
-   No verifica origen del token
-   Tokens expirados siguen válidos

**✅ Solución**:

```csharp
// ✅ BIEN: Validación estricta
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidIssuer = configuration["Jwt:Issuer"],
    ValidAudience = configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ClockSkew = TimeSpan.Zero  // ✅ Sin tolerancia
};
```

---

## 🚫 Authorization Anti-Patterns

### ❌ Anti-Pattern 4: Autorización en Cliente

```csharp
// ❌ MAL: Confiar en el cliente
[HttpPost("delete-all-data")]
public IActionResult DeleteAllData([FromHeader] bool isAdmin)
{
    if (isAdmin)  // ❌ Cliente puede enviar isAdmin=true
    {
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
        return Ok();
    }
    return Forbid();
}
```

**Problemas**:

-   Cliente puede manipular headers
-   No hay verificación del token
-   Escalación de privilegios trivial

**✅ Solución**:

```csharp
// ✅ BIEN: Autorización en servidor
[HttpPost("delete-all-data")]
[Authorize(Policy = "AdminOnly")]  // ✅ Validar claim en token
public IActionResult DeleteAllData()
{
    // Token JWT contiene claim "role": "admin"
    // Policy verifica: context.User.HasClaim("role", "admin")
    _dbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
    return Ok();
}

// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "admin"));
});
```

---

### ❌ Anti-Pattern 5: Scopes Demasiado Amplios

```csharp
// ❌ MAL: Un solo scope para todo
var claims = new[]
{
    new Claim("sub", userId),
    new Claim("scope", "mcp:all")  // ❌ Acceso total
};

// Todos los endpoints aceptan el mismo scope
[Authorize]
public IActionResult ReadData() { }

[Authorize]
public IActionResult WriteData() { }

[Authorize]
public IActionResult DeleteAllData() { }
```

**Problemas**:

-   Violación de principio de mínimo privilegio
-   Token robado tiene acceso completo
-   No hay granularidad de permisos

**✅ Solución**:

```csharp
// ✅ BIEN: Scopes jerárquicos granulares
var claims = new[]
{
    new Claim("sub", userId),
    new Claim("scope", "mcp:resources:read")  // ✅ Solo lectura
};

// Endpoints con scopes específicos
[Authorize(Policy = "RequireRead")]
public IActionResult ReadData() { }

[Authorize(Policy = "RequireWrite")]  // Implica read también
public IActionResult WriteData() { }

[Authorize(Policy = "RequireAdmin")]  // Implica write y read
public IActionResult DeleteAllData() { }

// Program.cs con jerarquía
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireRead", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" && (
                c.Value.Contains("mcp:resources:read") ||
                c.Value.Contains("mcp:tools:execute") ||
                c.Value.Contains("mcp:admin")))));

    options.AddPolicy("RequireWrite", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" && (
                c.Value.Contains("mcp:tools:execute") ||
                c.Value.Contains("mcp:admin")))));
});
```

---

## 🚫 Rate Limiting Anti-Patterns

### ❌ Anti-Pattern 6: Rate Limiting por IP

```csharp
// ❌ MAL: Solo por IP (fácil de evadir)
public class RateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var key = $"rate-limit:{clientIp}";

        // Atacante usa proxy/VPN para cambiar IP
        // NAT causa que usuarios legítimos compartan IP
    }
}
```

**Problemas**:

-   Proxy/VPN evade límite
-   NAT causa false positives (office networks)
-   No discrimina usuarios autenticados

**✅ Solución**:

```csharp
// ✅ BIEN: Rate limiting por userId + scope
public class RateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Primero extraer userId del token JWT
        var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
        var scope = context.User.FindFirst("scope")?.Value ?? "none";

        // Key compuesta: userId + scope + endpoint
        var endpoint = context.Request.Path;
        var key = $"rate-limit:{userId}:{scope}:{endpoint}";

        // Límites diferenciados por scope
        var limit = scope switch
        {
            "mcp:admin" => 10,
            "mcp:tools:execute" => 50,
            "mcp:resources:read" => 100,
            _ => 5  // Anónimos muy limitados
        };
    }
}
```

---

### ❌ Anti-Pattern 7: Fixed Window (ventana fija)

```csharp
// ❌ MAL: Fixed window permite burst doble
public bool IsAllowed(string key)
{
    var minute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
    var redisKey = $"{key}:{minute}";
    var count = _redis.StringIncrement(redisKey);
    _redis.KeyExpire(redisKey, TimeSpan.FromMinutes(1));

    return count <= 100;
}

// Problema: Usuario hace 100 requests a las 10:59:59
//           y otros 100 requests a las 11:00:01
//           = 200 requests en 2 segundos!
```

**✅ Solución**:

```csharp
// ✅ BIEN: Sliding window
public bool IsAllowed(string key, int limit, TimeSpan window)
{
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var windowStart = now - (long)window.TotalSeconds;

    // Remover requests antiguos
    _redis.SortedSetRemoveRangeByScore(key, 0, windowStart);

    // Contar requests en ventana
    var count = _redis.SortedSetLength(key);

    if (count < limit)
    {
        // Agregar request actual
        _redis.SortedSetAdd(key, now, now);
        _redis.KeyExpire(key, window);
        return true;
    }

    return false;
}
```

---

## 🚫 Logging Anti-Patterns

### ❌ Anti-Pattern 8: Logging de Datos Sensibles

```csharp
// ❌ MAL: Loguear todo sin filtros
_logger.LogInformation("User login: {Request}", JsonSerializer.Serialize(request));

// Request contiene:
// {
//   "email": "user@example.com",
//   "password": "MyPassword123!",    ❌ Password en logs!
//   "creditCard": "4532-1234-5678-9012"  ❌ PCI violation!
// }
```

**Problemas**:

-   Violación GDPR/PCI-DSS
-   Passwords/tokens expuestos en logs
-   Log aggregators tienen datos sensibles

**✅ Solución**:

```csharp
// ✅ BIEN: Redactar campos sensibles
public class SensitiveDataRedactor
{
    private static readonly string[] SensitiveFields =
        { "password", "token", "secret", "apiKey", "creditCard", "ssn" };

    public static string Redact(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var document = JsonDocument.Parse(json);
        var root = document.RootElement.Clone();

        return RedactElement(root).ToString();
    }

    private static JsonElement RedactElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var redacted = new Dictionary<string, object>();
            foreach (var property in element.EnumerateObject())
            {
                var key = property.Name.ToLowerInvariant();
                if (SensitiveFields.Any(f => key.Contains(f)))
                {
                    redacted[property.Name] = "[REDACTED]";
                }
                else
                {
                    redacted[property.Name] = property.Value;
                }
            }
            return JsonSerializer.SerializeToElement(redacted);
        }
        return element;
    }
}

// Uso
_logger.LogInformation("User login: {Request}",
    SensitiveDataRedactor.Redact(request));
```

---

### ❌ Anti-Pattern 9: Log Injection

```csharp
// ❌ MAL: User input directo en logs
_logger.LogInformation($"User {username} logged in");

// Atacante envía username = "admin\nFAKE LOG: User superadmin logged in"
// Resultado:
// 2025-11-18 10:00:00 INFO User admin
// FAKE LOG: User superadmin logged in
```

**Problemas**:

-   Logs falsificados (forged)
-   Ocultación de actividad maliciosa
-   SIEM/alertas confundidos

**✅ Solución**:

```csharp
// ✅ BIEN: Usar structured logging con parámetros
_logger.LogInformation("User {Username} logged in",
    username.Replace("\n", "").Replace("\r", ""));

// O mejor aún: Usar structured logging JSON
_logger.LogInformation("User login event: {@Event}", new
{
    Username = SanitizeUsername(username),
    Timestamp = DateTime.UtcNow,
    IpAddress = GetClientIp()
});

private string SanitizeUsername(string username)
{
    return Regex.Replace(username, @"[\r\n]", "");
}
```

---

## 🚫 Data Exposure Anti-Patterns

### ❌ Anti-Pattern 10: Stack Traces en Producción

```csharp
// ❌ MAL: Error details expuestos
app.UseDeveloperExceptionPage();  // ❌ En producción!

// Cliente ve:
// {
//   "error": "System.Data.SqlClient.SqlException",
//   "message": "Cannot connect to database server 10.0.1.5",
//   "stackTrace": "at MyApp.Database.Connect()..."
// }
```

**Problemas**:

-   Información de infraestructura expuesta
-   IP interna del servidor SQL revelada
-   Estructura de código visible
-   Ayuda a atacantes

**✅ Solución**:

```csharp
// ✅ BIEN: Error handling genérico
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/error");

    app.Map("/error", errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "internal_server_error",
                message = "An unexpected error occurred. Please contact support.",
                requestId = context.TraceIdentifier  // Para correlación interna
            }));
        });
    });
}
else
{
    app.UseDeveloperExceptionPage();  // ✅ Solo en desarrollo
}
```

---

## 📊 Security Checklist por Fase

### Development

-   [ ] Usar secretos de desarrollo (no producción)
-   [ ] Developer exception page habilitado
-   [ ] Logging en nivel Debug
-   [ ] CORS permisivo para localhost
-   [ ] HTTPS opcional

### Staging

-   [ ] Usar secretos de staging (rotar regularmente)
-   [ ] Error handling genérico
-   [ ] Logging en nivel Information
-   [ ] CORS configurado con dominios staging
-   [ ] HTTPS obligatorio
-   [ ] Security headers habilitados

### Production

-   [ ] Secretos en Azure Key Vault con Managed Identity
-   [ ] Error handling genérico sin detalles
-   [ ] Logging en nivel Warning/Error
-   [ ] CORS con whitelist estricta
-   [ ] HTTPS obligatorio con HSTS
-   [ ] Security headers completos (CSP, X-Frame-Options, etc.)
-   [ ] Rate limiting agresivo
-   [ ] WAF habilitado (Azure Application Gateway)
-   [ ] DDoS protection habilitado
-   [ ] Monitoring y alertas configurados

---

## 🎯 Key Takeaways

1. **Nunca hardcodear secretos** → Usar Azure Key Vault
2. **Tokens cortos** → 1 hora max + refresh tokens
3. **Validación estricta** → Verificar issuer, audience, lifetime
4. **Scopes granulares** → Principio de mínimo privilegio
5. **Rate limiting por usuario** → No solo por IP
6. **Sliding window** → Evitar burst attacks
7. **Redactar datos sensibles** → GDPR/PCI compliance
8. **Sanitizar inputs** → Prevenir log injection
9. **Errores genéricos en producción** → No exponer internals
10. **Monitorear todo** → Application Insights + alertas

---

**Siguiente paso**: Aplicar estos patrones en Exercise 3 (Secure Server) y revisar código contra este checklist.
