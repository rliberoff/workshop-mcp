# MCP Workshop - Pre-Session Checklist

Checklist de validación completa para instructores. Ejecutar 24 horas antes del workshop.

---

## ✅ Validación Técnica (60 minutos)

### 1. Environment Setup

-   [ ] **.NET SDK 10.0+** instalado y en PATH

    ```powershell
    dotnet --version  # Debe mostrar 10.0.x
    ```

-   [ ] **Visual Studio Code** o **Visual Studio 2022** instalado

    ```powershell
    code --version  # VS Code
    ```

-   [ ] **Git** instalado y configurado

    ```powershell
    git --version
    git config --global user.name
    ```

-   [ ] **PowerShell 7+** instalado

    ```powershell
    $PSVersionTable.PSVersion  # Debe ser 7.x
    ```

-   [ ] **Docker Desktop** instalado (opcional para ejercicios avanzados)
    ```powershell
    docker --version
    ```

### 2. Repository & Dependencies

-   [ ] **Repositorio clonado** y actualizado

    ```powershell
    git clone <repo-url>
    cd mcp-workshop
    git pull origin main
    ```

-   [ ] **Todos los proyectos compilan** sin errores

    ```powershell
    dotnet clean
    dotnet restore
    dotnet build -c Release
    # Verificar: Build succeeded. 0 Error(s)
    ```

-   [ ] **NuGet packages restaurados** correctamente
    ```powershell
    dotnet list package
    # Verificar ModelContextProtocol está presente
    ```

### 3. Sample Data

-   [ ] **Datos de muestra generados**

    ```powershell
    .\scripts\create-sample-data.ps1
    ```

-   [ ] **Archivos JSON existen** en proyectos
    ```powershell
    Get-ChildItem -Recurse -Include "*.json" -Path .\src\McpWorkshop.Servers\Exercise*\Data
    # Debe mostrar: customers.json, orders.json, products.json, regions.json
    ```

### 4. Exercise Validation

-   [ ] **Exercise 1: Static Resources** funciona

    ```powershell
    cd .\src\McpWorkshop.Servers\Exercise1StaticResources
    dotnet run &
    Start-Sleep 5
    .\scripts\verify-exercise1.ps1
    # Esperado: ✅ 2/2 tests passed
    ```

-   [ ] **Exercise 2: Parametric Query** funciona

    ```powershell
    cd .\src\McpWorkshop.Servers\Exercise2ParametricQuery
    dotnet run &
    Start-Sleep 5
    .\scripts\verify-exercise2.ps1
    # Esperado: ✅ 3/3 tools validated
    ```

-   [ ] **Exercise 3: Secure Server** funciona con JWT

    ```powershell
    cd .\src\McpWorkshop.Servers\Exercise3SecureServer
    dotnet run &
    Start-Sleep 5

    # Generar token de prueba
    $token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." # Token pre-generado

    .\scripts\verify-exercise3.ps1 -Token $token
    # Esperado: ✅ Authentication works, ✅ Rate limiting enforced
    ```

-   [ ] **Exercise 4: Virtual Analyst** funciona con orquestación

    ```powershell
    # Iniciar los 3 servidores MCP
    .\scripts\start-exercise4-servers.ps1
    Start-Sleep 10

    # Validar VirtualAnalyst
    cd .\src\McpWorkshop.Servers\Exercise4VirtualAnalyst
    dotnet run &
    Start-Sleep 5

    .\scripts\verify-exercise4.ps1
    # Esperado: ✅ Orchestration successful, ✅ SQL+Cosmos+REST integrated
    ```

### 5. Test Suite

-   [ ] **Todos los unit tests pasan**

    ```powershell
    .\scripts\run-all-tests.ps1
    # Verificar: Total tests: 96. Passed: 96. Failed: 0.
    ```

-   [ ] **Test coverage > 80%**
    ```powershell
    .\scripts\run-all-tests.ps1 -Coverage $true
    # Verificar coverage report en ./coverage/
    ```

### 6. Documentation

-   [ ] **README.md** es claro y actualizado

    ```powershell
    Get-Content .\docs\README.md | Measure-Object -Line
    # Verificar contiene tabla de agenda, prerequisites, quick start
    ```

