# Bloque 1: Apertura del Taller MCP (10 minutos)

**Duración**: 10 minutos  
**Objetivo**: Dar la bienvenida, establecer contexto y generar expectativas claras para el taller

---

## 🎯 Objetivos del Bloque

1. Presentar el instructor y el formato del taller
2. Contextualizar Model Context Protocol (MCP) en el ecosistema de IA
3. Establecer agenda y expectativas de aprendizaje
4. Crear ambiente colaborativo y de experimentación

---

## 📋 Agenda General del Taller (3 horas)

| Bloque | Tema                                       | Duración | Tipo                   |
| ------ | ------------------------------------------ | -------- | ---------------------- |
| 1      | Apertura y Bienvenida                      | 10 min   | Presentación           |
| 2      | Fundamentos de MCP                         | 25 min   | Teoría                 |
| 3      | Anatomía de un Proveedor MCP               | 20 min   | Live Coding            |
| 4      | **Ejercicio 1**: Recursos Estáticos        | 15 min   | Práctica Guiada        |
| 5      | **Ejercicio 2**: Consultas Paramétricas    | 20 min   | Práctica Independiente |
| 6      | **Ejercicio 3**: Seguridad y Autenticación | 20 min   | Práctica Independiente |
| 7      | Seguridad y Gobernanza (Sesión)            | 15 min   | Teoría                 |
| 8      | **Ejercicio 4**: Integración Multi-fuente  | 25 min   | Práctica Avanzada      |
| 9      | Despliegue en Azure (Sesión)               | 15 min   | Teoría                 |
| 10     | Demostración de Despliegue                 | 10 min   | Demostración           |
| 11     | Cierre y Q&A                               | 5 min    | Interactivo            |

---

## 🌟 ¿Qué es Model Context Protocol (MCP)?

> **MCP es un protocolo abierto que permite a las aplicaciones de IA conectarse de forma estandarizada a fuentes de datos y herramientas externas.**

### Contexto en el Ecosistema de IA

En 2024-2025, vemos una explosión de aplicaciones de IA generativa:

-   **Asistentes conversacionales** (ChatGPT, Claude, Gemini)
-   **Copilots de código** (GitHub Copilot, Cursor, Cline)
-   **Agentes autónomos** (AutoGPT, BabyAGI)
-   **Aplicaciones empresariales** con LLMs integrados

**Problema**: Cada herramienta necesita conectarse a datos empresariales (bases de datos, APIs, archivos) pero no existe un estándar común.

**Solución MCP**: Un protocolo universal que define cómo las aplicaciones de IA:

1. **Descubren** recursos y herramientas disponibles
2. **Acceden** a datos de forma segura y estructurada
3. **Invocan** funciones con parámetros validados
4. **Reportan** resultados en formatos consistentes

---

## 🎓 Lo que Aprenderás Hoy

Al finalizar este taller, serás capaz de:

### ✅ Fundamentos

-   Entender la diferencia entre MCP y sistemas tradicionales de plugins
-   Conocer la arquitectura cliente-servidor de MCP
-   Comprender el flujo JSON-RPC 2.0 subyacente

### ✅ Implementación Práctica

-   Crear un servidor MCP desde cero en C# / .NET 10.0
-   Exponer recursos estáticos (datos JSON)
-   Implementar herramientas (tools) con parámetros dinámicos
-   Agregar autenticación JWT y rate limiting
-   Integrar múltiples fuentes de datos (SQL, Cosmos DB, Blob Storage)

### ✅ Despliegue Empresarial

-   Desplegar servidores MCP en Azure Container Apps
-   Configurar logging y observabilidad estructurada
-   Aplicar prácticas de seguridad y gobernanza

---

## 🛠️ Tecnologías que Usaremos

### Lenguajes y Frameworks

-   **C# / .NET 10.0**: Lenguaje principal
-   **ModelContextProtocol NuGet**: Librería oficial de MCP
-   **xUnit**: Testing framework

### Infraestructura Azure

-   **Azure Container Apps**: Hosting de servidores MCP
-   **Azure SQL Database**: Datos relacionales
-   **Azure Cosmos DB**: Datos NoSQL
-   **Azure Blob Storage**: Archivos y objetos
-   **Azure Log Analytics**: Observabilidad

### Herramientas

-   **Terraform**: Infrastructure as Code
-   **Visual Studio Code**: Editor recomendado
-   **Azure CLI**: Gestión de recursos

---

## 📦 Prerrequisitos Técnicos

### Software Instalado

-   ✅ .NET 10.0 SDK
-   ✅ PowerShell 7+
-   ✅ Azure CLI 2.80+
-   ✅ Terraform 1.14+
-   ✅ Visual Studio Code (recomendado)
-   ✅ Git (opcional)

### Conocimientos Previos

-   🟢 **Esencial**: C# básico (clases, métodos, async/await)
-   🟡 **Recomendado**: APIs REST y JSON
-   🟡 **Recomendado**: Conceptos de autenticación (JWT)
-   🔵 **Opcional**: Azure básico

### Verificación del Entorno

Antes de comenzar, ejecuta el script de verificación:

```powershell
.\scripts\verify-setup.ps1
```

Deberías ver:

