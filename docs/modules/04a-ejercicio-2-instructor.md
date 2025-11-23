# Bloque 4: Ejercicio 2 - Guía del Instructor (20 minutos)

**Propósito**: Segundo ejercicio práctico donde los asistentes implementan herramientas con parámetros dinámicos.  
**Formato**: Semi-independiente (instructor explica conceptos, asistentes implementan con apoyo).  
**Nivel**: Intermedio - requiere comprensión del Ejercicio 1.

---

## ⏱️ Timing Detallado

| Minuto | Actividad                                       | Duración  |
| ------ | ----------------------------------------------- | --------- |
| 0-2    | Explicación de recursos vs herramientas         | 2 min     |
| 2-5    | Crear proyecto y estructura                     | 3 min     |
| 5-15   | Implementar 3 herramientas (semi-independiente) | 10 min    |
| 15-18  | Implementar Program.cs                          | 3 min     |
| 18-20  | Probar herramientas                             | 2 min     |
| **20** | **Finalizar ejercicio**                         | **TOTAL** |

---

## 🎯 Objetivo del Instructor

Al terminar este bloque, los asistentes deben:

1. ✅ Entender la diferencia fundamental entre recursos y herramientas
2. ✅ Saber definir JSON Schema para parámetros de entrada
3. ✅ Implementar al menos 2 de las 3 herramientas propuestas
4. ✅ Probar herramientas con diferentes combinaciones de parámetros
5. ✅ Comprender el flujo `tools/list` → `tools/call`

---

## 🧩 Pre-Setup del Instructor

**Antes de comenzar el ejercicio**:

-   [ ] Detén el `Exercise1Server` si está corriendo (puerto 5001 libre)
-   [ ] Confirma que `data/orders.json` existe (incluido en el repositorio)
-   [ ] Prepara 3 terminales en VS Code:
    -   Terminal 1: Comandos de creación de proyecto
    -   Terminal 2: Ejecución del servidor (puerto 5002)
    -   Terminal 3: Tests con PowerShell
-   [ ] Ten las 3 herramientas (`SearchCustomersTool.cs`, `GetOrderDetailsTool.cs`, `CalculateMetricsTool.cs`) en archivos de respaldo
-   [ ] Abre el contrato de referencia: `specs/001-mcp-workshop-course/contracts/exercise-2-parametric-query.json`
-   [ ] Valida que el puerto **5002** está libre:

```powershell
netstat -ano | Select-String "5002"
# No debe devolver nada
```

---

## 📋 Guion del Ejercicio

### Minutos 0-2: Conceptos Clave (Explicativo)

**Script para decir**:

> "Antes de empezar a programar, necesito que entiendan la diferencia clave entre **recursos** y **herramientas** en MCP."

#### Analogía: Biblioteca vs Consultor

> "Imaginen una biblioteca. Los **recursos** son como libros en estanterías: están ahí, los puedes consultar cuando quieras, y siempre devuelven el mismo contenido. En el Ejercicio 1, `mcp://customers` era un recurso: siempre te daba la lista completa de clientes."

> "Las **herramientas**, en cambio, son como un consultor especializado. Le haces una pregunta específica con parámetros, y te devuelve una respuesta calculada dinámicamente. Por ejemplo, 'buscar clientes que vivan en España y se llamen Ana' requiere parámetros: país y nombre."

#### Tabla comparativa (proyectar en pantalla)

| Aspecto         | Recursos                           | Herramientas                    |
| --------------- | ---------------------------------- | ------------------------------- |
| **Métodos MCP** | `resources/list`, `resources/read` | `tools/list`, `tools/call`      |
| **Parámetros**  | Opcional (solo URI)                | Requeridos (JSON Schema)        |
| **Ejemplo**     | `mcp://customers`                  | `search_customers(name="John")` |

> "En este ejercicio vamos a implementar 3 herramientas: búsqueda de clientes, detalles de pedido, y cálculo de métricas."

**Pausa de validación** (15 segundos):

