# Bloque 6: Seguridad y Gobernanza - Guía del Instructor (15 minutos)

**Propósito**: Corto inciso para hablar sobre patrones empresariales y mejores prácticas de seguridad para despliegues MCP en producción.  
**Formato**: Presentación con ejemplos prácticos y casos reales.  
**Nivel**: Intermedio - todos deben comprender los conceptos.

---

## ⏱️ Timing Detallado

| Minuto | Actividad                                            | Duración  |
| ------ | ---------------------------------------------------- | --------- |
| 0-3    | Autenticación en producción (Azure AD, Key Vault)    | 3 min     |
| 3-6    | Autorización avanzada (scopes jerárquicos, recursos) | 3 min     |
| 6-9    | Auditoría y compliance (logging, GDPR)               | 3 min     |
| 9-12   | Despliegue seguro en Azure (arquitectura, HTTPS)     | 3 min     |
| 12-14  | Monitoreo y alertas                                  | 2 min     |
| 14-15  | Q&A y checklist                                      | 1 min     |
| **15** | **Finalizar bloque**                                 | **TOTAL** |

---

## 🎯 Objetivo del Instructor

Al terminar este bloque, los asistentes deben:

1. ✅ Identificar anti-patrones de seguridad (secretos hardcodeados, validación deshabilitada)
2. ✅ Conocer Azure AD / Entra ID como solución de autenticación empresarial
3. ✅ Comprender el valor de Azure Key Vault para gestión de secretos
4. ✅ Reconocer la importancia de auditoría y compliance (GDPR)
5. ✅ Visualizar una arquitectura segura de despliegue en Azure

---

## 🧩 Pre-Setup del Instructor

**Antes de comenzar el bloque**:

- [ ] Prepara slides con diagramas (arquitectura de seguridad, flujo de autenticación)
- [ ] Ten ejemplos de código proyectables (anti-patrones vs mejores prácticas)
- [ ] Abre Azure Portal en una pestaña (para mostrar Key Vault, Azure AD si hay tiempo)
- [ ] Ten el diagrama Mermaid de arquitectura visible
- [ ] Prepara ejemplos de queries de Log Analytics
- [ ] Ten el checklist de seguridad para producción impreso/visible

---

## 📋 Guion del Bloque

### Minutos 0-3: Autenticación en Producción (Explicativo)

**Script para decir**:

> "Acaban de implementar autenticación JWT en el Ejercicio 3. Funciona, pero hay problemas serios si lo llevamos a producción así. Vamos a ver qué NO hacer y cómo hacerlo bien."

#### Anti-Patrón 1: Secretos Hardcodeados

**Proyecta el código malo**:

```csharp
private const string SecretKey = "my-secret-key-123";  // ❌ MAL
```

**Explica el problema**:

> "Si suben esto a GitHub, el secreto queda expuesto públicamente. Atacantes usan bots que escanean repositorios buscando secretos. En 2023, GitHub reportó 2 millones de secretos expuestos."

**Muestra la solución**:

> "La solución: Azure Key Vault. Guardas el secreto en Key Vault, tu aplicación lo lee en tiempo de ejecución con credenciales gestionadas. Nunca aparece en el código."

**Proyecta el código bueno**:

```csharp
var keyVaultUrl = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());

var secretKey = builder.Configuration["JwtSecretKey"];  // ✅ Seguro
```

**Validación rápida**:

> "¿Alguien ha usado Key Vault? [Espera respuestas] Si no, después del taller es buen momento para probarlo."

#### Mejora: Usar Azure AD

**Analogía**:

> "Generar tokens JWT manualmente es como ser tu propio banco: posible, pero mejor dejar que el banco (Azure AD) lo haga por ti."

**Ventajas de Azure AD** (enumera):

1. MFA integrado
2. Gestión centralizada de usuarios
3. Integración con políticas empresariales
4. Auditoría automática

**Mensaje clave**:

> "En producción empresarial, usen Azure AD o servicios equivalentes (Auth0, Okta). No reinventen la rueda."

---

### Minutos 3-6: Autorización Avanzada (Explicativo)

**Script para decir**:

