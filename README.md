# Taller MCP: Model Context Protocol en Azure

Taller práctico de 3 horas para aprender a construir servidores MCP que explotan datos desde diversas fuentes utilizando C# .NET 10.0 y Azure.

## 🎯 Descripción

Este taller te guía a través de la construcción de servidores **Model Context Protocol (MCP)** para integrar y explotar datos desde múltiples fuentes (Azure SQL, Cosmos DB, REST APIs). Aprenderás desde conceptos fundamentales hasta patrones empresariales de orquestación multi-fuente.

### ¿Qué es MCP?

Model Context Protocol es un protocolo estándar para exponer datos y capacidades a modelos de IA de manera estructurada, componible y segura.

## 📚 Contenido del Taller

**Duración**: 3 horas  
**Formato**: 11 bloques con teoría, demostraciones y ejercicios prácticos

### Bloques

1. **Apertura** (10 min) - Introducción y contexto
2. **Fundamentos** (25 min) - Conceptos MCP, arquitectura, casos de uso
3. **Anatomía de un Proveedor + Ejercicio 1** (30 min) - Live coding y recursos estáticos
4. **Ejercicio 2** (20 min) - Consultas paramétricas
5. **Ejercicio 3** (20 min) - Seguridad JWT y rate limiting
6. **Seguridad y Gobernanza** (15 min) - Patrones empresariales
7. **Ejercicio 4** (25 min) - Reto integrador: Analista virtual
8. **Orquestación Multi-Fuente** (15 min) - Patrones de integración
9. **Ejercicio 5** (30 min) - Agente de IA con Microsoft Agent Framework
10. **Hoja de Ruta y Casos B2B** (10 min) - Escenarios de negocio
11. **Cierre** (10 min) - Retrospectiva y siguientes pasos

## 🚀 Quick Start

### Prerequisitos

- **SDK**: .NET 10.0 o superior
- **IDE**: Visual Studio 2022 o VS Code con C# Dev Kit
- **Tools**: Git, PowerShell 7+
- **Azure** (opcional): Cuenta de Azure para ejercicios cloud

### Instalación

```powershell
# 1. Clonar el repositorio
git clone <repository-url>
cd mcp-workshop

# 2. Verificar entorno
.\scripts\verify-setup.ps1

# 3. Generar datos de ejemplo
.\scripts\create-sample-data.ps1

# 4. Construir solución
dotnet build McpWorkshop.sln
```

> **Nota**: Los datos de ejemplo se generan ejecutando el script `create-sample-data.ps1`, que crea archivos JSON dinámicos en la carpeta `data/` del repositorio (customers.json, products.json, orders.json, sessions.json, abandoned-carts.json, cart-events.json).

## 📖 Documentación

- **[Agenda Completa](docs/AGENDA.md)** - Cronograma detallado del taller
- **[Quick Reference](docs/QUICK_REFERENCE.md)** - Referencia rápida MCP y C#
- **[Instructor Handbook](docs/INSTRUCTOR_HANDBOOK.md)** - Guía de facilitación
- **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Solución de problemas comunes
- **[Azure Deployment](docs/AZURE_DEPLOYMENT.md)** - Despliegue en Azure

### Módulos por Bloque

- [01 - Apertura](docs/modules/01b-apertura.md)
- [02 - Fundamentos](docs/modules/02b-fundamentos.md)
- [03 - Ejercicio 1: Anatomía de un Proveedor MCP](docs/modules/03b-ejercicio-1-anatomia-proveedor.md)
- [04 - Ejercicio 2: Consultas Paramétricas](docs/modules/04b-ejercicio-2-consultas-parametricas.md)
- [05 - Ejercicio 3: Seguridad](docs/modules/05b-ejercicio-3-seguridad.md)
- [06 - Seguridad y Gobernanza](docs/modules/06b-seguridad-gobernanza.md)
- [07 - Ejercicio 4: Orquestador](docs/modules/07b-ejercicio-4-orquestador.md)
- [08 - Orquestación Multi-Fuente](docs/modules/08-orquestacion-multifuente.md)
- [09 - Ejercicio 5: Agente con Microsoft Agent Framework](docs/modules/09b-ejercicio-5-agente-maf.md)
- [10 - Roadmap & Casos B2B](docs/modules/10-roadmap-casos-b2b.md)
- [11 - Cierre](docs/modules/11-cierre.md)