> "¿Tiene sentido la diferencia entre recurso y herramienta? ¿Alguna duda antes de empezar?"

---

### Minutos 2-5: Crear Proyecto (Guiado Rápido)

**Script para decir**:

> "Vamos rápido con la estructura del proyecto porque ya saben cómo hacerlo del Ejercicio 1."

#### Comandos rápidos

**Narración mientras escribes**:

> "Creamos el proyecto Exercise2Server en puerto 5002..."

```powershell
cd src/McpWorkshop.Servers
dotnet new web -n Exercise2Server -f net10.0
cd Exercise2Server
dotnet add reference ../../McpWorkshop.Shared/McpWorkshop.Shared.csproj
cd ../../..
dotnet sln add src/McpWorkshop.Servers/Exercise2Server/Exercise2Server.csproj
```

> "Creamos las carpetas Models y Tools..."

```powershell
cd src/McpWorkshop.Servers/Exercise2Server
mkdir Models
mkdir Tools
```

> "Copiamos los modelos del Ejercicio 1 y creamos Order.cs..."

```powershell
Copy-Item ../Exercise1Server/Models/Customer.cs Models/
Copy-Item ../Exercise1Server/Models/Product.cs Models/
```

**Muestra rápidamente `Models/Order.cs`**:

```csharp
namespace Exercise2Server.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}
```

> "Copien este modelo en `Models/Order.cs` o descárguenlo del documento del ejercicio."

**Checkpoint rápido** (10 segundos):

> "Si tienen las 3 carpetas (Models, Tools) y los 3 modelos, pueden continuar."

---

### Minutos 5-15: Implementar Herramientas (Semi-Independiente)

**Script para decir**:

> "Ahora viene la parte semi-independiente. Van a implementar 3 herramientas en la carpeta `Tools/`. Voy a explicar la primera completa, y las otras dos las hacen ustedes con el documento de guía."

#### Herramienta 1: SearchCustomersTool (Explicación completa, 4 min)

**Narración**:

> "Creamos `Tools/SearchCustomersTool.cs`. Esta herramienta tiene dos métodos: `GetDefinition` para declarar sus parámetros, y `Execute` para ejecutar la búsqueda."

**Parte 1: GetDefinition (JSON Schema)**

```csharp
public static object GetDefinition()
{
    return new
    {
        name = "search_customers",
        description = "Busca clientes por nombre parcial y/o país",
        inputSchema = new
        {
            type = "object",
            properties = new
            {
                name = new
                {
                    type = "string",
                    description = "Nombre parcial del cliente (case-insensitive)"
                },
                country = new
                {
                    type = "string",
                    description = "País del cliente (exacto)"
                }
            },
            required = new string[] { } // Ambos son opcionales
        }
    };
}
```

> "Esto es **JSON Schema**. Define que la herramienta acepta dos parámetros opcionales: `name` y `country`. Ambos son strings. El campo `description` ayuda a los clientes a entender qué hace cada parámetro."

**Parte 2: Execute (Lógica de búsqueda)**

```csharp
public static object Execute(Dictionary<string, JsonElement> arguments, List<Customer> customers)
{
    var query = customers.AsEnumerable();

    // Filtrar por nombre si se proporciona
    if (arguments.TryGetValue("name", out var nameElement))
    {
        var name = nameElement.GetString();
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Filtrar por país si se proporciona
    if (arguments.TryGetValue("country", out var countryElement))
    {
        var country = countryElement.GetString();
        if (!string.IsNullOrEmpty(country))
        {
            query = query.Where(c => c.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
        }
    }

    var results = query.ToList();

    return new
    {
        content = new[]
        {
            new
            {
                type = "text",
                text = $"Se encontraron {results.Count} cliente(s):\n" +
                       JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true })
            }
        }
    };
}
```

> "La lógica es simple: empezamos con todos los clientes, filtramos si hay parámetro `name`, luego si hay `country`, y devolvemos los resultados como JSON. Usan LINQ para filtrar."

**Pausa crítica** (30 segundos):

