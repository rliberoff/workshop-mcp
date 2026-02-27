# Bloque 5: Ejercicio 3 - Seguridad - Guía del Instructor (20 minutos)

**Propósito**: Ejercicio de seguridad donde los asistentes implementan autenticación, autorización y rate limiting.  
**Formato**: Implementación enfocada con validación continua.  
**Nivel**: Intermedio-Avanzado - requiere comprensión de JWT y middlewares.

---

## ⏱️ Timing Detallado

| Minuto | Actividad                             | Duración  |
| ------ | ------------------------------------- | --------- |
| 0-2    | Explicación de conceptos de seguridad | 2 min     |
| 2-4    | Crear proyecto y estructura           | 2 min     |
| 4-6    | Implementar modelos                   | 2 min     |
| 6-14   | Implementar servicios de seguridad    | 8 min     |
| 14-18  | Implementar middlewares y Program.cs  | 4 min     |
| 18-20  | Probar escenarios de seguridad        | 2 min     |
| **20** | **Finalizar ejercicio**               | **TOTAL** |

---

## 🎯 Objetivo del Instructor

Al terminar este bloque, los asistentes deben:

1. ✅ Comprender autenticación vs autorización
2. ✅ Implementar validación JWT básica
3. ✅ Aplicar scopes a métodos MCP
4. ✅ Configurar rate limiting por tier de usuario
5. ✅ Probar acceso autorizado y denegado

---

## 🧩 Pre-Setup del Instructor

**Antes de comenzar el ejercicio**:

- [ ] Detén `Exercise2Server` (puerto 5002 libre)
- [ ] Ten el paquete NuGet `System.IdentityModel.Tokens.Jwt` listo para instalar
- [ ] Prepara 3 terminales:
  - Terminal 1: Creación de proyecto
  - Terminal 2: Ejecución del servidor (puerto 5003)
  - Terminal 3: Tests con PowerShell
- [ ] Ten los archivos de servicios de seguridad en respaldo
- [ ] Abre el contrato: `specs/001-mcp-workshop-course/contracts/exercise-3-secure-server.json`
- [ ] Valida puerto **5003** libre:

```powershell
netstat -ano | Select-String "5003"
```

---

## 📋 Guion del Ejercicio

### Minutos 0-2: Conceptos de Seguridad (Explicativo)

**Script para decir**:

> "Antes de programar, necesitamos aclarar dos conceptos que SIEMPRE se confunden: autenticación y autorización."

#### Tabla Comparativa (proyectar en pantalla)

| Concepto          | Pregunta           | Ejemplo                                                |
| ----------------- | ------------------ | ------------------------------------------------------ |
| **Autenticación** | ¿Quién eres?       | "Soy Ana García, aquí está mi token JWT"               |
| **Autorización**  | ¿Qué puedes hacer? | "Ana tiene scope 'read', puede leer pero no modificar" |

**Analogía del Aeropuerto**:

> "Imaginen el aeropuerto. Autenticación es cuando muestran su pasaporte en el mostrador: '¿Quién eres?'. Autorización es cuando pasan por seguridad: '¿Tienes permiso para abordar este vuelo?'. Dos cosas distintas."

#### Scopes (Alcances)

**Proyecta la tabla**:

| Scope   | Permisos               | Uso típico             |
| ------- | ---------------------- | ---------------------- |
| `read`  | Solo lectura           | Consultores, auditores |
| `write` | Lectura + modificación | Empleados operativos   |
| `admin` | Todo + configuración   | Administradores        |

> "Los scopes son como llaves. 'read' abre la puerta de consulta, 'write' abre la puerta de modificación, 'admin' abre todas las puertas."

#### Rate Limiting

> "Y rate limiting: limitar cuántas solicitudes puede hacer un usuario por minuto. En este ejercicio: usuarios 'base' → 10 req/min, usuarios 'premium' → 50 req/min. Previene abuso."

**Pausa de validación** (15 segundos):

> "¿Todos claros con autenticación vs autorización? Vamos a implementarlo."

---

### Minutos 2-4: Crear Proyecto (Guiado Rápido)

**Script para decir**:

