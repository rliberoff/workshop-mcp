# Bloque 9: Ejercicio 5 - Guía para Instructores

**⚠️ Requisito**: Es imprescindible contar con un recurso **Azure OpenAI** configurado y accesible para realizar este ejercicio. Asegúrate de que los alumnos tengan el endpoint y la API key antes de comenzar.

**Duración**: 30 minutos  
**Dificultad**: Avanzada  
**Prerequisitos**: Ejercicios 1, 2 y 3 completados

---

## 🎯 Objetivos Pedagógicos

Este ejercicio es la **culminación del taller**, donde los alumnos:

1. **Integran** todos los conceptos aprendidos (MCP servers, tools, seguridad)
2. **Aplican** el Microsoft Agent Framework para crear agentes conversacionales
3. **Experimentan** con IA generativa en un contexto empresarial real
4. **Comprenden** la arquitectura de sistemas multi-agente con herramientas MCP

## 🏗️ Arquitectura

```text
Usuario (Lenguaje Natural en Español)
    ↓
Microsoft Agent Framework (AIAgent)
    ↓
Function Calling → Selecciona herramientas MCP
    ↓
╔═══════════════╦═══════════════╦═══════════════╗
║  SQL Server   ║  Cosmos DB    ║  REST API     ║
║  MCP Client   ║  MCP Client   ║  MCP Client   ║
╠═══════════════╬═══════════════╬═══════════════╣
║  Clientes     ║  Carritos     ║  Productos    ║
║  Pedidos      ║  Sesiones     ║  Inventario   ║
╚═══════════════╩═══════════════╩═══════════════╝
```

---

## 📋 Preparación Previa (Instructor)

### 1. Verificar Prerequisitos (15 minutos antes)

```powershell
# Verificar que los 3 servidores MCP funcionan
.\scripts\verify-exercise1.ps1
.\scripts\verify-exercise2.ps1
.\scripts\verify-exercise3.ps1

# Verificar Azure OpenAI
az account show
az cognitiveservices account show --name <your-resource> --resource-group <your-rg>
```

### 2. Configurar Azure OpenAI (Si es necesario)

**Opción A: Despliegue compartido** (Recomendado para workshops)

-   Crea un único recurso Azure OpenAI con `gpt-4o` o `gpt-4o-mini`
-   Comparte el endpoint y API key con los alumnos
-   Configura rate limiting para evitar sobrecargas

**Opción B: Despliegue individual**

-   Cada alumno usa su propio recurso Azure OpenAI
-   Requiere que los alumnos tengan suscripciones activas
-   Más lento para setup pero mejor para producción

### 3. Preparar Configuración de Ejemplo

Crea un archivo `appsettings.example.json` para compartir:

```json
{
    "AzureOpenAI": {
        "Endpoint": "https://workshop-openai.openai.azure.com",
        "DeploymentName": "gpt-4o-mini",
        "ApiKey": "PROPORCIONADO_POR_INSTRUCTOR"
    },
    "McpServers": {
        "SqlServer": "http://localhost:5010",
        "CosmosServer": "http://localhost:5011",
        "RestApiServer": "http://localhost:5012"
    },
    "Agent": {
        "Name": "Asistente de Ventas",
        "Instructions": "Eres un asistente virtual experto en datos de e-commerce..."
    }
}
```

---

## 🚀 Desarrollo del Ejercicio

### Fase 1: Introducción (5 minutos)

**Script sugerido**:

> "Hasta ahora hemos construido tres servidores MCP independientes: uno para SQL, otro para Cosmos DB, y otro para REST APIs. Cada uno expone herramientas específicas.
>
> Pero, ¿cómo pueden los usuarios finales usar estas herramientas sin conocer JSON-RPC o saber qué servidor llamar?
>
> Aquí es donde entra el **Microsoft Agent Framework**. Vamos a crear un agente conversacional que:
>
> 1. Entiende preguntas en lenguaje natural (español)
> 2. Decide automáticamente qué servidor MCP usar
> 3. Combina información de múltiples fuentes
> 4. Responde de forma natural
>
> Es como tener un analista de datos con IA que habla español y tiene acceso a todas nuestras bases de datos."

### Fase 2: Setup del Proyecto (5 minutos)