> "¿Ven cómo funciona? JSON Schema define los parámetros, Execute los recibe y ejecuta la lógica. ¿Preguntas?"

#### Herramientas 2 y 3: GetOrderDetailsTool y CalculateMetricsTool (Trabajo independiente, 6 min)

**Script para decir**:

> "Las otras dos herramientas siguen el mismo patrón. En el documento del ejercicio tienen el código completo. Tienen 6 minutos para copiar/implementar `GetOrderDetailsTool.cs` y `CalculateMetricsTool.cs`. Si tienen dudas, levanten la mano."

**Instrucciones en pantalla**:

1. **GetOrderDetailsTool**:

    - Parámetro requerido: `orderId` (integer)
    - Devuelve: Combinación de Order + Customer + Product

2. **CalculateMetricsTool**:
    - Parámetro requerido: `metricType` (enum: "sales", "average", "top_products")
    - Devuelve: Texto con la métrica calculada

> "Usen el documento del ejercicio, copien el código, y si compilan sin errores, continúan al siguiente paso."

**Estrategia de apoyo**:

-   Camina por el aula/sala virtual
-   Ayuda a quien tiene errores de compilación
-   Valida que al menos 2 de las 3 herramientas estén implementadas antes de continuar

**Checkpoint de tiempo** (Minuto 11):

> "Quienes ya tienen las 3 herramientas, compilen con `dotnet build`. Quienes aún no terminan, tienen 2 minutos más."

---

### Minutos 15-18: Implementar Program.cs (Explicativo Rápido)

**Script para decir**:

> "Ahora conectamos las herramientas con el servidor. Abran `Program.cs`."

**Diferencias clave con Ejercicio 1**:

> "El cambio principal está en el routing: en vez de `resources/list` y `resources/read`, ahora tenemos `tools/list` y `tools/call`."

#### Código clave (muestra las partes importantes)

**1. Configuración (puerto 5002)**:

```csharp
options.Server.Port = 5002;  // Diferente al Exercise1Server
```

**2. Carga de datos (agrega orders)**:

```csharp
var orders = LoadData<Order>("../../../data/orders.json");
```

**3. Routing con tools**:

```csharp
var response = request.Method switch
{
    "initialize" => HandleInitialize(settings),
    "tools/list" => HandleToolsList(),
    "tools/call" => HandleToolsCall(request.Params, customers, products, orders),
    _ => CreateErrorResponse(-32601, "Method not found", null, request.Id)
};
```

**4. HandleToolsList (devuelve definiciones)**:

```csharp
static JsonRpcResponse HandleToolsList(object? requestId)
{
    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = new
        {
            tools = new[]
            {
                SearchCustomersTool.GetDefinition(),
                GetOrderDetailsTool.GetDefinition(),
                CalculateMetricsTool.GetDefinition()
            }
        },
        Id = requestId
    };
}
```

**5. HandleToolsCall (invoca herramientas)**:

```csharp
static JsonRpcResponse HandleToolsCall(
    object? requestId,
    IDictionary<string, object>? parameters,
    List<Customer> customers,
    List<Product> products,
    List<Order> orders)
{
    // Parsear el nombre de la herramienta
    string? toolName = null;
    if (parameters != null && parameters.TryGetValue("name", out var nameValue))
    {
        if (nameValue is JsonElement nameElement)
        {
            toolName = nameElement.GetString();
        }
        else if (nameValue is string strValue)
        {
            toolName = strValue;
        }
    }

    // Parsear los argumentos
    Dictionary<string, JsonElement> arguments;
    if (parameters != null && parameters.TryGetValue("arguments", out var argsValue))
    {
        string argumentsJson;
        if (argsValue is JsonElement argsElement)
        {
            argumentsJson = argsElement.GetRawText();
        }
        else
        {
            argumentsJson = JsonSerializer.Serialize(argsValue);
        }
        arguments = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson)
                    ?? new Dictionary<string, JsonElement>();
    }
    else
    {
        arguments = new Dictionary<string, JsonElement>();
    }

    var result = toolName switch
    {
        "search_customers" => SearchCustomersTool.Execute(arguments, customers),
        "get_order_details" => GetOrderDetailsTool.Execute(arguments, orders, customers, products),
        "calculate_metrics" => CalculateMetricsTool.Execute(arguments, orders, products),
        _ => throw new ArgumentException($"Unknown tool: {toolName}")
    };

    return new JsonRpcResponse
    {
        JsonRpc = "2.0",
        Result = result,
        Id = requestId
    };
}
```