> "Proyecto rápido, ya saben cómo hacerlo."

#### Comandos rápidos

```powershell
cd src/McpWorkshop.Servers
dotnet new web -n Exercise3Server -f net10.0
cd Exercise3Server
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.15.0
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise3Server/Exercise3Server.csproj
```

**Crear carpetas**:

```powershell
cd src/McpWorkshop.Servers/Exercise3Server
mkdir Security
mkdir Middleware
mkdir Models
```

**Checkpoint** (10 segundos):

> "Si tienen las 3 carpetas creadas, continuamos."

---

### Minutos 4-6: Modelos (Rápido)

**Script para decir**:

> "Dos modelos simples: uno para el usuario autenticado, otro para el rate limiting."

#### AuthenticatedUser

**Muestra el código**:

```csharp
namespace Exercise3Server.Models;

public class AuthenticatedUser
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public string Tier { get; set; } = "base";
}
```

> "Este objeto representa al usuario después de validar su token JWT. Tiene ID, nombre, lista de scopes, y tier (base o premium)."

#### RateLimitInfo

```csharp
namespace Exercise3Server.Models;

public class RateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public int Limit { get; set; }
}
```

> "Este rastrea cuántas solicitudes ha hecho el usuario en la ventana de 1 minuto. Cuando pasa el minuto, reseteamos el contador."

**Pausa** (10 segundos):

> "Copien estos dos modelos del documento. Compilamos después."

---

### Minutos 6-14: Servicios de Seguridad (Semi-Independiente)

**Script para decir**:

> "Ahora los servicios de seguridad. Son 3: JwtAuthenticationService (valida tokens), ScopeAuthorizationService (verifica permisos), RateLimitingService (cuenta solicitudes)."

#### Estrategia de enseñanza

**Opción A (si hay tiempo)**: Explica el primer servicio completo, los otros dos los copian.

**Opción B (si el tiempo apremia)**: "Tienen los 3 servicios en el documento del ejercicio. Cópienlos a la carpeta `Security/`. Voy a explicar las partes clave mientras ustedes copian."

#### Partes clave a explicar (mientras copian)

**1. JwtAuthenticationService - ValidateToken**:

```csharp
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
```

> "Esto valida el token JWT. Verifica que la firma sea correcta con nuestra clave secreta, y que no haya expirado. Si todo OK, devuelve un objeto `ClaimsPrincipal` con los claims."

**2. ScopeAuthorizationService - GetRequiredScopeForMethod**:

```csharp
return method switch
{
    "initialize" => "",          // Público
    "resources/list" => "",      // Público
    "resources/read" => "read",  // Requiere read
    "tools/call" => "write",     // Requiere write
    _ => "admin"
};
```

> "Aquí definimos las reglas: qué scope necesita cada método MCP. `initialize` y `list` son públicos, `read` necesita scope 'read', `call` necesita 'write'."

**3. RateLimitingService - IsAllowed**:

```csharp
if ((now - limitInfo.WindowStart).TotalMinutes >= 1)
{
    limitInfo.RequestCount = 0;
    limitInfo.WindowStart = now;
}

if (limitInfo.RequestCount >= limit)
{
    return false;
}

limitInfo.RequestCount++;
return true;
```

> "Lógica de ventana deslizante. Si pasó 1 minuto, reseteamos el contador. Si el usuario alcanzó su límite, devolvemos false. Si no, incrementamos y permitimos."

**Checkpoint de tiempo** (Minuto 12):

> "Quienes ya tienen los 3 servicios, compilen con `dotnet build`. Quienes aún no, tienen 2 minutos más."

---

### Minutos 14-18: Middlewares y Program.cs (Explicativo)

**Script para decir**:

> "Ahora conectamos todo con middlewares. Los middlewares son filtros que procesan cada solicitud HTTP antes de llegar al endpoint."

#### AuthenticationMiddleware (explicación rápida)

```csharp
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
```

> "Este middleware extrae el token del header `Authorization: Bearer xxx`, lo valida, y si es válido, guarda el usuario en `context.Items['User']` para que el endpoint lo use después."

#### RateLimitingMiddleware (explicación rápida)

