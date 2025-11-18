# Taller MCP: Model Context Protocol en Azure

Taller práctico de 3 horas para aprender a construir servidores MCP que explotan datos desde diversas fuentes utilizando C# .NET 10.0, Azure y Terraform.

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
3. **Anatomía de un Proveedor** (20 min) - Live coding de servidor MCP
4. **Ejercicio 1** (15 min) - Recursos estáticos
5. **Ejercicio 2** (20 min) - Consultas paramétricas
6. **Ejercicio 3** (20 min) - Seguridad y gobernanza
7. **Security & Gobernanza** (15 min) - Micro-charla sobre patrones empresariales
8. **Ejercicio 4** (30 min) - Reto integrador: Analista virtual
9. **Orquestación Multi-Fuente** (15 min) - Patrones de integración
10. **Roadmap & Casos B2B** (10 min) - Escenarios de negocio
11. **Cierre** (10 min) - Retrospectiva y siguientes pasos

## 🚀 Quick Start

### Prerequisitos

-   **SDK**: .NET 10.0 o superior
-   **IDE**: Visual Studio 2022 o VS Code con C# Dev Kit
-   **Tools**: Git, PowerShell 7+
-   **Azure** (opcional): Cuenta de Azure para ejercicios cloud

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

### Ejecutar Primer Ejercicio

```powershell
# Navegar al ejercicio 1
cd src\McpWorkshop.Servers\Exercise1StaticResources

# Ejecutar servidor MCP
dotnet run

# En otra terminal, verificar
.\scripts\verify-exercise1.ps1
```

## 📖 Documentación

-   **[Agenda Completa](docs/AGENDA.md)** - Cronograma detallado del taller
-   **[Quick Reference](docs/QUICK_REFERENCE.md)** - Referencia rápida MCP y C#
-   **[Instructor Handbook](docs/INSTRUCTOR_HANDBOOK.md)** - Guía de facilitación
-   **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Solución de problemas comunes
-   **[Azure Deployment](docs/AZURE_DEPLOYMENT.md)** - Despliegue en Azure

### Módulos por Bloque

-   [01 - Apertura](docs/modules/01-apertura.md)
-   [02 - Fundamentos](docs/modules/02-fundamentos.md)
-   [03 - Anatomía de un Proveedor](docs/modules/03-anatomia-proveedor.md)
-   [04 - Ejercicio 1: Recursos Estáticos](docs/modules/04-ejercicio-1-recursos-estaticos.md)
-   [05 - Ejercicio 2: Consultas Paramétricas](docs/modules/05-ejercicio-2-consultas-parametricas.md)
-   [06 - Ejercicio 3: Seguridad](docs/modules/06-ejercicio-3-seguridad.md)
-   [07 - Security & Gobernanza](docs/modules/07-seguridad-gobernanza.md)
-   [08 - Ejercicio 4: Analista Virtual](docs/modules/08-ejercicio-4-analista-virtual.md)
-   [09 - Orquestación Multi-Fuente](docs/modules/09-orquestacion-multifuente.md)
-   [10 - Roadmap & Casos B2B](docs/modules/10-roadmap-casos-b2b.md)
-   [11 - Cierre](docs/modules/11-cierre.md)

## 🏗️ Estructura del Proyecto

```
mcp-workshop/
├── docs/                    # Documentación del taller (Markdown)
│   ├── modules/            # Módulos de aprendizaje (11 bloques)
│   └── diagrams/           # Diagramas Mermaid
├── src/                    # Código fuente
│   └── McpWorkshop.Servers/
│       ├── Exercise1StaticResources/
│       ├── Exercise2ParametricQuery/
│       ├── Exercise3SecureServer/
│       ├── Exercise4SqlMcpServer/
│       ├── Exercise4CosmosMcpServer/
│       ├── Exercise4RestApiMcpServer/
│       ├── Exercise4VirtualAnalyst/
│       └── McpWorkshop.Shared/
├── tests/                  # Pruebas xUnit
│   └── McpWorkshop.Tests/
├── infrastructure/         # Terraform para Azure
│   └── terraform/
│       └── modules/
├── scripts/               # Scripts PowerShell
│   ├── verify-setup.ps1
│   ├── create-sample-data.ps1
│   └── verify-exercise*.ps1
└── templates/             # Plantillas para ejercicios
    ├── exercise1-starter/
    ├── exercise2-starter/
    ├── exercise3-starter/
    └── exercise4-starter/
```

## 🎓 Ejercicios Prácticos

### Ejercicio 1: Recursos Estáticos (15 min)

Crear un servidor MCP que expone listas de clientes y productos como recursos estáticos.

### Ejercicio 2: Consultas Paramétricas (20 min)

Implementar herramientas MCP con parámetros para búsquedas y filtros dinámicos.

### Ejercicio 3: Servidor Seguro (20 min)

Agregar autenticación JWT, autorización por scopes, rate limiting y logging estructurado.

### Ejercicio 4: Analista Virtual (30 min - Grupal)

Reto integrador: construir un agente que orquesta múltiples servidores MCP (SQL, Cosmos DB, REST API) para responder preguntas de negocio en lenguaje natural.

## 🛠️ Tecnologías

-   **Lenguaje**: C# .NET 10.0
-   **MCP Library**: ModelContextProtocol (NuGet prerelease)
-   **Azure Services**:
    -   Azure Container Apps
    -   Azure App Service
    -   Azure SQL Database
    -   Azure Cosmos DB
    -   Azure Blob Storage
    -   Azure Log Analytics
-   **IaC**: Terraform
-   **Testing**: xUnit, Microsoft.AspNetCore.Mvc.Testing

## 📊 Criterios de Éxito

-   ✅ 80% de asistentes completan Ejercicio 1
-   ✅ 70% de asistentes completan Ejercicio 2
-   ✅ 90% de grupos completan Ejercicio 4
-   ✅ Satisfacción promedio ≥ 4.0/5.0

## 🤝 Contribuir

Este es un proyecto educativo. Las contribuciones son bienvenidas:

1. Fork el repositorio
2. Crea una rama feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit con [Conventional Commits](https://www.conventionalcommits.org/)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## 🙋 Soporte

-   **Issues**: [GitHub Issues](<repository-url>/issues)
-   **Discussions**: [GitHub Discussions](<repository-url>/discussions)
-   **Documentación**: [docs/](docs/)

## 🌟 Créditos

Desarrollado como parte del Data Saturday Madrid Workshop 2025.

---

**¡Disfruta del taller y construye servidores MCP increíbles! 🚀**