**Puntos clave a enfatizar**:

1. **Paquetes NuGet**:

    ```
    - Azure.AI.OpenAI: Cliente para Azure OpenAI
    - Azure.Identity: Autenticación con Azure
    - Microsoft.Agents.AI.OpenAI: Framework de agentes
    - ModelContextProtocol: SDK de MCP para C#
    ```

2. **Configuración**:
    - Endpoint de Azure OpenAI
    - Modelo deployment (gpt-4o o gpt-4o-mini)
    - URLs de los 3 servidores MCP

**Posibles problemas**:

-   ❌ **Error**: "Package not found" → Asegúrate de agregar `--prerelease`
-   ❌ **Error**: "SDK version mismatch" → Usa .NET 10.0

### Fase 3: Conectar a Servidores MCP (7 minutos)

**Concepto clave**: Transport Layers

Explica las diferencias:

| Transport     | Uso                                | Ejemplo                         |
| ------------- | ---------------------------------- | ------------------------------- |
| **Stdio**     | Proceso local que controlas        | `dotnet run --project ./Server` |
| **HTTP**      | Servidor remoto o en otro puerto   | `http://localhost:5010`         |
| **WebSocket** | Conexión persistente bidireccional | `ws://api.example.com`          |

**⚠️ Nota importante sobre HTTP Transport**:

Los servidores MCP del workshop usan endpoints POST en `/mcp`. Es crítico:

1. **Incluir el path `/mcp` en el endpoint**:

```csharp
var options = new HttpClientTransportOptions
{
    Endpoint = new Uri(serverUrl.TrimEnd('/') + "/mcp")  // ← Importante: agregar /mcp
};
```

2. **Dar tiempo suficiente** para la conexión inicial (el `HttpClientTransport` puede intentar auto-detectar SSE, lo cual puede tomar unos segundos antes de caer back a POST simple)

**Demo en vivo**:

1. Muestra cómo `ListToolsAsync()` descubre herramientas
2. Imprime las herramientas disponibles
3. Explica cómo se convierten a `AITool` usando **McpToolAdapter**

```csharp
// Mostrar en consola
foreach (var tool in sqlTools)
{
    Console.WriteLine($"Tool: {tool.Name}");
    Console.WriteLine($"  Description: {tool.Description}");
}

// IMPORTANTE: Usar McpToolAdapter para convertir a AITools ejecutables
var allAITools = new List<AITool>();
allAITools.AddRange(McpToolAdapter.ConvertToAITools(sqlTools, sqlMcpClient, "SQL Server"));
allAITools.AddRange(McpToolAdapter.ConvertToAITools(cosmosTools, cosmosMcpClient, "Cosmos DB"));
allAITools.AddRange(McpToolAdapter.ConvertToAITools(restApiTools, restApiMcpClient, "REST API"));
```

**💡 Concepto crítico: Por qué necesitamos McpToolAdapter**

> "Las herramientas MCP (`McpClientTool`) son METADATA pura - solo describen las herramientas.
> NO tienen capacidad de ejecución. Si las usas directamente con `Cast<AITool>()`, el agente
> verá las herramientas pero NO podrá ejecutarlas.
>
> El `McpToolAdapter` crea wrappers ejecutables (`AIFunction`) que:
>
> 1. Capturan el `McpClient` para cada servidor
> 2. Parsean argumentos JSON
> 3. Llaman a `CallToolAsync()` en el servidor correcto
> 4. Extraen el contenido de la respuesta MCP
> 5. Devuelven el resultado al agente
>
> Sin este adaptador, el agente fallará silenciosamente al intentar ejecutar las herramientas."

**💡 Punto de énfasis crítico: Calidad de las descripciones**

> "Las **descripciones de herramientas son CRUCIALES** para que el agente las seleccione correctamente. El modelo de IA usa estas descripciones para decidir qué herramienta llamar mediante function calling.
>
> Por ejemplo, para `get_order_details`:
>
> -   ❌ Descripción vaga: "Obtener detalles de un pedido"
> -   ✅ Descripción clara: "Obtiene información detallada de un pedido específico, incluyendo cliente, producto, cantidad y monto total. Usa esta herramienta cuando te pregunten sobre un pedido específico por su número o ID (ejemplo: 'pedido 1001', 'pedido número 1001', 'order 1001')."
>
> Incluye:
>
> 1. **Qué hace** la herramienta
> 2. **Cuándo usarla** (ejemplos de frases del usuario)
> 3. **Qué retorna**
> 4. **Ejemplos de valores** para parámetros"

