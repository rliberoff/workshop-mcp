# Bloque 7: Ejercicio 4 - Orquestador - Notas para el Instructor

**Duración objetivo**: 25 minutos (ejercicio en grupo)  
**Estilo**: Supervisión y troubleshooting, con soporte individual por equipos

---

## ⏱️ Cronometraje Detallado

| Sección                                      | Tiempo | Acumulado | Checkpoint                   |
| -------------------------------------------- | ------ | --------- | ---------------------------- |
| Formación de equipos y lectura del enunciado | 3 min  | 3 min     | Equipos formados             |
| Implementación (equipos trabajando)          | 15 min | 18 min    | Código base completo         |
| Pruebas y debugging                          | 5 min  | 23 min    | Al menos 2 pruebas exitosas  |
| Retrospectiva y aprendizajes                 | 2 min  | 25 min    | Conceptos clave consolidados |

**⚠️ Alerta de tiempo**: Si a los 10 minutos varios equipos están bloqueados en la estructura del proyecto, haz una pausa grupal de 2 minutos para mostrar la estructura correcta.

---

## 🎬 Setup Previo (5 minutos antes del bloque)

### Checklist Pre-Ejercicio

- [ ] Verificar que los 3 servidores MCP están corriendo y funcionando:
  - SqlMcpServer en <http://localhost:5010>
  - CosmosMcpServer en <http://localhost:5011>
  - RestApiMcpServer en <http://localhost:5012>
- [ ] Proyector o pantalla compartida con el documento del ejercicio visible
- [ ] Solución completa del Exercise4Server preparada como backup
- [ ] Timer visible (25 minutos)
- [ ] Canal de Slack/Teams abierto para preguntas rápidas

**CRÍTICO**: Tener el código completo y funcional del Exercise4Server disponible por si algún equipo necesita referencia.

---

## 🎤 Script de Apertura (2 minutos)

> "Perfecto, ahora viene el ejercicio más complejo del taller: el Orquestador Multi-Fuente. Van a trabajar en equipos de 3-5 personas durante 25 minutos.
>
> El objetivo es crear un orquestador que coordina 3 servidores MCP diferentes (SQL, Cosmos DB, REST API) para responder preguntas de negocio en español. Es el escenario más cercano a la realidad que encontrarán en producción.
>
> Los servidores MCP ya están corriendo. Ustedes solo implementan el orquestador. El documento tiene todo el código paso a paso, pero van a aprender más si lo tipean y entienden cada línea.
>
> Formen equipos ahora y lean el enunciado completo antes de empezar a codear. Tienen 25 minutos. ¿Preguntas rápidas antes de empezar?"

**Acción**: Iniciar timer de 25 minutos visible.

---

## 📋 Puntos de Control Durante el Ejercicio

### Minuto 5: Verificar Estructura del Proyecto

**Camina entre equipos y verifica**:

- ¿Crearon las carpetas `Orchestration/`, `Models/`, `Parsers/`?
- ¿Agregaron la referencia a `McpWorkshop.Shared`?
- ¿Tienen los 3 archivos base (`McpServerClient.cs`, `QueryRequest.cs`, `SpanishQueryParser.cs`)?

**Si varios equipos están atascados**: Haz una pausa grupal de 2 minutos y muestra la estructura en pantalla.

---

### Minuto 12: Verificar Implementación del Parser

**Pregunta clave**: "¿Su parser reconoce las 4 intenciones (new_customers, abandoned_carts, order_status, sales_summary)?"

**Señales de problemas**:

- No usan `ToLowerInvariant()` antes de buscar keywords → fallarán las pruebas
- No extraen parámetros (país, horas, orderId) correctamente
- Regex mal formateado → no extrae números de pedido

**Solución rápida**: Muéstrales el método `ExtractOrderId()` completo si están atascados.

---

### Minuto 18: Verificar Orquestador y Cliente MCP

**Problema común #1**: El `McpServerClient` no extrae correctamente la respuesta JSON-RPC.

**Síntoma**: Las trazas del servidor SQL muestran que respondió, pero el orquestador no recibe los datos.

**Causa**: El formato MCP estándar envuelve el resultado en `result.content[0].text`, pero el cliente intenta deserializar directamente desde `result`.

**Solución**: Asegúrate de que el `CallToolAsync` extrae correctamente:

```csharp
// ✅ CORRECTO - Extrae result.content[0].text
if (resultProperty.TryGetProperty("content", out var contentProperty) && contentProperty.ValueKind == JsonValueKind.Array)
{
    var firstContent = contentProperty.EnumerateArray().FirstOrDefault();
    if (firstContent.TryGetProperty("text", out var textProperty))
    {
        var textValue = textProperty.GetString();
        if (textValue != null)
        {
            return JsonSerializer.Deserialize<T>(textValue);
        }
    }
}
```

**Problema común #2**: Error de tipo al pasar `orderId` como string en lugar de int.

**Síntoma**: Error "requires an element of type 'Number', but the target element has type 'String'".

**Solución**: Convertir a `int` antes de pasar a los tools:

```csharp
// ✅ CORRECTO
orderId = int.Parse(parameters["orderId"])
```

---

### Minuto 23: Pruebas Finales

**Acción**: Pide a los equipos que ejecuten al menos 2 de las 4 pruebas del documento.

**Prueba mínima requerida**: Estado de pedido (Prueba 4) - es la más compleja y demuestra orquestación secuencial.

**Si un equipo termina antes**: Pídeles que implementen una nueva intención (ejemplo: "productos más vendidos").