> "Autenticación dice 'quién eres', autorización dice 'qué puedes hacer'. Vamos a dos patrones avanzados."

#### Patrón 1: Scopes Jerárquicos

**Explica el problema**:

> "En el Ejercicio 3, si un usuario tiene scope 'admin', debe poder hacer TODO: leer, escribir, configurar. Pero tuvimos que darle explícitamente scopes 'admin', 'write', 'read'. Eso es tedioso."

**Muestra la solución**:

> "Scopes jerárquicos: 'admin' incluye automáticamente 'write' y 'read'."

**Proyecta el código**:

```csharp
private static readonly Dictionary<string, List<string>> Hierarchy = new()
{
    { "admin", new List<string> { "admin", "write", "read" } },
    { "write", new List<string> { "write", "read" } },
    { "read", new List<string> { "read" } }
};
```

> "Con esto, das scope 'admin' a un usuario, y automáticamente tiene 'write' y 'read'. Menos errores, más claro."

#### Patrón 2: Autorización Basada en Recursos

**Escenario**:

> "Un vendedor puede ver solo sus propios clientes, no los de otros vendedores. Pero el gerente puede ver todos."

**Código de ejemplo**:

```csharp
public bool CanAccessResource(AuthenticatedUser user, string customerId)
{
    var customer = _repo.GetById(customerId);
    return customer.OwnerId == user.UserId || user.Scopes.Contains("admin");
}
```

**Explicación**:

> "Verificamos dos cosas: 1) ¿El recurso pertenece al usuario? 2) ¿El usuario es admin? Solo entonces permitimos acceso."

**Pausa para preguntas** (15 segundos):

> "¿Dudas sobre scopes jerárquicos o autorización por recursos?"

---

### Minutos 6-9: Auditoría y Compliance (Casos Prácticos)

**Script para decir**:

> "Seguridad sin auditoría es como tener cámaras de seguridad apagadas. Necesitamos registrar TODO lo importante."

#### Eventos Críticos a Registrar

**Proyecta la tabla**:

| Evento                | Severidad | Por qué es crítico                      |
| --------------------- | --------- | --------------------------------------- |
| Autenticación fallida | Warning   | Posible ataque de fuerza bruta          |
| Acceso denegado       | Warning   | Usuario intentando acceder sin permisos |
| Token expirado        | Info      | Normal, pero útil para métricas         |
| Rate limit excedido   | Warning   | Posible abuso o DoS                     |

**Ejemplo de log estructurado**:

```csharp
_logger.LogWarning("Authentication failed for user {Username} from {IpAddress}: {Reason}",
    username, ipAddress, reason);
```

**Ventaja de logs estructurados**:

> "Con logs estructurados, pueden consultar en Log Analytics: 'Muéstrame todos los usuarios que fallaron autenticación más de 5 veces en la última hora'. Con logs de texto plano, eso es casi imposible."

#### GDPR / LOPD (Compliance)

**Pregunta al salón**:

> "¿Quién trabaja con datos personales de clientes europeos? [Espera manos levantadas] Entonces necesitan cumplir GDPR."

**3 Derechos clave del GDPR**:

1. **Derecho al olvido**: El usuario puede pedir que borres todos sus datos.

```csharp
app.MapDelete("/api/users/{userId}/data", async (string userId) =>
{
    await _userDataService.DeleteAllUserData(userId);
    return Results.Ok();
});
```

2. **Portabilidad de datos**: El usuario puede exportar sus datos.

```csharp
app.MapGet("/api/users/{userId}/export", async (string userId) =>
{
    var data = await _userDataService.GetAllUserData(userId);
    return Results.File(JsonSerializer.SerializeToUtf8Bytes(data), "application/json");
});
```

3. **Consentimiento**: Registrar que el usuario dio permiso para procesar sus datos.

**Mensaje clave**:

> "GDPR no es opcional si operan en Europa. Planifiquen esto desde el diseño, no como parche después."

---

### Minutos 9-12: Despliegue Seguro en Azure (Arquitectura)

**Script para decir**:

> "Ahora la infraestructura. Vamos a ver una arquitectura de despliegue seguro en Azure."