### Fase 4: Crear el Agente (8 minutos)

**Concepto clave**: Function Calling

Explica cómo funciona:

1. **Usuario pregunta**: "¿Cuántos clientes hay en Madrid?"
2. **LLM analiza**: Necesito usar `list_customers_by_city` con `city=Madrid`
3. **Framework ejecuta**: Llama al servidor SQL MCP
4. **Servidor responde**: `{ count: 342, customers: [...] }`
5. **LLM sintetiza**: "Actualmente hay 342 clientes en Madrid."

**Código crítico**:

```csharp
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: instructions,  // ← MUY IMPORTANTE: Define personalidad y comportamiento
        name: agentName,
        tools: allAITools  // ← Herramientas convertidas con McpToolAdapter
    );
```

**Nota importante**: Asegúrate de que los alumnos tengan los using statements correctos:

-   `using Microsoft.Extensions.AI;` para `AITool`
-   `using OpenAI;` para las extensiones de `CreateAIAgent`

**Punto de énfasis**:

> "Las **instructions** son cruciales. No solo dicen 'responde en español', sino que guían cómo el agente debe comportarse, qué tono usar, y cómo estructurar respuestas."

### Fase 5: Loop de Conversación (5 minutos)

**Concepto clave**: AgentThread = Contexto

```csharp
var thread = agent.GetNewThread();  // ← Crea contexto vacío

while (true)
{
    var userInput = Console.ReadLine();
    var response = await agent.RunAsync(userInput, thread);  // ← Pasa el thread
    Console.WriteLine(response);
}
```

**Demo interactiva**:

Prueba esta secuencia:

```
👤: ¿Cuántos clientes hay en España?
🤖: 1,247 clientes

👤: ¿Y en Madrid?
🤖: 342 clientes  ← El agente recuerda que hablábamos de España
```

Sin thread, el agente no sabría el contexto de "¿Y en Madrid?".

---

## 🔍 Puntos de Atención Durante el Ejercicio

### 1. Problemas Comunes de los Alumnos

#### Errores de Compilación

| Problema                                    | Causa                                                | Solución                                                                  |
| ------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------- |
| "AITool could not be found"                 | Falta `using Microsoft.Extensions.AI;`               | Agregar el using statement al inicio de Program.cs                        |
| "CreateAIAgent not found"                   | Falta `using OpenAI;`                                | Agregar el using para las extensiones de OpenAI                           |
| "McpToolAdapter could not be found"         | Falta crear la clase McpToolAdapter.cs               | Crear el archivo McpToolAdapter.cs con el código del adaptador            |
| "Cast<AITool>() not working"                | Las herramientas MCP no son directamente ejecutables | Usar `McpToolAdapter.ConvertToAITools()` en lugar de `Cast<AITool>()`     |
| "IClientTransport.ConnectAsync not found"   | Implementación incorrecta de IClientTransport        | Usar `Task<ITransport> ConnectAsync()` en lugar de `ReadAsync/WriteAsync` |
| "ModelContextProtocol.Client.Transports..." | Namespace incorrecto                                 | Usar `ModelContextProtocol.Client` y `ModelContextProtocol.Protocol`      |

#### Errores de Ejecución

| Problema                                 | Causa                                            | Solución                                                                                                                                                          |
| ---------------------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| "TimeoutException" o "Failed to connect" | HttpClientTransport intenta usar SSE por defecto | Asegurarse de incluir `/mcp` en el endpoint: `new HttpClientTransportOptions { Endpoint = new Uri(serverUrl + "/mcp") }` y dar tiempo suficiente para la conexión |
| "El agente no responde"                  | Servidores MCP no están corriendo                | Verificar con `Test-NetConnection localhost -Port 5010`                                                                                                           |
| "Authentication failed"                  | Azure credentials no configuradas                | Ejecutar `az login`                                                                                                                                               |
| "El agente responde en inglés"           | Instructions no especifican idioma               | Añadir explícitamente "Siempre responde en español"                                                                                                               |
| "Tools not found"                        | Conexión MCP falló silenciosamente               | Verificar logs de conexión y configuración del transporte                                                                                                         |
| "Rate limit exceeded"                    | Demasiadas peticiones                            | Implementar retry o usar caching                                                                                                                                  |