-   [ ] **Módulos 01-11** existen en docs/modules/

    ```powershell
    Get-ChildItem .\docs\modules\*.md | Measure-Object
    # Debe mostrar: Count: 11
    ```

-   [ ] **Exercise guides** completas

    ```powershell
    Get-ChildItem .\docs\exercises\*.md
    # Debe mostrar: exercise1.md, exercise2.md, exercise3.md, exercise4.md
    ```

-   [ ] **Checklists** marcadas como completas
    ```powershell
    Get-Content .specify\checklists\*.md | Select-String '\[ \]' | Measure-Object
    # Verificar: Count: 0 (todas las tareas marcadas como [x])
    ```

---

## 🎨 Materiales de Presentación (30 minutos)

### 7. Slides

-   [ ] **Slide deck** actualizado con branding del evento
-   [ ] **Portada** con título, fecha, lugar, instructor
-   [ ] **Agenda** refleja timing actualizado (3 horas)
-   [ ] **Screenshots** de código son legibles (font 14+)
-   [ ] **Diagramas** de arquitectura son claros (1080p mínimo)
-   [ ] **Transiciones** no son distractoras (máximo fade in/out)
-   [ ] **Slide de contacto** con LinkedIn, GitHub, email

### 8. Live Coding Setup

-   [ ] **IDE configurado**:

    -   [ ] Font size 16+ (legible en proyector)
    -   [ ] Dark theme (menos fatiga visual)
    -   [ ] Extensions instaladas: C# Dev Kit, PowerShell
    -   [ ] Snippets de código precargados

-   [ ] **Terminal/PowerShell**:

    -   [ ] Font size 14+
    -   [ ] Color scheme de alto contraste
    -   [ ] Historial limpio (sin comandos sensibles)