#### Diagrama de Arquitectura (proyectar el Mermaid)

**Explica cada componente**:

1. **Azure API Management** (apunta al diagrama):

   > "Primera línea de defensa. Rate limiting global, políticas de seguridad, transformación de requests. Si llegan 10,000 solicitudes/segundo, APIM filtra antes de llegar a tu servidor."

2. **Application Gateway + WAF**:

   > "Web Application Firewall. Bloquea ataques comunes: SQL injection, XSS, DDoS. WAF tiene reglas actualizadas automáticamente por Microsoft."

3. **Azure Container Apps**:

   > "Donde corre tu servidor MCP. Escala automáticamente de 0 a 30 instancias según demanda. Pagas solo por lo que usas."

4. **Azure Key Vault**:

   > "Ya lo vimos. Secretos, certificados, claves de cifrado. Todo centralizado."

5. **Azure AD**:

   > "Autenticación centralizada. Integración con MFA, políticas de acceso condicional."

6. **Application Insights**:

   > "Monitoreo en tiempo real. Logs, métricas, trazas distribuidas. Veremos esto en el siguiente minuto."

**Enfatiza**:

> "Esta arquitectura no es overkill para producción, es lo MÍNIMO para despliegue empresarial seguro."

#### HTTPS Obligatorio

**Mensaje corto**:

> "En producción: HTTPS siempre. Azure Container Apps da certificados SSL gratis para \*.azurecontainerapps.io. Si tienes dominio propio, sube el certificado a Key Vault y lo vinculas. Cero excusas para no usar HTTPS."

---

### Minutos 12-14: Monitoreo y Alertas (Demos Rápidas)

**Script para decir**:

> "Seguridad reactiva: detectar y responder rápido cuando algo malo pasa."

#### Alerta 1: Intentos de Autenticación Fallidos

**Proyecta la query de Log Analytics**:

```kusto
traces
| where timestamp > ago(5m)
| where message contains "Authentication failed"
| summarize FailureCount = count() by tostring(customDimensions.Username)
| where FailureCount > 10
```

**Explica**:

> "Esta query busca usuarios con más de 10 intentos fallidos en 5 minutos. Si dispara, envía email al equipo de seguridad y puede bloquear la IP automáticamente."

#### Alerta 2: Rate Limit Excedido

**Escenario**:

> "Si un usuario excede rate limit constantemente, puede ser:
>
> 1. Un bot malicioso
> 2. Una integración mal configurada
> 3. Un cliente legítimo que necesita tier premium
>
> La alerta permite investigar y tomar acción."

**Métricas de Seguridad** (proyecta la tabla):

| Métrica                       | Objetivo | Acción                         |
| ----------------------------- | -------- | ------------------------------ |
| Tasa de autenticación fallida | < 5%     | Investigar brute force         |
| Accesos denegados             | < 10%    | Revisar asignación de permisos |

**Mensaje**:

> "Métricas no son solo para performance. Son vitales para seguridad."

---

### Minutos 14-15: Checklist y Q&A (Cierre)

**Script para decir**:

> "Antes de cerrar, vamos a repasar el checklist de seguridad para producción."

#### Checklist Rápido (lee los puntos clave)

**Proyecta el checklist**:

✅ Autenticación:

- Azure AD / Entra ID
- Tokens con expiración ≤ 30 min
- Secretos en Key Vault

✅ Autorización:

- Scopes de mínimo privilegio
- Validación en cada endpoint

✅ Auditoría:

- Logs estructurados a Application Insights
- Retención ≥ 90 días
- Alertas configuradas

✅ Infraestructura:

- HTTPS obligatorio
- WAF habilitado
- Network isolation con VNET

**Pregunta final**:

> "¿Alguna duda sobre seguridad antes de pasar al Ejercicio 4?"

[Espera 20 segundos para preguntas]

**Si no hay preguntas**:

> "Perfecto. Ahora saben cómo hacer MCP seguro en producción. En el Ejercicio 4 van a aplicar esto en un escenario complejo: orquestar 3 servidores MCP con seguridad centralizada."

---

## 🚨 Contingencias

### Contingencia A: Tiempo insuficiente (Minuto 13+)