## 🏗️ Estructura del Proyecto

```text
mcp-workshop/
│
├── docs/                          # Documentación del taller (30 archivos)
│   ├── modules/                  # 24 módulos educativos (teoría + ejercicios + instructor)
│   │   ├── 01a-apertura-instructor.md
│   │   ├── 01b-apertura.md
│   │   ├── 02a-fundamentos-instructor.md
│   │   ├── 02b-fundamentos.md
│   │   ├── 03a-ejercicio-1-instructor.md
│   │   ├── 03b-ejercicio-1-anatomia-proveedor.md
│   │   ├── 03b-ejercicio-1-anatomia-proveedor.http
│   │   ├── 04a-ejercicio-2-instructor.md
│   │   ├── 04b-ejercicio-2-consultas-parametricas.md
│   │   ├── 04b-ejercicio-2-consultas-parametricas.http
│   │   ├── 05a-ejercicio-3-instructor.md
│   │   ├── 05b-ejercicio-3-seguridad.md
│   │   ├── 05b-ejercicio-3-seguridad.http
│   │   ├── 06-seguridad-gobernanza-antipatterns.md
│   │   ├── 06a-seguridad-gobernanza-instructor.md
│   │   ├── 06b-seguridad-gobernanza.md
│   │   ├── 07a-ejercicio-4-instructor.md
│   │   ├── 07b-ejercicio-4-orquestador.md
│   │   ├── 07b-ejercicio-4-orquestador.http
│   │   ├── 08-orquestacion-multifuente.md
│   │   ├── 09a-ejercicio-5-instructor.md
│   │   ├── 09b-ejercicio-5-agente-maf.md
│   │   ├── 10-roadmap-casos-b2b.md
│   │   └── 11-cierre.md
│   ├── AGENDA.md                 # Cronograma detallado 180 minutos
│   ├── CHECKLIST.md              # Lista de verificación del workshop
│   ├── INSTRUCTOR_HANDBOOK.md    # Guía para instructores
│   ├── QUICK_REFERENCE.md        # Cheat sheet de MCP y C#
│   ├── TROUBLESHOOTING.md        # Solución de problemas
│   └── README.md                 # Documentación principal
│
├── src/                           # Código fuente (50 archivos .cs)
│   ├── McpWorkshop.Servers/
│   │   ├── CosmosMcpServer/      # Servidor MCP para Azure Cosmos DB
│   │   │   ├── Models/
│   │   │   ├── Tools/
│   │   │   ├── Program.cs
│   │   │   └── CosmosMcpServer.csproj
│   │   ├── RestApiMcpServer/     # Servidor MCP para REST APIs
│   │   │   ├── Tools/
│   │   │   ├── Program.cs
│   │   │   └── RestApiMcpServer.csproj
│   │   └── SqlMcpServer/         # Servidor MCP para SQL Server
│   │       ├── Models/
│   │       ├── Tools/
│   │       ├── Program.cs
│   │       └── SqlMcpServer.csproj
│   └── McpWorkshop.Shared/       # Librería compartida
│       ├── Configuration/
│       ├── Logging/
│       ├── Mcp/
│       ├── Monitoring/
│       ├── Security/
│       └── McpWorkshop.Shared.csproj
│
├── tests/                         # Suite de pruebas (13 archivos .cs)
│   └── McpWorkshop.Tests/
│       ├── CosmosMcpServerToolsTests.cs
│       ├── DataModelsTests.cs
│       ├── InputSanitizerTests.cs
│       ├── JsonRpcComplianceTests.cs
│       ├── McpTestClient.cs
│       ├── PerformanceTrackerTests.cs
│       ├── SecurityHeadersMiddlewareTests.cs
│       ├── SqlMcpServerToolsTests.cs
│       ├── StructuredLoggerTests.cs
│       ├── WorkshopSettingsTests.cs
│       ├── README.md
│       └── McpWorkshop.Tests.csproj
│
├── data/                          # Datos de ejemplo JSON
│   ├── abandoned-carts.json
│   ├── cart-events.json
│   ├── customers.json
│   ├── orders.json
│   ├── products.json
│   └── sessions.json
│
├── scripts/                       # Scripts de automatización PowerShell
│   ├── create-sample-data.ps1    # Generar o actualizar datos de ejemplo
│   ├── verify-setup.ps1          # Verificación de prerrequisitos
│   ├── verify-exercise1.ps1      # Validación Ejercicio 1
│   ├── verify-exercise2.ps1      # Validación Ejercicio 2
│   ├── verify-exercise3.ps1      # Validación Ejercicio 3
│   ├── verify-exercise4.ps1      # Validación Ejercicio 4
│   ├── verify-exercise5.ps1      # Validación Ejercicio 5
│   └── run-all-tests.ps1         # Ejecutar suite completa de tests
│
│
├── .editorconfig                  # Configuración de editor
├── .gitignore                     # Exclusiones de Git
├── LICENSE                        # Licencia MIT
├── QUICKSTART.md                  # Guía de inicio rápido
├── README.md                      # Este archivo
└── McpWorkshop.sln               # Solución .NET
```

