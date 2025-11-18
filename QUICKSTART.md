# 🚀 Guía de Inicio Rápido - Taller MCP

## Antes del Taller

### Requisitos del Sistema

-   **SDK .NET**: 10.0 o superior
-   **IDE**: Visual Studio 2022 o VS Code con C# Dev Kit
-   **PowerShell**: 7.0 o superior
-   **Git**: Para clonar el repositorio

### Instalación

```powershell
# 1. Clonar el repositorio
git clone <url-repositorio>
cd mcp-workshop

# 2. Verificar el entorno
.\scripts\verify-setup.ps1

# 3. Generar datos de ejemplo
.\scripts\create-sample-data.ps1

# 4. Construir la solución
dotnet build McpWorkshop.sln
```

**Salida esperada del script de verificación**:

```
✓ .NET SDK 10.0.x detectado
✓ PowerShell 7.x detectado
✓ Puertos 5000-5012 disponibles
✓ Paquetes NuGet restaurados correctamente
```

---

## Durante el Taller

### Ejercicio 1: Servidor de Recursos Estáticos

**Objetivo**: Crear un servidor MCP que sirva archivos JSON estáticos.

```powershell
# Ejecutar el servidor
cd src\McpWorkshop.Servers\Exercise1StaticResources
dotnet run

# Verificar (en otra terminal)
cd ..\..\..
.\scripts\verify-exercise1.ps1
```

**Prueba manual**:

```powershell
# Listar recursos disponibles
Invoke-RestMethod -Uri "http://localhost:5000/resources/list" -Method POST

# Obtener un recurso específico
Invoke-RestMethod -Uri "http://localhost:5000/resources/read" -Method POST `
    -Body '{"uri":"customers://001"}' -ContentType "application/json"
```

---

### Ejercicio 2: Servidor de Consultas Paramétricas

**Objetivo**: Implementar herramientas MCP con parámetros de entrada.

```powershell
# Ejecutar el servidor
cd src\McpWorkshop.Servers\Exercise2ParametricQuery
dotnet run

# Verificar (en otra terminal)
cd ..\..\..
.\scripts\verify-exercise2.ps1
```

**Prueba manual**:

```powershell
# Listar herramientas disponibles
Invoke-RestMethod -Uri "http://localhost:5001/tools/list" -Method POST

# Ejecutar búsqueda de cliente
Invoke-RestMethod -Uri "http://localhost:5001/tools/call" -Method POST `
    -Body '{"name":"search-customer","arguments":{"query":"Acme"}}' `
    -ContentType "application/json"
```

---

### Ejercicio 3: Servidor Seguro con JWT

**Objetivo**: Añadir autenticación JWT y límites de tasa.

```powershell
# Ejecutar el servidor
cd src\McpWorkshop.Servers\Exercise3SecureServer
dotnet run

# Verificar (en otra terminal)
cd ..\..\..
.\scripts\verify-exercise3.ps1
```

**Prueba manual**:

```powershell
# Obtener token JWT
$token = (Invoke-RestMethod -Uri "http://localhost:5002/auth/token" -Method POST `
    -Body '{"username":"admin","password":"P@ssw0rd!"}' `
    -ContentType "application/json").token

# Usar el token para llamar a herramientas
Invoke-RestMethod -Uri "http://localhost:5002/tools/call" -Method POST `
    -Headers @{Authorization="Bearer $token"} `
    -Body '{"name":"get-customer","arguments":{"id":"001"}}' `
    -ContentType "application/json"
```

---

### Ejercicio 4: Analista Virtual (Orquestación)

**Objetivo**: Orquestar múltiples servidores MCP backend.

```powershell
# 1. Iniciar los servidores backend
cd src\McpWorkshop.Servers\SqlMcpServer
start powershell -NoExit -Command "dotnet run"

cd ..\CosmosMcpServer
start powershell -NoExit -Command "dotnet run"

cd ..\RestApiMcpServer
start powershell -NoExit -Command "dotnet run"

# 2. Iniciar el servidor de orquestación
cd ..\Exercise4VirtualAnalyst
dotnet run

# 3. Verificar (en otra terminal)
cd ..\..\..\
.\scripts\verify-exercise4.ps1
```