### 2. Preguntas Frecuentes de los Alumnos

**P: ¿Por qué usar MAF en lugar de llamar OpenAI directamente?**

> R: MAF abstrae complejidad: manejo de tools, streaming, retries, gestión de contexto, logs. Con OpenAI directo, tendrías que implementar todo eso manualmente.

**P: ¿Puedo usar otros LLMs como Claude o Llama?**

> R: Sí, MAF soporta múltiples proveedores. Solo cambia el `ChatClient`:
>
> ```csharp
> var agent = new AnthropicClient(...)
>     .GetChatClient("claude-3-5-sonnet")
>     .CreateAIAgent(...);
> ```

**P: ¿Cómo sé qué herramienta está usando el agente?**

> R: Puedes suscribirte a eventos:
>
> ```csharp
> agent.OnToolCall += (sender, tool) =>
>     Console.WriteLine($"Using tool: {tool.Name}");
> ```

**P: ¿Esto funciona con conversaciones de voz?**

> R: Sí, solo necesitas agregar speech-to-text (Azure Speech) antes del agente y text-to-speech después.

---

## 🎓 Conceptos Avanzados (Para Alumnos Rápidos)

### 1. Multi-Agent Systems

Si un alumno termina rápido, sugiérele crear múltiples agentes especializados:

```csharp
// Agente especialista en clientes
var customerAgent = chatClient.CreateAIAgent(
    instructions: "Solo respondes sobre clientes",
    tools: [.. sqlTools.Where(t => t.Name.Contains("customer"))]
);

// Agente especialista en inventario
var inventoryAgent = chatClient.CreateAIAgent(
    instructions: "Solo respondes sobre productos e inventario",
    tools: [.. restApiTools]
);

// Agente coordinador que decide a quién delegar
var coordinatorAgent = chatClient.CreateAIAgent(
    instructions: "Delegas preguntas a especialistas",
    tools: [customerAgent.AsAIFunction(), inventoryAgent.AsAIFunction()]
);
```

### 2. Streaming de Respuestas

Para mejorar UX, muestra respuestas en tiempo real:

```csharp
await foreach (var chunk in agent.RunStreamingAsync(userInput, thread))
{
    Console.Write(chunk);
}
```

### 3. Observabilidad y Tracing

Añade telemetría:

```csharp
using var activitySource = new ActivitySource("McpWorkshop.Agent");

using var activity = activitySource.StartActivity("ProcessUserQuery");
activity?.SetTag("user.input", userInput);
activity?.SetTag("agent.name", agentName);

var response = await agent.RunAsync(userInput, thread);

activity?.SetTag("response.length", response.Length);
```

---

## 📊 Evaluación y Verificación

### Checklist de Evaluación

Para cada alumno, verifica:

-   [ ] **Conexión exitosa**: Los 3 servidores MCP están conectados
-   [ ] **Discovery de herramientas**: Las 12 herramientas se listan correctamente
-   [ ] **Conversación básica**: El agente responde al menos 1 pregunta
-   [ ] **Contexto**: El agente mantiene contexto en conversaciones multi-turno
-   [ ] **Manejo de errores**: El agente responde gracefully si un servidor falla
-   [ ] **Español**: Las respuestas están en español

### Script de Verificación Automática