---

## 🐛 Troubleshooting en Vivo

### Error 1: "Failed to connect to MCP server"

**Causa**: Alguno de los 3 servidores MCP no está corriendo.

**Solución rápida**:

```powershell
# Verificar cada servidor
Invoke-RestMethod -Uri "http://localhost:5010/" -Method GET  # SQL
Invoke-RestMethod -Uri "http://localhost:5011/" -Method GET  # Cosmos
Invoke-RestMethod -Uri "http://localhost:5012/" -Method GET  # REST
```

Si uno falla, levántalo con `dotnet run` en la carpeta correspondiente.

---

### Error 2: "Unknown tool: get_order_details"

**Causa**: El servidor SQL no tiene registrado el tool o no se recompiló.

**Verificación**: Al iniciar SqlMcpServer, debe mostrar:

```text
🔧 Tools: query_customers_by_country, get_sales_summary, get_order_details
```

**Solución**: Recompilar y reiniciar el servidor SQL:

```powershell
cd src/McpWorkshop.Servers/SqlMcpServer
dotnet build
dotnet run
```

---

### Error 3: Parser no reconoce consultas

**Causa**: No usan `ToLowerInvariant()` o las keywords están mal escritas.

**Prueba diagnóstica**:

```csharp
var query = "¿Cuántos clientes nuevos hay?";
var parsed = parser.Parse(query);
Console.WriteLine($"Intent: {parsed.Intent}"); // Debe ser "new_customers", no "unknown"
```

**Solución**: Asegúrate de normalizar:

```csharp
query = query.ToLowerInvariant();
```

---

### Error 4: No recibe datos del servidor MCP (problema principal detectado)

**Causa**: El `McpServerClient` no extrae correctamente `result.content[0].text` del formato JSON-RPC.

**Diagnóstico**: Revisa las trazas del servidor SQL. Si muestra "Order found" pero el orquestador dice "Pedido no encontrado", el problema es la deserialización.

**Solución**: Actualizar el método `CallToolAsync` en `McpServerClient.cs` para extraer el campo `text` del array `content`:

```csharp
// El formato MCP estándar es: { result: { content: [ { type: "text", text: "..." } ] } }
if (resultProperty.TryGetProperty("content", out var contentProperty) && contentProperty.ValueKind == JsonValueKind.Array)
{
    var firstContent = contentProperty.EnumerateArray().FirstOrDefault();
    if (firstContent.TryGetProperty("text", out var textProperty))
    {
        var textValue = textProperty.GetString();
        if (textValue != null)
        {
            return JsonSerializer.Deserialize<T>(textValue);
        }
    }
}
```

**Verificación**: Después del cambio, la consulta "¿Cuál es el estado del pedido #1001?" debe devolver el objeto completo con `order`, `inventory` y `shipping`.

---

## 🎯 Retrospectiva Grupal (2 minutos al final)

### Preguntas de Reflexión

> "Antes de terminar, quiero que reflexionen 30 segundos sobre estas preguntas:
>
> 1. ¿Cuál fue el desafío técnico más difícil? ¿Parser, orquestador, o cliente MCP?
> 2. ¿Cuándo usarían patrón paralelo vs secuencial en su trabajo?
> 3. ¿Qué agregarían a este orquestador para llevarlo a producción?"

**Respuestas esperadas**:

1. **Desafío técnico**: Generalmente el cliente MCP (deserialización del formato JSON-RPC) y el parser (regex para extraer parámetros)
2. **Paralelo vs Secuencial**: Paralelo cuando las consultas son independientes; secuencial cuando una depende de los datos de otra
3. **Producción**: Autenticación, rate limiting, logging estructurado, métricas, circuit breaker, retry policies

**Acción**: Toma nota mental de los comentarios para el cierre del taller.

---

## 📊 Métricas de Éxito del Ejercicio

Al final del ejercicio, verifica:

- [ ] Al menos 80% de los equipos tienen el orquestador compilando sin errores
- [ ] Al menos 60% de los equipos completaron 2+ pruebas exitosamente
- [ ] Todos los equipos entienden la diferencia entre patrón paralelo y secuencial
- [ ] Identificaste los errores más comunes para mencionar en el cierre

---

## 🔗 Transición al Siguiente Bloque

> "Excelente trabajo. Este ejercicio les mostró el poder real de MCP: orquestar múltiples fuentes de datos con código limpio y mantenible.
>
> Pero falta algo crítico: ¿qué pasa si uno de los servidores tarda 30 segundos? ¿O si un usuario malicioso envía 1000 queries por segundo? En el siguiente bloque vamos a hablar de seguridad, gobernanza, y antipatrones en producción. 5 minutos de descanso."

**Acción**: Cerrar timer, agradecer participación, y anunciar descanso de 5 minutos.

---

## 📝 Notas para la Próxima Iteración del Taller

### Mejoras Sugeridas

- [ ] Crear un snippet de código del `McpServerClient` correcto para copiar/pegar rápido
- [ ] Grabar un video corto (2 min) del proceso de debugging del problema de deserialización
- [ ] Incluir un test unitario del parser en el documento del alumno
- [ ] Agregar diagrama de secuencia del flujo de datos en el patrón secuencial

### Feedback de Alumnos (llenar después del taller)

- ¿Qué parte fue más confusa?
- ¿El tiempo de 25 minutos fue suficiente?
- ¿Los mensajes de error fueron claros?
- ¿Preferirían más o menos código base pre-escrito?

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Febrero 2026