**Mensaje clave**:

> "El patrón es simple: `tools/list` devuelve las definiciones JSON Schema, `tools/call` recibe el nombre de la herramienta y sus argumentos, y ejecuta la lógica correspondiente."

**Checkpoint** (10 segundos):

> "Compilen con `dotnet build`. Si compila limpio, pueden ejecutar."

---

### Minutos 18-20: Probar Herramientas (Validación)

**Script para decir**:

> "Ejecutamos el servidor y probamos las 3 herramientas rápidamente."

#### Ejecutar servidor (Terminal 2)

```powershell
cd src/McpWorkshop.Servers/Exercise2Server
dotnet run
```

> "Deben ver 'Now listening on: http://localhost:5002'."

#### Prueba 1: Tools/List (Terminal 3)

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/list"
    params = @{}
    id = "list-tools"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Deben ver 3 herramientas. ✅"

#### Prueba 2: Search Customers

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "search_customers"
        arguments = @{ name = "John" }
    }
    id = "call-search"
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Deben ver clientes con 'John' en el nombre. ✅"

#### Prueba 3: Calculate Metrics

```powershell
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "calculate_metrics"
        arguments = @{ metricType = "sales" }
    }
    id = "call-metrics"
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:5002/mcp" -Method POST -Body $body -ContentType "application/json"
```

> "Deben ver 'Total de ventas: $XX,XXX.XX'. Si ven esto, ¡su servidor de herramientas funciona! 🎉"

---

## 🚨 Contingencias

### Contingencia A: Errores de compilación en herramientas (Minuto 12+)

**Problema**: Muchos asistentes tienen errores en `SearchCustomersTool.cs`.

**Acción**:

1. **Detener trabajo**:

    > "Pausa general. Veo que hay confusión. Voy a compartir las 3 herramientas completas por chat."

2. **Compartir código**:

    - Sube los 3 archivos (`SearchCustomersTool.cs`, `GetOrderDetailsTool.cs`, `CalculateMetricsTool.cs`) a Teams/Zoom
    - Indica: "Descarguen los archivos, colóquenlos en la carpeta `Tools/`, reemplacen lo que tengan"

3. **Continuar**:

    - Minuto 13: Todos tienen las herramientas
    - Minuto 13-16: Implementan `Program.cs`
    - Minuto 16-20: Prueban

**Ganancia de tiempo**: Terminas en el minuto 20.

---

### Contingencia B: Tiempo insuficiente (Minuto 14+)

**Problema**: Llevas 14 minutos y muchos no terminaron las herramientas.

**Acción**:

1. **Reducir alcance**:

    > "Para ganar tiempo, vamos a implementar solo 2 herramientas: `search_customers` y `calculate_metrics`. Omitan `get_order_details`."

2. **Ajustar Program.cs**:

    - En `HandleToolsList`, comenta la línea de `GetOrderDetailsTool`:

    ```csharp
    tools = new[]
    {
        SearchCustomersTool.GetDefinition(),
        // GetOrderDetailsTool.GetDefinition(),  // OMITIDA
        CalculateMetricsTool.GetDefinition()
    }
    ```

    - En `HandleToolsCall`, comenta el case:

    ```csharp
    var result = toolName switch
    {
        "search_customers" => SearchCustomersTool.Execute(arguments, customers),
        // "get_order_details" => GetOrderDetailsTool.Execute(...),  // OMITIDA
        "calculate_metrics" => CalculateMetricsTool.Execute(arguments, orders, products),
        _ => throw new ArgumentException($"Unknown tool: {toolName}")
    };
    ```

