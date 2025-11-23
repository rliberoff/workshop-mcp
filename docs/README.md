1.14.0# MCP Workshop - Taller Práctico de Model Context Protocol

![Workshop Banner](https://img.shields.io/badge/Workshop-MCP-blue) ![.NET](https://img.shields.io/badge/.NET-10.0-purple) ![Duration](https://img.shields.io/badge/Duration-3h-green) ![Level](https://img.shields.io/badge/Level-Intermediate-yellow)

Taller práctico de 3 horas para aprender a construir servidores Model Context Protocol (MCP) con .NET 10, desde recursos estáticos hasta orquestación multi-fuente con integración empresarial.

## 📋 Índice

-   [Sobre el Workshop](#sobre-el-workshop)
-   [Objetivos de Aprendizaje](#objetivos-de-aprendizaje)
-   [Prerequisitos](#prerequisitos)
-   [Estructura del Workshop](#estructura-del-workshop)
-   [Ejercicios Prácticos](#ejercicios-prácticos)
-   [Materiales](#materiales)
-   [Guía de Inicio Rápido](#guía-de-inicio-rápido)
-   [Recursos Adicionales](#recursos-adicionales)

---

## 🎯 Sobre el Workshop

El **Model Context Protocol (MCP)** es un protocolo abierto que permite a las aplicaciones de IA (como Claude, ChatGPT, o agentes personalizados) conectarse de forma estandarizada con diversas fuentes de datos y herramientas empresariales. Este taller te enseña a construir servidores MCP robustos y listos para producción.

### ¿Por qué MCP?

-   **Estandarización**: Un protocolo común para todas las integraciones de IA
-   **Seguridad**: Control granular de acceso, autenticación JWT, rate limiting
-   **Escalabilidad**: Arquitectura modular que permite orquestar múltiples fuentes
-   **Interoperabilidad**: Compatible con cualquier cliente MCP

### ¿Qué construirás?

Al final del taller habrás creado:

1. ✅ Servidor MCP con recursos estáticos (clientes, productos)
2. ✅ Servidor con herramientas parametrizables (búsquedas, filtros, agregaciones)
3. ✅ Servidor seguro con autenticación JWT y rate limiting
4. ✅ Orquestador que coordina múltiples servidores MCP (SQL, Cosmos, REST)

---

## 🎓 Objetivos de Aprendizaje

Al completar este workshop serás capaz de:

### Conocimientos Fundamentales

-   [ ] Explicar qué es MCP y sus diferencias con APIs REST tradicionales
-   [ ] Describir la arquitectura cliente-servidor de MCP
-   [ ] Identificar casos de uso apropiados para MCP vs otras tecnologías

### Habilidades Técnicas

-   [ ] Implementar servidores MCP con recursos estáticos usando JSON-RPC 2.0
-   [ ] Crear herramientas parametrizables con validación de esquemas
-   [ ] Aplicar autenticación JWT y autorización basada en scopes
-   [ ] Configurar rate limiting y logging estructurado
-   [ ] Orquestar múltiples servidores MCP con patrones de integración

### Aplicación Empresarial

-   [ ] Diseñar arquitecturas MCP para escenarios B2B reales
-   [ ] Evaluar trade-offs de seguridad y rendimiento
-   [ ] Calcular ROI de adopción de MCP en tu organización

---

## 📚 Prerequisitos

### Conocimientos Requeridos

-   **C# Intermedio**: Clases, interfaces, async/await, LINQ
-   **ASP.NET Core**: Conceptos básicos de middleware y endpoints
-   **JSON**: Lectura y manipulación de estructuras JSON
-   **PowerShell**: Ejecución de scripts básicos

### Software Necesario

| Software               | Versión | Propósito                     |
| ---------------------- | ------- | ----------------------------- |
| **.NET SDK**           | 10.0+   | Runtime y compilación         |
| **Visual Studio Code** | Latest  | Editor recomendado            |
| **PowerShell**         | 7.0+    | Scripts de verificación       |
| **Git**                | 2.0+    | Control de versiones          |
| **Terraform**          | 1.14.0+ | Despliegue de infraestructura |
| **Postman/Insomnia**   | Latest  | Pruebas de API (opcional)     |

### Instalación Rápida

```powershell
# Verificar versiones
dotnet --version  # Debe ser 10.0.x
pwsh --version    # Debe ser 7.x

# Clonar repositorio
git clone https://github.com/yourusername/mcp-workshop.git
cd mcp-workshop

# Restaurar dependencias
dotnet restore

# Verificar entorno
.\scripts\verify-setup.ps1
```

Ver [Guía de Inicio Rápido](./quickstart.md) para instrucciones detalladas.

---

## 🗓️ Estructura del Workshop

### Parte 1: Fundamentos (1h 10min)

#### Bloque 1: Apertura (10 min)

-   Bienvenida y presentaciones
-   Contexto del workshop
-   Configuración del entorno

#### Bloque 2: Fundamentos MCP (25 min)

-   ¿Qué es Model Context Protocol?
-   Arquitectura cliente-servidor
-   MCP vs Plugins vs APIs REST
-   Casos de uso empresariales

📚 [Documentación](./modules/02b-fundamentos.md) | 👨‍🏫 [Notas del Instructor](./modules/02a-fundamentos-instructor.md)

#### Bloque 3: Anatomía de un Proveedor (20 min)

-   Live coding: Primer servidor MCP
-   Manifest de servidor
-   Recursos vs Tools
-   JSON-RPC 2.0 en acción

📚 [Guía de Live Coding](./modules/03b-anatomia-proveedor.md) | 💻 [Código de Referencia](../src/McpWorkshop.Servers/DemoServer/)

#### Bloque 4: Ejercicio 1 - Recursos Estáticos (15 min)

-   **Objetivo**: Exponer clientes y productos como recursos MCP
-   **Duración**: 15 minutos guiados
-   **Skills**: `resources/list`, `resources/read`, JSON estructurado

📚 [Instrucciones](./modules/03b-anatomia-proveedor.md) | ✅ [Solución](../src/McpWorkshop.Servers/Exercise1StaticResources/)

### Parte 2: Herramientas y Seguridad (1h 10min)

#### Bloque 5: Ejercicio 2 - Consultas Parametrizadas (20 min)

-   **Objetivo**: Crear herramientas con parámetros (búsqueda, filtros, agregaciones)
-   **Duración**: 20 minutos independiente
-   **Skills**: `tools/list`, `tools/call`, JSON Schema validation

📚 [Instrucciones](./modules/04b-ejercicio-2-consultas-parametricas.md) | ✅ [Solución](../src/McpWorkshop.Servers/Exercise2ParametricQuery/)

#### Bloque 6: Ejercicio 3 - Seguridad (20 min)

-   **Objetivo**: Implementar autenticación JWT, autorización, rate limiting
-   **Duración**: 20 minutos
-   **Skills**: Bearer tokens, scopes, logging estructurado

📚 [Instrucciones](./modules/05b-ejercicio-3-seguridad.md) | ✅ [Solución](../src/McpWorkshop.Servers/Exercise3SecureServer/)

#### Bloque 7: Seguridad & Gobernanza (15 min)

-   Sesión sobre seguridad empresarial
-   Autenticación vs Autorización
-   Rate limiting strategies
-   Logging y auditoría
-   Anti-patterns de seguridad

📚 [Presentación](./modules/06b-seguridad-gobernanza.md) | ⚠️ [Anti-patterns](./modules/06-seguridad-gobernanza-antipatterns.md)

#### Bloque 8: Ejercicio 4 - Analista Virtual (25 min)

-   **Objetivo**: Orquestar SQL MCP + Cosmos MCP + REST MCP
-   **Duración**: 25 minutos en grupos de 3-5 personas
-   **Skills**: Multi-source orchestration, caching, natural language queries

📚 [Instrucciones](./modules/07-ejercicio-4-analista-virtual.md) | ✅ [Solución](../src/McpWorkshop.Servers/Exercise4VirtualAnalyst/)

### Parte 3: Arquitectura y Casos de Negocio (40 min)

#### Bloque 9: Orquestación Multi-Fuente (15 min)

-   Patrones de integración (parallel, sequential, fanOut)
-   Circuit breakers y retry policies
-   Distributed tracing
-   Caching strategies

📚 [Patrones Avanzados](./modules/08-orquestacion-multifuente.md)

#### Bloque 10: Roadmap & Casos B2B (10 min)

-   7 casos de uso empresariales reales
-   ROI calculators
-   Decision matrix: ¿Cuándo usar MCP?
-   Comparativa de costos (MCP vs APIs vs Database directo)

📚 [Casos de Negocio](./modules/09-roadmap-casos-b2b.md)

#### Bloque 11: Cierre (10 min)

-   Retrospectiva 3-2-1
-   Q&A
-   Próximos pasos
-   Feedback

📚 [Guía de Cierre](./modules/10-cierre.md)

---

## 💻 Ejercicios Prácticos

| Ejercicio                        | Duración | Formato       | Complejidad      | Skills                              |
| -------------------------------- | -------- | ------------- | ---------------- | ----------------------------------- |
| **Exercise 1: Static Resources** | 15 min   | Guiado        | ⭐ Básico        | `resources/list`, `resources/read`  |
| **Exercise 2: Parametric Tools** | 20 min   | Independiente | ⭐⭐ Intermedio  | `tools/list`, `tools/call`, schemas |
| **Exercise 3: Security**         | 20 min   | Semi-guiado   | ⭐⭐⭐ Avanzado  | JWT, scopes, rate limiting, logging |
| **Exercise 4: Virtual Analyst**  | 25 min   | Grupos 3-5    | ⭐⭐⭐⭐ Experto | Orchestration, caching, NLP         |

### Criterios de Éxito

-   ✅ **Exercise 1**: 80% de asistentes completan en 15 min
-   ✅ **Exercise 2**: 70% de asistentes completan en 20 min
-   ✅ **Exercise 3**: 60% implementan seguridad correctamente
-   ✅ **Exercise 4**: 90% de grupos demuestran orquestación funcional

---

## 📦 Materiales

### Para Instructores

-   📖 [Instructor Handbook](./INSTRUCTOR_HANDBOOK.md) - Facilitation tips, timing, troubleshooting
-   📋 [Workshop Checklist](./CHECKLIST.md) - Pre-session validation
-   🎯 [Agenda Maestra](./AGENDA.md) - Timing detallado, transiciones, contingencias
-   🧪 [Notas de Cada Bloque](./modules/) - Talking points, engagement strategies

### Para Asistentes

-   🚀 [Quick Start Guide](./quickstart.md) - Instalación y configuración
-   📚 [Quick Reference](./QUICK_REFERENCE.md) - MCP protocol cheat sheet, code snippets
-   🔧 [Troubleshooting Guide](./TROUBLESHOOTING.md) - Solución de problemas comunes

### Recursos Técnicos

-   🧪 [Tests](../tests/McpWorkshop.Tests/) - 96 tests automatizados
-   ☁️ [Terraform Modules](../infrastructure/terraform/) - Despliegue en Azure
-   📊 [Sample Data](../data/) - Datos de ejemplo incluidos en el repositorio

---

## 🚀 Guía de Inicio Rápido

### 1. Verificación del Entorno

```powershell
# Ejecutar script de verificación
cd mcp-workshop
.\scripts\verify-setup.ps1

# Salida esperada:
# ✓ .NET SDK 10.0.x detectado
# ✓ PowerShell 7.x detectado
# ✓ Puertos 5000-5004 disponibles
# ✓ Todos los paquetes NuGet restaurados
```

### 2. Ejecutar el Primer Ejemplo

```powershell
# Compilar servidor de demostración
cd src/McpWorkshop.Servers/DemoServer
dotnet run

# En otra terminal, probar con PowerShell
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        clientInfo = @{ name = "test-client"; version = "1.0.0" }
    }
    id = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"
```

### 3. Explorar los Ejercicios

```powershell
# Exercise 1: Static Resources
cd src/McpWorkshop.Servers/Exercise1StaticResources
dotnet run
.\scripts\verify-exercise1.ps1

# Exercise 2: Parametric Query
cd src/McpWorkshop.Servers/Exercise2ParametricQuery
dotnet run
.\scripts\verify-exercise2.ps1

# Exercise 3: Secure Server
cd src/McpWorkshop.Servers/Exercise3SecureServer
dotnet run
.\scripts\verify-exercise3.ps1

# Exercise 4: Virtual Analyst (requiere 4 servidores)
.\scripts\start-exercise4-servers.ps1
.\scripts\verify-exercise4.ps1
```

### 4. Ejecutar Tests

```powershell
# Todos los tests con coverage
.\scripts\run-all-tests.ps1 -Coverage $true

# Solo tests de un ejercicio específico
.\scripts\run-all-tests.ps1 -Filter "Exercise1"

# Ver reporte de coverage
start coverage/report/index.html
```

---

## 📖 Recursos Adicionales

### Documentación Oficial

-   [MCP Specification](https://modelcontextprotocol.io/specification/2025-06-18) - Especificación oficial del protocolo
-   [JSON-RPC 2.0](https://www.jsonrpc.org/specification) - Especificación del protocolo de transporte
-   [.NET 10 Documentation](https://learn.microsoft.com/dotnet/) - Documentación de .NET

### Ejemplos y Tutoriales

-   [MCP Examples Repository](https://github.com/modelcontextprotocol/examples) - Ejemplos oficiales
-   [Building MCP Servers with .NET](https://youtu.be/example) - Video tutorial (placeholder)

### Comunidad

-   [MCP Discord](https://discord.gg/mcp) - Comunidad oficial
-   [Stack Overflow - mcp tag](https://stackoverflow.com/questions/tagged/mcp)
-   [GitHub Discussions](https://github.com/modelcontextprotocol/discussions)

### Herramientas

-   [MCP Inspector](https://github.com/modelcontextprotocol/inspector) - Debug tool para servidores MCP
-   [MCP Client SDK](https://github.com/modelcontextprotocol/sdk) - SDKs para múltiples lenguajes
-   [Postman Collection](./postman/MCP-Workshop.postman_collection.json) - Colección de requests

---

## 🏢 Casos de Uso Empresariales

### 1. CRM Enrichment (ROI: 725%)

Enriquecimiento automático de datos de clientes desde múltiples fuentes (LinkedIn, PremiumAPI, internal DB).

**Ahorro**: 80% reducción en tiempo de enriquecimiento manual  
**ROI**: 3 meses de payback

### 2. Document Compliance Auditor (96% time reduction)

Auditoría automática de documentos contra políticas corporativas (GDPR, SOX, HIPAA).

**Ahorro**: 96% reducción en tiempo de auditoría  
**Valor**: 450K€/año en costos evitados

### 3. Multi-Source Inventory Sync (120K€ value)

Sincronización de inventario en tiempo real entre ERP, e-commerce, almacenes, y proveedores.

**Beneficio**: Reducción de 35% en stockouts  
**Valor anual**: 120K€

### 4. AI-Powered Customer Insights (450K€ impact)

Agregación de datos de CRM, transacciones, redes sociales, y soporte para análisis de sentimiento.

**Impacto**: 18% incremento en customer retention  
**Valor anual**: 450K€

Más casos en [Roadmap & Casos B2B](./modules/09-roadmap-casos-b2b.md)

---

## 🛠️ Troubleshooting

### Problemas Comunes

**Error: "Port 5000 already in use"**

```powershell
# Cambiar puerto en appsettings.json o usar variable de entorno
$env:ASPNETCORE_URLS="http://localhost:5001"
dotnet run
```

**Error: "ModelContextProtocol package not found"**

```powershell
# Instalar paquete prerelease
dotnet add package ModelContextProtocol --prerelease
```

**Error: "JWT token invalid"**

```powershell
# Generar nuevo token con script
.\scripts\generate-jwt-token.ps1 -Role admin
```

Ver [Troubleshooting Guide](./TROUBLESHOOTING.md) completo.

---

## 🤝 Contribuciones

Este workshop es open-source. Contribuciones bienvenidas:

1. Fork el repositorio
2. Crea una branch (`git checkout -b feature/mejora-ejercicio2`)
3. Commit tus cambios (`git commit -am 'Añadir validación extra'`)
4. Push a la branch (`git push origin feature/mejora-ejercicio2`)
5. Crea un Pull Request

Ver [CONTRIBUTING.md](../CONTRIBUTING.md) para más detalles.

---

## 📜 Licencia

Este proyecto está licenciado bajo MIT License - ver [LICENSE](../LICENSE) para detalles.

---

## 📬 Contacto

-   **Instructor**: [Tu Nombre] - [@tu_twitter](https://twitter.com/tu_twitter)
-   **Email**: workshop@example.com
-   **Website**: [https://mcp-workshop.dev](https://mcp-workshop.dev)

---

## ⭐ Agradecimientos

-   [Anthropic](https://www.anthropic.com/) por crear el Model Context Protocol
-   [Microsoft](https://microsoft.com/) por .NET 10
-   Todos los contribuidores del workshop

---

<div align="center">

**¿Listo para comenzar?** 🚀

[📖 Leer Quickstart](./quickstart.md) | [👨‍🏫 Guía del Instructor](./INSTRUCTOR_HANDBOOK.md) | [💬 Discord Community](https://discord.gg/mcp)

</div>