-   [ ] **Browser**:

    -   [ ] Pestañas precargadas:
        -   [ ] GitHub repo
        -   [ ] MCP Spec (https://spec.modelcontextprotocol.io/)
        -   [ ] JWT.io (para debugging)
        -   [ ] Timer online (visible para asistentes)
    -   [ ] Bookmarks organizados en carpeta "MCP Workshop"
    -   [ ] Extensiones bloqueadoras de anuncios activas

-   [ ] **Postman/Insomnia**:
    -   [ ] Colección del workshop importada
    -   [ ] Requests organizadas por ejercicio
    -   [ ] Variables de entorno configuradas ({{baseUrl}}, {{token}})

### 9. Backup Materials

-   [ ] **Video de live coding** (8 minutos) como Plan B
-   [ ] **USB con repositorio completo** (offline backup)
-   [ ] **NuGet packages offline** (.nupkg files)
-   [ ] **Cheat sheets impresos** (5 copias en papel)
-   [ ] **Soluciones pre-implementadas** en carpeta separada

---

## 📡 Conectividad & Hardware (15 minutos)

### 10. Venue Setup

-   [ ] **Proyector probado**:

    -   [ ] Resolución óptima (1920x1080 o superior)
    -   [ ] Duplicar pantalla (no extender)
    -   [ ] Colores se ven correctamente (no washed out)

-   [ ] **Audio/Microphone**:

    -   [ ] Mic inalámbrico funciona (test de 5 min)
    -   [ ] Audio de laptop se escucha en speakers (para videos)
    -   [ ] Baterías de mic cargadas (llevar repuestos)

-   [ ] **Wi-Fi**:

    -   [ ] Speed test: >10 Mbps download, >5 Mbps upload
    -   [ ] Latencia <50ms (ping google.com)
    -   [ ] Conexión estable (no caídas intermitentes)
    -   [ ] Tener credentials del venue anotadas

-   [ ] **Alimentación**:
    -   [ ] Laptop cargado 100%
    -   [ ] Cargador a mano (no confiar en batería)
    -   [ ] Regleta con suficientes enchufes (para asistentes)

### 11. Contingency Plans

-   [ ] **Plan B para internet caído**:

    -   [ ] Hotspot móvil configurado y testeado
    -   [ ] Packages NuGet en USB (distribución local)
    -   [ ] Repositorio en carpeta compartida de red

-   [ ] **Plan B para proyector fallado**:

    -   [ ] Font gigante en IDE (size 24+)
    -   [ ] Código compartido en chat cada 2 min
    -   [ ] Impresos de backup disponibles

-   [ ] **Plan B para timing atrasado**:
    -   [ ] Exercise 2 puede convertirse en demo (ganar 15 min)
    -   [ ] Exercise 3 puede usar código pre-hecho (ganar 10 min)
    -   [ ] Bloque 9 (Enterprise Patterns) reducible a 15 min

---

## 👥 Asistentes & Comunicación (45 minutos antes)

### 12. Pre-Workshop Communication

-   [ ] **Email de recordatorio enviado** (24h antes):

    -   [ ] Link al repositorio
    -   [ ] Instrucciones de instalación de .NET 10
    -   [ ] Documento de prerequisites (QUICKSTART.md)
    -   [ ] Formulario de pre-assessment (opcional)

-   [ ] **Canal de comunicación activo**:
    -   [ ] Discord/Slack workspace creado
    -   [ ] Link de invitación compartido
    -   [ ] Canales organizados: #general, #exercise1, #exercise2, etc.

### 13. Day-Of Setup

-   [ ] **Llegar 60 minutos antes** del inicio
-   [ ] **Cartel de bienvenida** en puerta con Wi-Fi credentials
-   [ ] **Mesas organizadas**:

    -   [ ] Espacio para laptops
    -   [ ] Enchufes accesibles
    -   [ ] Visibilidad clara a proyector

-   [ ] **Materiales físicos distribuidos**:
    -   [ ] Name tags (si aplica)
    -   [ ] Cheat sheets impresos
    -   [ ] Post-its para "parking lot" de preguntas
    -   [ ] Formularios de feedback (papel o QR code)

---

## 🕒 Last Minute Check (15 minutos antes)

### 14. Final Technical Validation

```powershell
# Ejecutar este script 15 minutos antes del inicio:
.\scripts\verify-setup.ps1 -Verbose

# Iniciar todos los servidores de Exercise 4 (para demo final):
.\scripts\start-exercise4-servers.ps1

# Verificar health de cada servidor:
@(5000,5001,5002,5003,5004) | ForEach-Object {
    try {
        Invoke-RestMethod "http://localhost:$_/health"
        Write-Host "✅ Server on port $_ is healthy" -ForegroundColor Green
    } catch {
        Write-Host "❌ Server on port $_ is DOWN" -ForegroundColor Red
    }
}

# Limpiar consolas (historial limpio para demo):
Clear-Host
```

### 15. Personal Readiness

-   [ ] **Hidratación**: Botella de agua a mano
-   [ ] **Notas**: Timing checklist impresa y visible
-   [ ] **Energía**: 5 minutos de respiración/mindfulness
-   [ ] **Backup laptop** (opcional): Segundo dispositivo con soluciones abiertas

---

## 🎯 Success Criteria

Al finalizar esta checklist, debes poder responder **SÍ** a:

1. ¿Compilaron todos los proyectos sin errores?
2. ¿Pasaron los 96 tests de la suite?
3. ¿Funcionan los 4 verify-exercise scripts?
4. ¿Es el contenido del repositorio accesible offline?
5. ¿Tienes al menos 2 planes B para cada categoría crítica?

**Si alguna respuesta es NO**: Resolver antes de iniciar el workshop.

---

## 📞 Emergency Contacts

Anotar aquí:

-   **Soporte técnico del venue**: ********\_\_\_********
-   **Coordinador del evento**: ********\_\_\_********
-   **Colega de respaldo** (para pair facilitation): ********\_\_\_********

---

## ✨ Final Notes

> "La preparación es la llave del éxito. Un instructor bien preparado puede convertir cualquier imprevisto en una oportunidad de enseñanza."

**¡Mucha suerte!** 🚀

---

**Última actualización**: [Fecha del workshop]  
**Instructor**: [Tu nombre]  
**Versión de checklist**: 1.0