```powershell
# .\scripts\verify-exercise5.ps1

$tests = @(
    @{ Query = "¿Cuántos clientes hay en España?"; ExpectedPattern = "\d+" },
    @{ Query = "¿Hay carritos abandonados?"; ExpectedPattern = "carrito" },
    @{ Query = "¿Productos con bajo stock?"; ExpectedPattern = "stock|producto" }
)

foreach ($test in $tests) {
    $response = Invoke-RestMethod -Uri "http://localhost:5014/chat" `
        -Method POST `
        -Body (@{ message = $test.Query } | ConvertTo-Json) `
        -ContentType "application/json"

    if ($response.response -match $test.ExpectedPattern) {
        Write-Host "✅ Passed: $($test.Query)"
    } else {
        Write-Host "❌ Failed: $($test.Query)"
    }
}
```

---

## 🎤 Cierre del Ejercicio (5 minutos)

**Script sugerido para el cierre**:

> "¡Felicidades! Han creado un agente conversacional inteligente que:
>
> ✅ Entiende lenguaje natural en español  
> ✅ Consulta múltiples fuentes de datos (SQL, Cosmos, APIs)  
> ✅ Decide automáticamente qué herramienta usar  
> ✅ Mantiene el contexto de conversaciones  
> ✅ Responde de forma natural
>
> Este es un patrón de arquitectura cada vez más común: **Tool-using Agents**.
>
> En producción, podrían:
>
> -   Exponerlo como API REST con autenticación
> -   Integrarlo con Teams, Slack, o web chat
> -   Añadir más servidores MCP (email, calendario, CRM)
> -   Implementar workflows complejos con múltiples agentes
> -   Añadir guardrails de seguridad y compliance
>
> Lo importante es que la **arquitectura MCP hace que sea fácil extender las capacidades del agente sin modificar su código core**. Solo añades más servidores MCP y el agente los descubre automáticamente."

---

## 📚 Recursos para Compartir

### Documentación

-   [Microsoft Agent Framework Docs](https://learn.microsoft.com/en-us/agent-framework/)
-   [Using MCP with Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/model-context-protocol/using-mcp-tools)
-   [MCP Specification](https://modelcontextprotocol.io/)

### Ejemplos de Código

-   [Agent Framework Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples)
-   [MCP Server Examples](https://github.com/modelcontextprotocol/servers)

### Artículos

-   [Building AI Agents with MCP](https://devblogs.microsoft.com/)
-   [Azure OpenAI Best Practices](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/best-practices)

---

## 🚀 Ideas para Extensiones (Homework)

Sugiéreles a los alumnos que experimenten con:

1. **Multi-Agent Orchestration**: Crear agentes especializados que colaboran
2. **Memory y RAG**: Añadir memoria de largo plazo con Azure AI Search
3. **Web UI**: Crear una interfaz web con SignalR para streaming
4. **Voice Interface**: Integrar Azure Speech para conversaciones de voz
5. **Deployment**: Desplegar en Azure Container Apps con autoscaling
6. **Monitoring**: Añadir Application Insights y alertas

---

## ⚠️ Advertencias Importantes

1. **Costos de Azure OpenAI**:

    - `gpt-4o-mini` es significativamente más barato que `gpt-4o`
    - Configura rate limits para evitar gastos inesperados
    - Considera usar Azure OpenAI PTU para workshops grandes

2. **Rate Limiting**:

    - Con muchos alumnos, puedes alcanzar rate limits
    - Implementa retry logic con exponential backoff
    - Considera usar múltiples deployments

3. **Seguridad**:

    - No compartas API keys en chat público
    - Usa Managed Identity en producción
    - Implementa input validation para prevenir prompt injection

4. **Tiempo**:
    - Este es el ejercicio más complejo
    - Algunos alumnos pueden necesitar más de 30 minutos
    - Ten un ejemplo funcionando listo para mostrar

---

## ✅ Checklist para el Instructor

Antes de comenzar:

-   [ ] Azure OpenAI configurado y accesible
-   [ ] Los 3 servidores MCP funcionan
-   [ ] `appsettings.example.json` compartido
-   [ ] Script de verificación probado
-   [ ] Ejemplo funcionando para demo
-   [ ] Respuestas a preguntas frecuentes preparadas
-   [ ] Plan B si Azure OpenAI falla (usar mocks)

Durante el ejercicio:

-   [ ] Monitorear que los alumnos conectan correctamente
-   [ ] Ayudar con problemas de autenticación
-   [ ] Verificar que las conversaciones funcionen
-   [ ] Tomar nota de problemas comunes para futuras sesiones

Después:

-   [ ] Recoger feedback de los alumnos
-   [ ] Actualizar documentación según problemas encontrados
-   [ ] Compartir recursos adicionales

---

**🎯 Objetivo Final**: Que los alumnos entiendan cómo construir agentes conversacionales que integran múltiples fuentes de datos de forma natural y escalable.