```powershell
✓ [REQUERIDO] .NET SDK - Versión correcta instalada
✓ [REQUERIDO] PowerShell - PowerShell 7+ instalado
✓ [REQUERIDO] Puertos TCP - Puertos 5000-5003 disponibles
✓ [REQUERIDO] NuGet Sources - NuGet.org configurado correctamente
✓ [REQUERIDO] Azure CLI - Azure CLI 2.80.0+ instalado
✓ [REQUERIDO] Terraform - Terraform 1.14.0+ instalado
✓ [REQUERIDO] Git - Git instalado

========================================
Estado general: PASS
========================================

✅ El entorno está listo para el taller MCP
```

---

## 🎯 Criterios de Éxito del Taller

Al finalizar, consideraremos el taller exitoso si:

1. **80%+ de grupos** completan el Ejercicio 1 (recursos estáticos)
2. **70%+ de grupos** completan el Ejercicio 2 (consultas paramétricas)
3. **90%+ de grupos** implementan el Ejercicio 4 (integración multi-fuente) sin errores críticos
4. **Satisfacción promedio** ≥ 4.0/5.0 en encuesta final
5. **Asistentes pueden articular** la diferencia entre MCP y sistemas de plugins tradicionales

---

## 💡 Cultura de Aprendizaje

### Durante el Taller

**✅ Esperamos que...**

-   Hagas preguntas en cualquier momento
-   Experimentes y cometas errores (así se aprende)
-   Compartas descubrimientos con tu grupo
-   Pidas ayuda cuando estés bloqueado más de 5 minutos

**❌ No te preocupes si...**

-   No completas todos los ejercicios a tiempo
-   Necesitas revisar conceptos de C# durante la práctica
-   Algunos ejercicios te resultan desafiantes
-   No tienes experiencia previa con Azure

### Recursos Disponibles

-   **Guía rápida impresa**: Referencia de comandos y conceptos
-   **Repositorio GitHub**: Todo el código y documentación
-   **Instructor**: Disponible para preguntas durante ejercicios
-   **Compañeros**: Forma grupos de 2-3 personas

---

## 🔗 Estructura del Repositorio

```
mcp-workshop/
├── docs/
│   ├── modules/                  # Documentación de cada bloque del taller
│   ├── QUICK_REFERENCE.md        # Referencia rápida de comandos y patrones
│   ├── TROUBLESHOOTING.md        # Solución de problemas comunes
│   ├── AZURE_DEPLOYMENT.md       # Guía de despliegue en Azure
│   └── README.md                 # Documentación principal del workshop
├── src/
│   ├── McpWorkshop.Shared/       # Librería compartida (helpers, logging, config)
│   └── McpWorkshop.Servers/      # Servidores MCP de cada ejercicio
│       ├── Exercise1StaticResources/
│       ├── Exercise2ParametricQuery/
│       ├── Exercise3SecureServer/
│       └── Exercise4VirtualAnalyst/
├── templates/
│   ├── exercise1-starter/        # Código inicial para Ejercicio 1
│   ├── exercise2-starter/        # Código inicial para Ejercicio 2
│   ├── exercise3-starter/        # Código inicial para Ejercicio 3
│   └── exercise4-starter/        # Código inicial para Ejercicio 4
├── tests/
│   └── McpWorkshop.Tests/        # Tests automatizados de los ejercicios
├── scripts/
│   ├── verify-setup.ps1          # Verifica entorno y dependencias
│   ├── verify-exercise1.ps1      # Valida Ejercicio 1
│   ├── verify-exercise2.ps1      # Valida Ejercicio 2
│   ├── verify-exercise3.ps1      # Valida Ejercicio 3
│   ├── verify-exercise4.ps1      # Valida Ejercicio 4
│   ├── create-sample-data.ps1    # Genera datos de muestra
│   └── run-all-tests.ps1         # Ejecuta todos los tests
├── infrastructure/
│   ├── terraform/                # Módulos de Terraform para Azure
│   └── scripts/
│       ├── deploy.ps1            # Script de despliegue
│       └── teardown.ps1          # Script de limpieza
├── Data/                         # Datos generados (no en el repo)
├── McpWorkshop.sln               # Solución de Visual Studio
└── README.md                     # Guía de inicio rápido del repositorio
```

---

## ⏱️ Gestión del Tiempo

| Fase                               | Tiempo Asignado | Señal de Avance           |
| ---------------------------------- | --------------- | ------------------------- |
| **Teoría** (Bloques 2, 7, 9)       | 55 min total    | Diapositivas completas    |
| **Live Coding** (Bloque 3)         | 20 min          | Servidor básico funcional |
| **Ejercicios** (Bloques 4-6, 8)    | 80 min total    | Verificación por grupo    |
| **Demos y Cierre** (Bloques 10-11) | 15 min          | Despliegue exitoso        |

**Regla de oro**: Si un grupo está bloqueado más de 5 minutos, levanta la mano para asistencia.

---

## 🚀 ¡Comencemos!

**Siguiente bloque**: Fundamentos de MCP (25 minutos)

En el próximo bloque exploraremos:

-   ¿Qué problema resuelve MCP?
-   Arquitectura cliente-servidor
-   Comparación con plugins tradicionales
-   Casos de uso reales

**Acción**: Abre el repositorio en Visual Studio Code y prepara tu terminal.

---

## 📚 Recursos Adicionales

-   **Especificación MCP oficial**: https://modelcontextprotocol.io/specification/2025-06-18
-   **GitHub ModelContextProtocol**: https://github.com/modelcontextprotocol
-   **Documentación .NET 10.0**: https://learn.microsoft.com/dotnet
-   **Azure Documentation**: https://learn.microsoft.com/azure

---

**Preparado por**: Instructor del taller MCP  
**Versión**: 1.0.0  
**Última actualización**: Noviembre 2025
