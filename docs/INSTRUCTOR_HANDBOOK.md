# MCP Workshop - Instructor Handbook

Guía completa para facilitar el taller de Model Context Protocol de 3 horas con máximo impacto pedagógico.

## 📋 Índice Rápido

-   [Preparación Pre-Workshop](#preparación-pre-workshop)
-   [Timing & Gestión del Reloj](#timing--gestión-del-reloj)
-   [Estrategias de Facilitación](#estrategias-de-facilitación)
-   [Manejo de Ejercicios](#manejo-de-ejercicios)
-   [Troubleshooting en Vivo](#troubleshooting-en-vivo)
-   [Engagement & Participación](#engagement--participación)
-   [Contingencias](#contingencias)

---

## ⏱️ Timing & Gestión del Reloj

### Cronometraje Estricto (3h total, ±5 min)

| Bloque                  | Duración   | Acumulado | Alertas de Tiempo            |
| ----------------------- | ---------- | --------- | ---------------------------- |
| 1. Apertura             | 10 min     | 0:10      | ⏰ 8 min: wrap up            |
| 2. Fundamentos          | 25 min     | 0:35      | ⏰ 20 min: último concepto   |
| 3. Anatomía (Live Code) | 20 min     | 0:55      | ⏰ 15 min: síntesis final    |
| 4. Ejercicio 1          | 15 min     | 1:10      | ⏰ 12 min: última ayuda      |
| 5. Ejercicio 2          | 20 min     | 1:30      | ⏰ 15 min: validación rápida |
| **Break**               | **10 min** | **1:40**  | **⏰ Estricto**              |
| 6. Ejercicio 3          | 20 min     | 2:00      | ⏰ 15 min: debugging crítico |
| 7. Seguridad Charla     | 15 min     | 2:15      | ⏰ 12 min: resumen           |
| 8. Ejercicio 4 (Grupos) | 25 min     | 2:40      | ⏰ 20 min: finalizar código  |
| 9. Orquestación Charla  | 15 min     | 2:55      | ⏰ 12 min: conclusiones      |
| 10. Roadmap B2B         | 10 min     | 3:05      | ⏰ 8 min: último caso        |
| 11. Cierre              | 10 min     | 3:15      | ⏰ 5 min: feedback forms     |

### Estrategias para Mantener el Ritmo

1. **Timer Visible**: Proyectar cronómetro en pantalla compartida
2. **Alertas de Voz**: "Quedan 5 minutos para este ejercicio"
3. **Parking Lot**: Post-it virtual para preguntas fuera de tiempo
4. **Slides con Reloj**: Cada slide muestra tiempo restante del bloque

---

## 🎯 Preparación Pre-Workshop

### 72 Horas Antes

**Checklist Técnico**:

```powershell
# Ejecutar validación completa
.\scripts\verify-setup.ps1 -Verbose

# Probar todos los ejercicios en secuencia
.\scripts\run-all-exercises.ps1

# Validar coverage de tests
.\scripts\run-all-tests.ps1 -Coverage $true

# Backup de soluciones
Copy-Item -Recurse src\McpWorkshop.Servers backup\solutions
```

**Materiales**:

-   [ ] Repositorio accesible (GitHub/GitLab)
-   [ ] Slides actualizadas con branding del evento
-   [ ] Datos de ejemplo generados (`.\scripts\create-sample-data.ps1`)
-   [ ] Tokens JWT pre-generados para Exercise 3
-   [ ] Backup de código en USB (contingencia sin internet)

**Comunicación**:

-   [ ] Email con prerequisitos a asistentes (48h antes)
-   [ ] Enlace al repositorio y quickstart.md
-   [ ] Formulario de pre-assessment (conocimientos previos)
-   [ ] Instrucciones de instalación de .NET 10

### 24 Horas Antes

-   [ ] Validar slides en proyector/pantalla del venue
-   [ ] Probar audio/mic con live coding
-   [ ] Confirmar acceso a Wi-Fi del venue
-   [ ] Imprimir 3-4 copias del cheat sheet (backup)
-   [ ] Cargar todos los servidores localmente (contingencia)

### 2 Horas Antes (Día del Workshop)

```powershell
# Setup técnico final
dotnet clean
dotnet restore
dotnet build -c Release

# Levantar todos los servidores de Exercise 4
.\scripts\start-exercise4-servers.ps1

# Validar puertos disponibles
Test-NetConnection localhost -Port 5000,5001,5002,5003,5004
```

-   [ ] Abrir IDE con código de demostración cargado
-   [ ] Tener Postman/Insomnia con colecciones importadas
-   [ ] Browser con pestañas: GitHub repo, MCP spec, docs
-   [ ] Segundo laptop/tablet con soluciones abiertas (referencia rápida)

---

## 🎤 Estrategias de Facilitación

### Bloque 1: Apertura (10 min)

**Objetivo**: Establecer rapport, nivelar expectativas, generar energía inicial.

**Script sugerido**:

> "¡Buenos días! Soy [Nombre], y en las próximas 3 horas vamos a construir juntos 4 servidores MCP desde cero. Al final, tendrás un orquestador que puede responder preguntas como '¿Cuáles son mis top clientes?' coordinando SQL, Cosmos y APIs REST. ¿Quién aquí ya ha usado Claude o ChatGPT? [Show of hands] Perfecto. Pues hoy vamos a ver cómo conectar esos LLMs a TUS datos empresariales de forma segura y estandarizada."

**Engagement Hooks**:

-   **Poll en vivo**: "¿Cuántos han integrado un LLM en producción?" (Slido/Mentimeter)
-   **Demo rápida** (30 seg): Mostrar VirtualAnalyst respondiendo pregunta en español
-   **Expectativas**: "Al final del día, cada uno tendrá código ejecutable y deployable en Azure"

**Red Flags**:

-   ❌ **Si más del 30% no tiene .NET 10**: Ofrecer pair programming durante ejercicios
-   ❌ **Si Wi-Fi es débil**: Activar plan B (repositorio local en USB)

### Bloque 2: Fundamentos MCP (25 min)

**Objetivo**: SC-007 - Asistentes articulan diferencia entre MCP y plugins tradicionales.

**Estrategia de Enseñanza**: Método Socrático + Analogía.

**Analogía Recomendada**:

> "MCP es como USB-C para IA. Antes teníamos plugins específicos para cada app (Lightning para iPhone, microUSB para Android, propietarios para laptops). MCP es el estándar universal: un servidor, múltiples clientes (Claude, ChatGPT, tu agente custom)."

**Chequeo de Comprensión** (min 15):

-   **Pregunta al grupo**: "Si necesito conectar 5 LLMs a 10 fuentes de datos, ¿cuántas integraciones necesito?"
    -   ❌ Sin MCP: 50 integraciones (5x10)
    -   ✅ Con MCP: 10 servidores MCP + 5 clientes (15 integraciones)

**Slides Críticas**:

1. Arquitectura MCP (diagrama cliente-servidor)
2. Comparativa MCP vs REST API (tabla)
3. Ejemplo real: CRM Enrichment (caso B2B)

**Tiempo de Preguntas** (min 22-25): Máximo 3 preguntas. Resto a parking lot.

### Bloque 3: Anatomía de un Proveedor - Live Coding (20 min)

**Objetivo**: SC-009 - Live coding sin errores críticos.

**Setup Previo**:

```powershell
# Abrir proyecto limpio en IDE
cd src\McpWorkshop.Servers\DemoServer
code .

# Terminal lista con comandos preparados
dotnet new web -n DemoMcpServer
cd DemoMcpServer
dotnet add package ModelContextProtocol --prerelease
```

**Guion de Live Coding** (paso a paso en [03b-anatomia-proveedor.md](./modules/03b-anatomia-proveedor.md)):

1. **Min 0-5**: Crear proyecto + instalar NuGet
2. **Min 5-10**: Implementar `initialize` endpoint
3. **Min 10-15**: Agregar `resources/list` con 2 recursos
4. **Min 15-18**: Probar con PowerShell/Postman
5. **Min 18-20**: Síntesis y preview de Exercise 1

**Manejo de Errores en Vivo**:

-   ✅ **Error de compilación**: "Perfecto, este es un error común. ¿Alguien ve qué falta?" (involucrar a audiencia)
-   ✅ **Puerto ocupado**: "Esto pasa en producción. Solución: variable de entorno `ASPNETCORE_URLS`"
-   ❌ **Error crítico desconocido**: Activar Plan B (video pre-grabado de 8 min en backup)

**Contingencia - Plan B**:
Si live coding falla críticamente (>3 min debugging):

1. Mostrar video pregrabado (8 min)
2. Usar tiempo restante (12 min) para Q&A anticipado
3. Saltar directo a Exercise 1 (no retrasar agenda)

---

## 💻 Manejo de Ejercicios

### Estrategia General

| Ejercicio  | Formato       | Supervisión                        | Intervención             |
| ---------- | ------------- | ---------------------------------- | ------------------------ |
| Exercise 1 | Guiado        | Activa (caminar entre mesas)       | Alta (cada 5 min)        |
| Exercise 2 | Independiente | Pasiva (disponible para preguntas) | Media (on-demand)        |
| Exercise 3 | Semi-guiado   | Activa (security es crítico)       | Alta (JWT setup)         |
| Exercise 4 | Grupos 3-5    | Moderada (rotar entre grupos)      | Media (validación final) |

### Exercise 1: Static Resources (15 min guiado)

**Objetivo de Éxito**: SC-002 - 80% completan en 15 min.

**Milestone Checkpoints**:

-   **Min 3**: "¿Todos tienen el proyecto compilando? Levantar mano si no."
-   **Min 7**: "¿Quién ya implementó `resources/list`? OK, los que falta: revisar línea 42 del template."
-   **Min 12**: "Último paso: probar con el script. Los que terminaron, ayuden a su vecino."

**Troubleshooting Rápido**:
| Error Común | Solución en 30 seg |
|-------------|-------------------|
| "Port 5000 in use" | `$env:ASPNETCORE_URLS="http://localhost:5001"` |
| "customers.json not found" | Verificar `Build Action: Content`, `Copy if newer` |
| "Invalid JSON response" | Revisar encoding (debe ser UTF-8) |

**Validación Final** (min 14-15):

```powershell
# Ejecutar script de verificación en proyector
.\scripts\verify-exercise1.ps1

# Salida esperada: ✅ 2/2 recursos, ✅ JSON válido, ✅ <500ms
```

### Exercise 2: Parametric Query (20 min independiente)

**Objetivo**: SC-003 - 70% completan en 20 min.

**Reducción de Intervención**: Fomentar autonomía.

**Estrategia de Ayuda**:

1. **Min 0-10**: Solo responder preguntas si levantan mano
2. **Min 10-15**: Caminar entre mesas, observar pantallas (silent supervision)
3. **Min 15-20**: Ofrecer hints si más del 40% está bloqueado

**Hint Progresivo** (si están atascados en schema):

> "El `inputSchema` es JSON Schema estándar. Busquen 'type', 'properties', 'required'. Tienen un ejemplo completo en la documentación del ejercicio, sección 3.2."

**Validación Express** (min 19):

-   No ejecutar script completo (consume tiempo)
-   Solo validar 1 tool: `search_customers`
-   Resto lo validan ellos en el break

### Exercise 3: Security (20 min semi-guiado)

**Objetivo**: 60% implementan seguridad completa.

**Challenges Anticipados**:

-   JWT signature validation (más complejo)
-   Rate limiting logic (conceptual)

**Andamiaje Pedagógico**:

1. **Min 0-5**: Explicar JWT structure en pizarra (header.payload.signature)
2. **Min 5-10**: Proveer tokens pre-generados (evitar debugging de generación)
3. **Min 10-15**: Implementar middleware (siguiendo template)
4. **Min 15-18**: Probar con Postman (requests con/sin token)
5. **Min 18-20**: Discutir rate limiting (pueden implementar en casa)

**Tokens Pre-Generated** (distribuir en chat):

```
# Admin token (valid 1 hour)
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJhZG1pbiIsInNjb3BlIjoibWNwOnJlc291cmNlczpyZWFkIG1jcDp0b29sczpleGVjdXRlIiwiZXhwIjoxNzM0NTYwMDAwfQ.SIGNATURE

# Viewer token (read-only)
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ2aWV3ZXIiLCJyb2xlIjoidmlld2VyIiwic2NvcGUiOiJtY3A6cmVzb3VyY2VzOnJlYWQiLCJleHAiOjE3MzQ1NjAwMDB9.SIGNATURE
```

### Exercise 4: Virtual Analyst (25 min grupos)

**Objetivo**: SC-004 - 90% grupos demuestran funcionalidad.

**Formación de Grupos** (min 0-2):

-   Grupos de 3-5 personas
-   Mezclar niveles (junior + senior)
-   Asignar roles:
    -   🏗️ **Architect**: Diseña flujo de orquestación
    -   💻 **Coder 1**: Implementa parser de queries
    -   💻 **Coder 2**: Implementa caching
    -   🧪 **Tester**: Valida con verify script
    -   📝 **Documenter**: Anota decisiones (para presentación)

**Checkpoint de Progreso**:

-   **Min 8**: "¿Todos los grupos tienen los 3 servidores MCP corriendo?"
-   **Min 15**: "¿Quién ya logró una consulta simple (e.g., clientes de España)?"
-   **Min 22**: "Último sprint: prueben la pregunta más compleja del contrato."

**Estrategia de Rescate** (si un grupo va muy atrasado):

-   **Min 18**: Ofrecer código de ejemplo simplificado
-   **Min 23**: Permitir demostrar funcionalidad parcial (e.g., solo SQL + Cosmos, sin REST)

**Presentaciones Rápidas** (opcional, si tiempo permite):

-   1 min por grupo
-   Mostrar 1 query funcionando en vivo
-   Nota: Solo si van adelantados. Priorizar contenido de Bloques 9-11.

---

## 🔥 Troubleshooting en Vivo

### Top 10 Problemas & Soluciones Instantáneas

#### 1. "dotnet: command not found"

```powershell
# Solución inmediata
# 1. Verificar PATH
$env:PATH -split ';' | Select-String 'dotnet'

# 2. Reinstalar .NET SDK (toma 5 min - usar tiempo de break)
winget install Microsoft.DotNet.SDK.10

# 3. Plan B: Pair programming con compañero
```

#### 2. "ModelContextProtocol package not found"

```powershell
# Prerelease flag olvidado
dotnet add package ModelContextProtocol --prerelease

# Si persiste: usar feed alternativo
dotnet add package ModelContextProtocol --source https://api.nuget.org/v3/index.json --prerelease
```

#### 3. "Port 5000-5004 already in use"

```powershell
# Solución 1: Cambiar puerto
$env:ASPNETCORE_URLS="http://localhost:5010"
dotnet run

# Solución 2: Matar proceso existente
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

#### 4. "JSON deserialization error"

```csharp
// Error común: missing JsonSerializerOptions
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};
var result = JsonSerializer.Deserialize<Customer>(json, options);
```

#### 5. "JWT signature invalid"

```csharp
// Usar secret correcto (appsettings.json)
var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]);

// Validar issuer/audience coinciden
ValidIssuer = "mcp-workshop",
ValidAudience = "mcp-servers"
```

#### 6. "Rate limit middleware not working"

```csharp
// Verificar orden en pipeline (ANTES de endpoints)
app.UseRateLimiting();  // ← Debe ir aquí
app.UseAuthorization();
app.MapControllers();
```

#### 7. "Cosmos DB connection timeout"

```powershell
# Para workshop: usar local JSON files
# No requiere Cosmos real
cd src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Data
ls *.json  # sessions.json, cart-events.json deben existir
```

#### 8. "CORS error in browser"

```csharp
// Agregar política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

#### 9. "Test failures in verify script"

```powershell
# Debugging paso a paso
$body = @{ jsonrpc="2.0"; method="resources/list"; id=1 } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json" -Verbose
```

#### 10. "Out of memory (Exercise 4 con 4 servidores)"

```powershell
# Reducir servers activos (solo SQL + Cosmos)
# REST server es opcional para demostración básica
cd src/McpWorkshop.Servers/Exercise4VirtualAnalyst
# Comentar RestMcpClient en OrchestratorService.cs
```

---

## 🎭 Engagement & Participación

### Técnicas para Mantener Energía

#### Inicio de Cada Bloque

-   **Hook de 30 seg**: Pregunta provocativa o dato impactante
    -   Bloque 2: "¿Sabían que el 73% de integraciones de IA fallan por falta de estandarización?"
    -   Bloque 7: "LinkedIn reportó 400 intentos de acceso no autorizados por segundo en Q4 2024. Seguridad no es opcional."

#### Puntos Medios (Evitar "Valley of Death")

-   **Min 90 (post-break)**: Quick poll - "¿Qué ejercicio ha sido más desafiante hasta ahora?"
-   **Min 120**: Stand-up stretch (30 seg) - "Todos de pie, respiren hondo, continuamos con orquestación."

#### Técnicas Específicas

**Think-Pair-Share** (para conceptos complejos):

1. **Think** (1 min): "¿Cuándo usarían parallel vs sequential integration?"
2. **Pair** (2 min): Discutir con compañero
3. **Share** (1 min): 2-3 grupos comparten con todos

**Live Debugging Theater** (durante live coding):

> "OK, tengo este error [mostrar stack trace]. ¿Qué haríamos en producción? [Pausa dramática] Exacto: leer el mensaje de error completo. Dice 'NullReferenceException line 42'. Vamos allá."

**Gamification Ligera**:

-   **Badge virtual**: Quien completa Exercise 4 primero: "🏆 MCP Master"
-   **Leaderboard de tests**: Mostrar cobertura de tests por ejercicio
-   **Nota**: No debe generar presión negativa. Solo diversión.

### Manejo de Preguntas Difíciles

**Categorías de Preguntas**:

1. **Clarification** (respuesta corta: 30 seg)

    > "¿El rate limiting es por usuario o por IP?"
    > **R**: "En Exercise 3 es por usuario (requiere JWT). En prod, considerarías ambos: IP para DoS, usuario para fair use. Ver Bloque 7 slide 14."

2. **Deep Dive** (parking lot)

    > "¿Cómo implementarían distributed tracing con OpenTelemetry?"
    > **R**: "Excelente pregunta para después del workshop. Tengo recursos en Bloque 9, slide 18. Hablemos en el break."

3. **Off-Topic** (redirigir amablemente)

    > "¿MCP funciona con GPT-4o?"
    > **R**: "Sí, MCP es agnóstico del modelo. Hay un link en la documentación. Sigamos con el ejercicio para llegar a tu caso de uso."

4. **Challenge to Instructor** (validar y re-encuadrar)
    > "¿No sería más fácil usar webhooks directos sin MCP?"
    > **R**: "Gran punto. Webhooks son válidos para 1-2 integraciones. MCP escala cuando tienes 5+ fuentes y múltiples consumidores. Veremos ROI en Bloque 10. ¿Cuántas integraciones gestionas actualmente?"

---

## 🆘 Contingencias

### Scenario A: Internet Cae

**Impacto**: No pueden descargar NuGet packages, acceder a GitHub.

**Plan B**:

1. **Pre-Workshop**: Crear `offline-packages.zip` con:

    ```powershell
    # Empaquetar todos los NuGets localmente
    dotnet pack -o offline-packages
    ```

2. **Durante Workshop**: Distribuir vía USB o carpeta compartida local

    ```powershell
    dotnet restore --source ./offline-packages
    ```

3. **Documentación**: Tener copia local del repo en cada laptop del instructor

**Tiempo de Recuperación**: 5 min

### Scenario B: Proyector Falla

**Impacto**: No pueden ver live coding ni slides.

**Plan B**:

1. **Descripción verbal detallada**: "Estoy escribiendo: `app.MapPost("/", async context => ...`"
2. **Compartir código en chat** cada 2 min
3. **Usar IDE con font gigante** (size 24+) para los de primeras filas

**Tiempo de Recuperación**: Continuar sin proyector (subóptimo pero viable)

### Scenario C: Más del 50% No Completaron Exercise 1 en Tiempo

**Impacto**: Riesgo de colapso de agenda.

**Plan C** (triage de contenido):

1. **Skip Exercise 2 completo** (usar solo demostración)
2. **Exercise 3**: Mostrar pre-implementado (no hacer en vivo)
3. **Exercise 4**: Demo instructor solamente
4. **Extender Q&A** (usar tiempo liberado para dudas)

**Trade-off**: Pierden práctica, ganan conceptos teóricos sólidos.

### Scenario D: Asistente con Problema Bloqueante (3+ min)

**Impacto**: Retrasa a todo el grupo.

**Plan D**:

1. **Min 1-2**: Intentar solución rápida
2. **Min 3**: Asignar buddy (otro asistente ayuda offline)
3. **Instructor**: Continuar con el grupo mayoritario
4. **Break**: Resolver individualmente el caso bloqueante

**Comunicación clave**: "Te dejo con [Nombre] que ya resolvió esto. Yo sigo para que el grupo avance. En el break volvemos juntos."

---

## 📋 Checklist de Inicio (Imprimir y Laminar)

**30 min antes del workshop**:

-   [ ] Laptop conectado y cargando
-   [ ] Proyector configurado (resolución, duplicar pantalla)
-   [ ] Audio/mic funcionando
-   [ ] Wi-Fi testeado (speed test > 10 Mbps)
-   [ ] Todos los servidores compilando (`dotnet build -c Release`)
-   [ ] Browser con pestañas:
    -   [ ] GitHub repo
    -   [ ] MCP spec
    -   [ ] Slack/Discord de soporte
    -   [ ] Timer online (visible para asistentes)
-   [ ] IDE configurado:
    -   [ ] Font size 16+ (legible en proyector)
    -   [ ] Dark theme (menos fatiga visual)
    -   [ ] Snippets de código precargados
-   [ ] PowerShell/Terminal abierta con comandos listos
-   [ ] Postman con colección del workshop importada
-   [ ] USB backup con:
    -   [ ] Repo completo
    -   [ ] NuGet packages offline
    -   [ ] Video de live coding (contingencia)
-   [ ] Impresos:
    -   [ ] 5 copias de cheat sheet
    -   [ ] Esta checklist
    -   [ ] Lista de asistentes (para networking)

**Último chequeo (5 min antes)**:

```powershell
# Validación final
.\scripts\verify-setup.ps1 -Verbose
.\scripts\start-exercise4-servers.ps1
Start-Sleep 5
Invoke-RestMethod http://localhost:5000/health
```

---

## 🌟 Principios Clave de Facilitación

1. **Start Strong, End Strong**: Primeros 10 min y últimos 10 min son críticos para impresión.
2. **Safety First**: Crear ambiente donde errores son oportunidades de aprendizaje.
3. **Progresive Disclosure**: No abrumar con detalles de implementación al inicio.
4. **Active Learning > Passive Watching**: Ejercicios prácticos maximizan retención.
5. **Adaptabilidad**: Leer la sala. Si están perdidos, ralentizar. Si dominan, acelerar.
6. **Energy Management**: Instructor con energía alta contagia al grupo (hasta post-break).
7. **Celebrate Small Wins**: Reconocer públicamente a quien completa cada ejercicio.

---

## 📞 Soporte Post-Workshop

Proveer a asistentes:

1. **Email de seguimiento** (enviar en 24h):

    - Link a grabación (si se grabó)
    - Recursos adicionales
    - Encuesta de feedback

2. **Canal de comunicación**:

    - Discord/Slack para dudas (1 semana de soporte)
    - Office hours virtuales (1h, 3 días después)

3. **Materiales extra**:
    - Certificado de asistencia (PDF)
    - Badge para LinkedIn
    - Casos de uso expandidos

---

**¡Éxito en tu workshop!** 🚀

Para más detalles, consultar [CHECKLIST.md](./CHECKLIST.md) y notas específicas de cada módulo en [modules/](./modules/).