```csharp
var user = context.Items["User"] as AuthenticatedUser;

if (user != null && !_rateLimitService.IsAllowed(user))
{
    context.Response.StatusCode = 429;
    // ... devuelve error
    return;
}

await _next(context);
```

> "Este middleware verifica si el usuario alcanzó su límite. Si sí, devuelve error 429 (Too Many Requests). Si no, pasa al siguiente middleware con `await _next(context)`."

#### Program.cs (partes críticas)

**1. Registro de middlewares**:

```csharp
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
```

> "Se ejecutan EN ORDEN. Primero autenticación, luego rate limiting, luego el endpoint."

**2. Endpoint de generación de tokens**:

```csharp
app.MapPost("/auth/token", (TokenRequest request, JwtAuthenticationService authService) =>
{
    var token = authService.GenerateToken(request.UserId, request.Name, request.Scopes, request.Tier, 60);
    return Results.Ok(new { token });
});
```

> "Este endpoint es SOLO PARA TESTING. En producción, la generación de tokens la haría un servicio de autenticación separado (Azure AD, Auth0, etc.). Aquí lo hacemos simple para probar."

**3. Verificación de autorización en el endpoint MCP**:

```csharp
if (user != null && !authz.IsAuthorized(user, request.Method))
{
    logger.LogError(request.Method, requestId, new Exception("Unauthorized"));
    return Results.Ok(CreateErrorResponse(-32004, "Insufficient permissions", ...));
}
```

> "Antes de ejecutar el método, verificamos si el usuario tiene el scope necesario. Si no, error 403 (Forbidden)."

**Mensaje de compilación**:

> "Copien el `Program.cs` completo del documento. Compilen. Debe compilar sin errores."

---

### Minutos 18-20: Probar Escenarios (Validación Rápida)

**Script para decir**:

> "Momento de verdad. Vamos a probar 3 escenarios: sin token (falla), con token 'read' (funciona para lectura), con token 'write' (funciona para herramientas)."

#### Ejecutar servidor (Terminal 2)

```powershell
cd src/McpWorkshop.Servers/Exercise3Server
dotnet run
```

> "Servidor en puerto 5003."

#### Prueba 1: Generar token con scope 'read' (Terminal 3)

```powershell
$body = @{
    userId = "user-001"
    name = "Ana García"
    scopes = @("read")
    tier = "base"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5003/auth/token" -Method POST -Body $body -ContentType "application/json"
$tokenRead = $response.token
Write-Host "Token generado: $tokenRead"
```

> "Token con scope 'read' solamente."

#### Prueba 2: Acceder a resources/read CON token (debe funcionar)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "resources/read"
    params = @{ uri = "mcp://secure-data" }
    id = "read-001"
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $tokenRead" }
Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json"
```

> "Debe devolver datos sensibles. ✅ Funciona porque tiene scope 'read'."

#### Prueba 3: Acceder a tools/call con scope 'read' (debe fallar)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{ name = "secure_action"; arguments = @{ action = "test" } }
    id = "call-001"
} | ConvertTo-Json -Depth 10

$headers = @{ Authorization = "Bearer $tokenRead" }
Invoke-RestMethod -Uri "http://localhost:5003/mcp" -Method POST -Body $body -Headers $headers -ContentType "application/json"
```

> "Debe devolver error 'Insufficient permissions'. ❌ Falla porque 'tools/call' requiere scope 'write'."

**Mensaje final**:

> "¡Perfecto! La seguridad funciona. Tienen autenticación JWT, autorización por scopes, y rate limiting. Esto es producción-ready con algunas mejoras (HTTPS, secretos en Azure Key Vault, etc.)."

---

## 🚨 Contingencias

### Contingencia A: Errores de compilación con JWT (Minuto 10+)

**Problema**: Paquete NuGet no se instaló correctamente.

**Acción**:

1. **Verificar instalación**:

```powershell
dotnet list package
# Debe mostrar System.IdentityModel.Tokens.Jwt 8.15.0
```

1. **Reinstalar si falta**:

```powershell
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.15.0
dotnet restore
```

1. **Si persiste**: Comparte el proyecto completo por chat.

---