**Prueba manual**:

```powershell
# Ejecutar consulta empresarial compleja
Invoke-RestMethod -Uri "http://localhost:5003/tools/call" -Method POST `
    -Body '{"name":"business-query","arguments":{"query":"¿Cuáles son los principales clientes por ingresos?"}}' `
    -ContentType "application/json"
```

---

## Referencia Rápida

### Comandos Esenciales

```powershell
# Limpiar y reconstruir
dotnet clean
dotnet restore
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar con logs detallados
dotnet run --verbosity detailed

# Detener todos los servidores dotnet
Get-Process dotnet | Stop-Process -Force
```

### Puertos del Servidor

| Servidor                   | Puerto |
| -------------------------- | ------ |
| Exercise1StaticResources   | 5000   |
| Exercise2ParametricQuery   | 5001   |
| Exercise3SecureServer      | 5002   |
| Exercise4VirtualAnalyst    | 5003   |
| SqlMcpServer (backend)     | 5010   |
| CosmosMcpServer (backend)  | 5011   |
| RestApiMcpServer (backend) | 5012   |

### Estructura del Protocolo MCP

**Formato de Solicitud**:

```json
{
    "jsonrpc": "2.0",
    "method": "resources/list | resources/read | tools/list | tools/call",
    "params": {
        /* parámetros específicos del método */
    },
    "id": 1
}
```

**Formato de Respuesta**:

```json
{
    "jsonrpc": "2.0",
    "result": {
        /* datos de respuesta */
    },
    "id": 1
}
```

### Métodos del Protocolo MCP

-   **resources/list**: Obtener recursos disponibles
-   **resources/read**: Leer contenido de un recurso
-   **tools/list**: Obtener herramientas disponibles
-   **tools/call**: Ejecutar una herramienta con argumentos

---

## Solución de Problemas

### "Puerto ya en uso"

```powershell
# Buscar proceso usando el puerto
netstat -ano | findstr :5001

# Eliminar proceso (reemplazar PID)
taskkill /PID <PID> /F
```

### "Conexión rechazada" en el Ejercicio 4

Asegúrate de que los 3 servidores backend están ejecutándose:

-   SqlMcpServer (5010)
-   CosmosMcpServer (5011)
-   RestApiMcpServer (5012)

### Errores de Compilación

```powershell
# Limpiar y reconstruir
dotnet clean
dotnet restore
dotnet build
```

### Fallo del Script de Verificación

1. Asegúrate de que el servidor está ejecutándose (`dotnet run`)
2. Espera 5 segundos para el inicio del servidor
3. Verifica que el puerto no esté bloqueado por el firewall

---

## Ayuda y Documentación

-   **Documentación Completa**: [docs/README.md](docs/README.md)
-   **Agenda**: [docs/AGENDA.md](docs/AGENDA.md)
-   **Referencia Rápida**: [docs/QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)
-   **Solución de Problemas**: [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)
-   **Guía del Instructor**: [docs/INSTRUCTOR_HANDBOOK.md](docs/INSTRUCTOR_HANDBOOK.md)

---

## Después del Taller

### Continuar Aprendiendo

-   Explora el despliegue en Azure: [docs/AZURE_DEPLOYMENT.md](docs/AZURE_DEPLOYMENT.md)
-   Revisa las notas del instructor para obtener conocimientos más profundos
-   Experimenta con herramientas y recursos personalizados
-   Únete a las discusiones de la comunidad MCP

### Comparte tu Opinión

-   Reporta problemas: GitHub Issues
-   Sugiere mejoras: Pull Requests
-   Comparte tus servidores MCP: Escaparate de la comunidad

---

**¡Feliz Aprendizaje! 🎓🚀**