3. **Continuar con 2 herramientas**:

    - Minuto 14-17: Implementan Program.cs
    - Minuto 17-20: Prueban 2 herramientas

**Ganancia de tiempo**: Terminas en el minuto 20, pero con menos funcionalidad.

---

### Contingencia C: JSON Schema confunde a los asistentes

**Problema**: No entienden cómo funciona `inputSchema`.

**Acción de clarificación** (1 minuto):

> "JSON Schema es como un formulario. Ustedes definen qué campos tiene, qué tipo de datos acepta cada campo, y cuáles son obligatorios. Ejemplo:"

```json
{
    "name": "search_customers",
    "inputSchema": {
        "properties": {
            "name": { "type": "string" },
            "country": { "type": "string" }
        },
        "required": [] // Ninguno obligatorio
    }
}
```

> "Esto significa que `search_customers` acepta dos parámetros opcionales: `name` y `country`, ambos strings. El cliente usa esta información para saber cómo llamar a la herramienta."

---

### Contingencia D: Puerto 5002 ocupado

**Problema**: Error "Address already in use".

**Acción rápida**:

> "Cambien el puerto a 5003 en `Program.cs`:"

```csharp
app.Run("http://localhost:5003");
```

> "Y actualicen las URLs de prueba a `http://localhost:5003/mcp`."

---

## ✅ Validación de Completitud

Al terminar el ejercicio, pregunta:

> "¿Cuántos pudieron ejecutar al menos 2 de las 3 herramientas exitosamente?"

-   **>75% levanta la mano**: ✅ **Ejercicio exitoso**, continúa al Bloque 6.
-   **50-75% levanta la mano**: ⚠️ **Revisar problemas comunes**, da 2 minutos extra.
-   **<50% levanta la mano**: 🚨 **Contingencia crítica**, ofrece código completo funcionando.

---

## 📊 Métricas de Éxito

| Indicador                                   | Objetivo | Resultado Real |
| ------------------------------------------- | -------- | -------------- |
| Asistentes que implementaron 3 herramientas | >70%     | \_\_\_ %       |
| Asistentes que implementaron 2 herramientas | >85%     | \_\_\_ %       |
| Asistentes que probaron `tools/call`        | >80%     | \_\_\_ %       |
| Tiempo total utilizado                      | 20 min   | \_\_\_ min     |

---

## 🎓 Lecciones Aprendidas (Post-Ejercicio)

**Después del ejercicio, refuerza estos conceptos** (1 minuto):

1. **Recursos vs Herramientas**:

    - Recursos: Datos pasivos (`resources/read`)
    - Herramientas: Operaciones activas (`tools/call`)

2. **JSON Schema**:

    - Define contrato de entrada
    - Ayuda a clientes a validar parámetros antes de enviar

3. **Pattern Matching con Switch**:
    - Routing simple y legible
    - Fácil agregar nuevas herramientas

**Pregunta de reflexión** (30 segundos):

> "¿Por qué creen que MCP usa JSON Schema en vez de simplemente aceptar cualquier JSON?"

**Respuesta esperada**:

> "JSON Schema valida los parámetros antes de ejecutar la herramienta, evita errores, y documenta automáticamente qué parámetros acepta cada herramienta."

---

## 🔗 Transición al Ejercicio 3

**Script de cierre** (30 segundos):

> "Perfecto. Ya tienen herramientas con parámetros. Pero hay un problema: cualquiera puede invocarlas. En el Ejercicio 3 vamos a agregar **seguridad**: autenticación con JWT, autorización basada en scopes, y rate limiting. Tomen un descanso de 2 minutos."

**Checklist de transición**:

-   [ ] Los asistentes detienen el servidor (Ctrl+C)
-   [ ] Confirma que todos tienen el código funcionando (carpeta `Exercise2Server`)
-   [ ] Abre el documento del Ejercicio 3 en VS Code
-   [ ] Prepara la terminal para el siguiente proyecto

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