**Problema**: Llevas 13 minutos y no has terminado monitoreo.

**Acción**:

1. **Omitir**: Detalles de queries de Log Analytics.
2. **Mantener**: Diagrama de arquitectura, concepto de alertas.
3. **Mensaje rápido**:

> "El documento tiene queries completas de Log Analytics. Úsenlas como base para sus proyectos."

**Terminas en minuto 15.**

---

### Contingencia B: Preguntas profundas sobre GDPR

**Pregunta típica**: "¿Qué pasa si no cumplimos GDPR?"

**Respuesta concisa**:

> "Multas de hasta 20 millones de euros o 4% de la facturación anual global, lo que sea mayor. Casos reales: Amazon multado con 746 millones en 2021. No es broma. Consulten con su equipo legal para implementación completa."

**Redirige**:

> "Es un tema amplio. Después del taller puedo compartir recursos específicos de GDPR para MCP."

---

### Contingencia C: Audiencia pregunta por costos de Azure

**Pregunta**: "¿Cuánto cuesta esta arquitectura al mes?"

**Respuesta estimada**:

> "Depende del tráfico, pero estimación para startup/SME:
>
> - Container Apps: ~50€/mes (escalado bajo)
> - Key Vault: ~5€/mes (operaciones básicas)
> - Application Insights: ~20€/mes (100GB logs)
> - API Management: Desde 0€ (tier Consumption)
> - Total: ~75-150€/mes para tráfico moderado
>
> Escala conforme crece tu uso. Pueden usar Azure Calculator para estimaciones precisas."

---

### Contingencia D: Confusión entre WAF y APIM

**Pregunta**: "¿WAF y API Management no hacen lo mismo?"

**Clarificación**:

> "Diferentes capas:
>
> - **WAF**: Seguridad (bloquea ataques SQL injection, XSS, DDoS)
> - **APIM**: Gestión de APIs (rate limiting, transformaciones, analytics)
>
> Analogía: WAF es el guardia de seguridad que verifica que no entren armas. APIM es la recepción que dirige visitantes a la sala correcta. Ambos son necesarios."

---

## ✅ Validación de Completitud

Al terminar el bloque, observa:

✅ **Comprensión conceptual**:

- Asistentes identifican anti-patrones de seguridad
- Entienden la diferencia entre autenticación y autorización
- Reconocen componentes de arquitectura segura

✅ **Engagement**:

- Preguntas sobre implementación práctica
- Interés en compliance (GDPR/LOPD)

❌ **Señales de alarma**:

- Confusión entre Azure AD y Azure Key Vault (repite diferencias)
- Preguntas sobre código del Ejercicio 3 (viene en el siguiente bloque)

---

## 📊 Métricas de Éxito

| Indicador                                             | Objetivo | Resultado Real |
| ----------------------------------------------------- | -------- | -------------- |
| Asistentes que entienden Azure AD vs JWT manual       | >85%     | \_\_\_ %       |
| Asistentes que reconocen importancia de Key Vault     | >90%     | \_\_\_ %       |
| Asistentes que identifican eventos críticos a auditar | >75%     | \_\_\_ %       |
| Tiempo total utilizado                                | 15 min   | \_\_\_ min     |

---

## 🔗 Transición al Ejercicio 4 (Bloque 8)

**Script de cierre** (30 segundos):

> "Perfecto. Ya saben cómo asegurar servidores MCP en producción. Ahora viene el desafío más complejo del taller: el Ejercicio 4. Van a crear un 'Orquestador' que coordina 3 servidores MCP diferentes (SQL, Cosmos, REST API) para responder preguntas en español sobre datos de negocio. Es un ejercicio de grupo, 25 minutos, y es el que más se parece a un escenario real de empresa. Tomen un descanso de 3 minutos, formen equipos de 3-5 personas."

**Checklist de transición**:

- [ ] Los asistentes hacen un descanso (3 min)
- [ ] Forman equipos de 3-5 personas
- [ ] Abre el documento del Ejercicio 4 en pantalla
- [ ] Prepara el contrato `exercise-4-virtual-analyst.json` para proyectar

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