### Componentes Clave

**Servidores MCP (4 implementaciones)**:

- 3 servidores de datos (SQL, Cosmos, REST API)
- 1 agente en desarrollo (Exercise5Agent)

**Documentación (30 archivos)**:

- 24 módulos educativos (teoría + ejercicios + instructor + archivos .http)
- 6 guías del workshop (AGENDA, CHECKLIST, INSTRUCTOR_HANDBOOK, QUICK_REFERENCE, TROUBLESHOOTING, README)

**Tests (13 archivos de prueba)**:

- Tests unitarios y de integración
- Validación de protocolo JSON-RPC
- Tests de seguridad y rendimiento
- Cliente de prueba MCP (McpTestClient)

## 🎓 Ejercicios Prácticos

### Ejercicio 1: Recursos Estáticos (15 min)

**Objetivo**: Crear un servidor MCP que expone listas de clientes y productos como recursos estáticos.

**Conceptos clave**:

- Implementación de `resources/list` para descubrimiento
- Implementación de `resources/read` para acceso a datos
- Estructura de recursos MCP (URI, nombre, descripción)
- Serialización JSON de datos estáticos

**Servidor**: `Exercise1StaticResources` (Puerto 5000)

**Verificación**:

```powershell
.\scripts\verify-exercise1.ps1
```

**[📄 Guía completa →](docs/modules/03b-ejercicio-1-anatomia-proveedor.md)** _(Fusionado con demostración en vivo)_

---

### Ejercicio 2: Consultas Paramétricas (20 min)

**Objetivo**: Implementar herramientas MCP con parámetros para búsquedas y filtros dinámicos.

**Conceptos clave**:

- Implementación de `tools/list` para exponer capacidades
- Implementación de `tools/call` para ejecutar herramientas
- Esquemas de validación de parámetros (JSON Schema)
- Herramientas: `GetCustomers`, `SearchOrders`, `CalculateTotal`
- Paginación y filtros opcionales

**Servidor**: `Exercise2ParametricQuery` (Puerto 5001)

**Herramientas implementadas**:

1. **GetCustomers**: Filtrar clientes por país, ciudad, límite
2. **SearchOrders**: Buscar órdenes por cliente, fechas, estado
3. **CalculateTotal**: Calcular totales con aplicación de descuentos

**Verificación**:

```powershell
.\scripts\verify-exercise2.ps1
```

**[📄 Guía completa →](docs/modules/04b-ejercicio-2-consultas-parametricas.md)**

---

### Ejercicio 3: Servidor Seguro (20 min)

**Objetivo**: Agregar autenticación JWT, autorización por scopes, rate limiting y logging estructurado.

**Conceptos clave**:

- Autenticación con tokens JWT (JSON Web Tokens)
- Autorización basada en scopes (`read`, `write`, `admin`)
- Rate limiting por tier de usuario (Base: 10 req/min, Premium: 50 req/min)
- Middleware de seguridad en ASP.NET Core
- Logging estructurado de eventos de seguridad
- Respuestas HTTP 401 (Unauthorized) y 403 (Forbidden)

**Servidor**: `Exercise3SecureServer` (Puerto 5002)

**Scopes disponibles**:

- `read`: Solo lectura de recursos
- `write`: Lectura y modificación
- `admin`: Acceso completo incluyendo configuración

**Verificación**:

```powershell
.\scripts\verify-exercise3.ps1
```

**[📄 Guía completa →](docs/modules/05b-ejercicio-3-seguridad.md)**

---

### Ejercicio 4: Orquestador (25 min)