### Contingencia B: Confusión con middlewares (Minuto 16+)

**Problema**: No entienden el orden de ejecución.

**Acción de clarificación** (1 minuto):

> "Middlewares son como filtros de agua. El agua (solicitud HTTP) pasa por varios filtros EN ORDEN:
>
> 1. AuthenticationMiddleware: '¿Tienes token válido?'
> 2. RateLimitingMiddleware: '¿No has excedido tu límite?'
> 3. Endpoint MCP: Ejecuta el método solicitado
>
> Si cualquier filtro dice NO, la solicitud se detiene ahí."

**Diagrama en pizarra** (si es presencial):

```
HTTP Request → Auth → Rate Limit → Endpoint → Response
```

---

### Contingencia C: Tiempo insuficiente (Minuto 17+)

**Problema**: Llevas 17 minutos y no has probado.

**Acción**:

1. **Reducir pruebas**: Haz solo 2 pruebas (generar token + acceso exitoso).

2. **Omitir validación de fallo**: "El documento tiene 8 pruebas completas. Ustedes pueden probar el resto después."

3. **Mensaje de cierre rápido**:

> "Excelente. El servidor está funcionando con seguridad. Tienen los 8 tests en el documento para probar todos los escenarios."

**Ganancia de tiempo**: Terminas en el minuto 20.

---

### Contingencia D: Preguntas sobre producción

**Pregunta típica**: "¿Dónde guardamos el secreto JWT en producción?"

**Respuesta rápida**:

> "En producción, NUNCA hardcodeas el secreto. Usas Azure Key Vault, AWS Secrets Manager, o variables de entorno protegidas. En el Bloque 7 veremos mejores prácticas de seguridad."

---

## ✅ Validación de Completitud

Al terminar el ejercicio, pregunta:

> "¿Cuántos pudieron generar un token y hacer al menos 1 solicitud autenticada?"

- **>80% levanta la mano**: ✅ **Ejercicio exitoso**, continúa al Bloque 7.
- **60-80% levanta la mano**: ⚠️ **Revisar problemas comunes**, da 2 minutos extra.
- **<60% levanta la mano**: 🚨 **Contingencia crítica**, ofrece código completo funcionando.

---

## 📊 Métricas de Éxito

| Indicador                             | Objetivo | Resultado Real |
| ------------------------------------- | -------- | -------------- |
| Asistentes que generaron tokens       | >85%     | \_\_\_ %       |
| Asistentes que probaron autenticación | >75%     | \_\_\_ %       |
| Asistentes que entendieron scopes     | >80%     | \_\_\_ %       |
| Tiempo total utilizado                | 20 min   | \_\_\_ min     |

---

## 🎓 Lecciones Aprendidas (Post-Ejercicio)

**Después del ejercicio, refuerza estos conceptos** (1 minuto):

1. **Autenticación vs Autorización**: Dos pasos distintos, ambos necesarios.
2. **JWT**: Tokens auto-contenidos, no requieren consultar DB en cada solicitud.
3. **Scopes**: Modelo flexible de permisos, más granular que roles.
4. **Rate Limiting**: Protección esencial contra abuso.

**Pregunta de reflexión** (30 segundos):

> "¿Por qué usamos JWT en vez de sesiones tradicionales con cookies?"

**Respuesta esperada**:

> "JWT es stateless: el servidor no guarda estado de sesión. Escala mejor porque no necesitas compartir sesiones entre múltiples servidores."

---

## 🔗 Transición al Bloque 7

**Script de cierre** (30 segundos):

> "Excelente trabajo. Ya implementaron seguridad básica pero funcional. En el siguiente bloque vamos a profundizar: mejores prácticas de seguridad, gestión de secretos, auditoría, y cómo desplegar esto en producción de forma segura. Es una sesión de 15 minutos. Tomen agua, estiren las piernas."

**Checklist de transición**:

- [ ] Los asistentes detienen el servidor (Ctrl+C)
- [ ] Confirma que todos tienen el código funcionando
- [ ] Abre el documento del Bloque 7 en VS Code
- [ ] Prepara slides de seguridad (si las tienes)

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Febrero 2026