**Objetivo**: Construir un orquestador MCP que coordina múltiples servidores para responder preguntas de negocio en español.

**Conceptos clave**:

- Arquitectura multi-servidor (3 servidores MCP independientes)
- Patrones de orquestación: paralelo, secuencial, fan-out
- Parser de lenguaje natural (español) para routing de consultas
- Caching con TTL para optimización
- Manejo de errores y fallbacks
- Síntesis de resultados de múltiples fuentes

**Arquitectura**:

```text
Usuario (español) → Orquestador → [SQL Server | Cosmos DB | REST API]
                         ↓
                    Cache (5 min TTL)
                         ↓
                   Respuesta sintetizada
```

**Servidores MCP implementados**:

1. **SqlMcpServer** (Puerto 5009): Datos transaccionales (clientes, órdenes)
2. **CosmosMcpServer** (Puerto 5010): Comportamiento de usuarios (sesiones, carritos)
3. **RestApiMcpServer** (Puerto 5011): APIs externas (inventario, envíos)
4. **Exercise4VirtualAnalyst** (Puerto 5012): Orquestador principal

**Preguntas de ejemplo**:

- "¿Cuántos clientes nuevos registrados en Madrid este mes?"
- "¿Qué usuarios abandonaron carritos en las últimas 24 horas?"
- "¿Cuál es el estado del pedido #1234 y su inventario asociado?"
- "Dame un resumen de ventas de la semana más productos más vendidos"

**Verificación**:

```powershell
.\scripts\verify-exercise4.ps1
```

**[📄 Guía completa →](docs/modules/07b-ejercicio-4-orquestador.md)**

### Ejercicio 5: Agente de Inteligencia Artificial con Microsoft Agent Framework (30 min)

**Objetivo**: Crear un agente conversacional inteligente que integra los MCP servers creados en ejercicios anteriores.

**Conceptos clave**:

- Integración de múltiples servidores MCP (SQL, Cosmos, REST API)
- Descubrimiento automático de herramientas (`ListToolsAsync()`)
- Microsoft Agent Framework (MAF) para agentes conversacionales
- Comprensión de lenguaje natural en español
- Mantenimiento de contexto conversacional (multi-turno)

**Servidor**: `Exercise5AgentServer`

**Verificación**:

```powershell
./scripts/verify-exercise5.ps1
```

**[📄 Guía completa →](docs/modules/09b-ejercicio-5-agente-maf.md)**

## 🛠️ Tecnologías

### Stack Principal

- **Lenguaje**: C# .NET 10.0
- **Framework Web**: ASP.NET Core Minimal APIs
- **MCP Library**: ModelContextProtocol (NuGet prerelease)
- **Autenticación**: System.IdentityModel.Tokens.Jwt
- **Serialización**: System.Text.Json

### Azure Services (Opcionales - Ejercicio 4)

- **Hosting**: Azure Container Apps, Azure App Service
- **Datos**: Azure SQL Database, Azure Cosmos DB
- **Storage**: Azure Blob Storage
- **Monitoring**: Azure Log Analytics, Application Insights

### Infraestructura y Testing

- **Testing**: xUnit 3.1+, Microsoft.AspNetCore.Mvc.Testing
- **Scripting**: PowerShell 7+

### Puertos Utilizados

| Ejercicio                 | Puerto | Servidor         |
| ------------------------- | ------ | ---------------- |
| Ejercicio 1               | 5001   | Exercise1Server  |
| Ejercicio 2               | 5002   | Exercise2Server  |
| Ejercicio 3               | 5003   | Exercise3Server  |
| Ejercicio 4 - SQL         | 5010   | SqlMcpServer     |
| Ejercicio 4 - Cosmos      | 5011   | CosmosMcpServer  |
| Ejercicio 4 - REST        | 5012   | RestApiMcpServer |
| Ejercicio 4 - Orquestador | 5004   | Exercise4Server  |
| Ejercicio 5 - Agente      | N/A    | Exercise5Agent   |

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## 🌟 Créditos

Desarrollado como parte del MCP Workshop Madrid.

Este taller fue construido utilizando [GitHub Spec-Kit](https://github.com/github/spec-kit) - un framework de GitHub para desarrollo guiado por especificaciones.

---

## **¡Disfruta del taller y construye servidores MCP increíbles! 🚀**
