# Bloque 1: Apertura - Notas para el Instructor

**Duración objetivo**: 10 minutos (±1 minuto)  
**Estilo**: Energético, acogedor, estableciendo tono colaborativo

---

## ⏱️ Cronometraje Detallado

| Sección                            | Tiempo | Acumulado | Checkpoint        |
| ---------------------------------- | ------ | --------- | ----------------- |
| Bienvenida y presentación personal | 2 min  | 2 min     | Nombre en pizarra |
| Contexto MCP en ecosistema IA      | 3 min  | 5 min     | Diagrama visual   |
| Recorrido por agenda               | 3 min  | 8 min     | Agenda proyectada |
| Cultura de aprendizaje y Q&A       | 2 min  | 10 min    | Reglas claras     |

**⚠️ Alerta de tiempo**: Si llegas a minuto 8 sin haber mostrado la agenda completa, acelera la sección de cultura.

---

## 🎤 Script de Apertura (2 minutos)

### Introducción Personal

> "¡Bienvenidos! Soy [NOMBRE], y durante las próximas 3 horas vamos a explorar juntos el Model Context Protocol, o MCP. Antes de empezar, quiero saber: ¿cuántos de ustedes han trabajado con aplicaciones de IA generativa? [PAUSA PARA MANOS]. ¿Y cuántos han creado APIs REST en C#? [PAUSA]. Perfecto, tenemos un buen mix de experiencias."

**Puntos clave**:

-   ✅ **Usa tu nombre completo** y rol (si aplica)
-   ✅ **Pide participación inmediata** (manos levantadas) para romper el hielo
-   ✅ **Valida niveles de experiencia** sin hacer sentir mal a nadie

### Promesa del Taller

> "Al final de este taller, cada uno de ustedes habrá creado al menos UN servidor MCP funcional que expone datos y herramientas. No vamos a ver diapositivas todo el tiempo: 55 minutos de teoría, 80 minutos de práctica. Así que preparen sus teclados."

**Objetivo**: Establecer expectativa clara de taller práctico, no conferencia pasiva.

---

## 🖼️ Contexto Visual: MCP en el Ecosistema (3 minutos)

### Narrativa Recomendada

**Problema que resonará con la audiencia**:

> "Imaginen que están en una empresa y les piden integrar ChatGPT con vuestra base de datos de clientes. Hoy lo hacen creando una API REST personalizada, documentándola, gestionando autenticación, manejando errores... Y mañana les piden lo mismo para Claude. Más trabajo. Y pasado mañana para GitHub Copilot. ¿Ven el patrón? Cada herramienta necesita su propia integración."

**Solución MCP**:

> "MCP es como el USB-C de las aplicaciones de IA. Un solo protocolo, múltiples clientes. Hoy crearemos servidores MCP que CUALQUIER aplicación compatible puede consumir: Claude Desktop, Cursor, agentes personalizados... Sin reescribir nada."

### Diagrama Recomendado (en pizarra o presentación)

```
SIN MCP:
  ChatGPT  →  API Custom 1
  Claude   →  API Custom 2      } Trabajo duplicado
  Copilot  →  API Custom 3

CON MCP:
  ChatGPT  ↘
  Claude    →  Servidor MCP  →  Datos
  Copilot  ↗
```

**⚠️ Evitar**: Lenguaje técnico excesivo. Usa analogías (USB-C, enchufes eléctricos).

---

## 📋 Recorrido por Agenda (3 minutos)

### Estrategia de Presentación

**Proyecta la tabla de agenda** desde `01b-apertura.md` y enfatiza:

1. **Estructura 3-fases**:

    - Fase 1 (Bloques 1-3): "Fundamentos y primera demo"
    - Fase 2 (Bloques 4-8): "Ejercicios progresivos - aquí aprenderán haciendo"
    - Fase 3 (Bloques 9-11): "Producción y cierre"

2. **Ejercicios prácticos** (resalta en amarillo si tienes diapositivas):

    - Ejercicio 1: Guiado (instructor ayuda paso a paso)
    - Ejercicios 2-3: Independientes (con asistencia disponible)
    - Ejercicio 4: Integración completa (desafío final)

3. **Tiempo buffer**:
    > "_Verán que tengo tiempos asignados, pero si necesitamos 5 minutos extra en un ejercicio, los tomaremos del cierre. Lo importante es que todos logren avanzar._"

**Preguntas frecuentes anticipadas**:

-   **"¿Hay grabación?"** → [Respuesta según tu caso]
-   **"¿Necesito conocer Terraform?"** → "No, lo veremos juntos y está documentado"
-   **"¿Funciona en Mac/Linux?"** → "Sí, .NET 10.0 es multiplataforma"

---

## 🎓 Cultura de Aprendizaje (2 minutos)

### Mensajes Clave para Transmitir

**1. Seguridad psicológica**:

> "Este es un espacio seguro para experimentar. Si algo falla en tu código, perfecto: aprenderemos juntos por qué. No hay preguntas tontas."

**2. Colaboración activa**:

> "Recomiendo formar grupos de 2-3 personas. Si estás bloqueado más de 5 minutos, levanta la mano o pregunta en el chat. No sufras en silencio."

**3. Ritmo flexible**:

> "Si terminas un ejercicio antes, hay extensiones opcionales en cada sección. Si necesitas más tiempo, el código está en GitHub para que sigas practicando después."

**4. Verificación del entorno**:

> "Antes del primer ejercicio, ejecutaremos `verify-setup.ps1` para asegurar que todos tienen el entorno listo. Si alguien tiene problemas ahora, levante la mano y lo resolvemos durante el Bloque 2."

---

## 🛠️ Configuración Técnica Pre-Taller

### Antes de Comenzar el Bloque

**Checklist del instructor** (completar 15 min antes):

-   [ ] Proyector conectado y funcionando
-   [ ] Repositorio GitHub abierto en pantalla compartida
-   [ ] Visual Studio Code abierto con `McpWorkshop.sln` cargado
-   [ ] Terminal PowerShell visible (fuente grande: 16-18pt)
-   [ ] Script `verify-setup.ps1` listo para ejecutar
-   [ ] Timer visible (recomendado: https://onlinealarmkur.com/timer/es/ o app móvil)
-   [ ] Agua y micrófono funcionando

### Materiales para Distribuir

-   [ ] URL del repositorio GitHub escrita en pizarra
-   [ ] Enlace a guía rápida (si es digital)
-   [ ] Código QR para acceso rápido (opcional)

---

## 🎯 Señales de Éxito del Bloque 1

Al finalizar estos 10 minutos, deberías observar:

✅ **Engagement**:

-   Asistentes tienen laptops abiertas y Visual Studio Code iniciándose
-   Al menos 2-3 preguntas o comentarios en chat/presencial
-   Ambiente relajado (sonrisas, movimiento)

✅ **Comprensión**:

-   Asistentes pueden responder: "¿Qué es MCP en una frase?" → "Protocolo estándar para conectar IA a datos"
-   Entienden que habrá 4 ejercicios prácticos

✅ **Preparación técnica**:

-   La mayoría tiene el repositorio clonado o descargado
-   No hay preguntas técnicas bloqueantes (se resolverán en Bloque 2)

---

## ⚠️ Contingencias y Plan B

### Si te Quedas Sin Tiempo (llegar a minuto 12+)

**Cortar de aquí**:

-   ❌ Recursos adicionales detallados (están en documentación)
-   ❌ Explicación profunda de prerequisitos (lo harán en verify-setup)
-   ❌ Presentación exhaustiva de tecnologías (vendrán naturalmente)

**Mantener sí o sí**:

-   ✅ Promesa del taller (qué van a lograr)
-   ✅ Agenda visual (tabla de bloques)
-   ✅ Cultura de aprendizaje (seguridad psicológica)

### Si Hay Problemas Técnicos de Proyección

**Backup**: Dicta URL del repositorio y pide que abran `docs/modules/01b-apertura.md` en sus propios dispositivos:

```
https://github.com/[TU-USUARIO]/mcp-workshop
```

La documentación es autocontenida y pueden seguir sin proyector.

### Si Alguien No Tiene Entorno Instalado

**Respuesta**:

> "Perfecto, durante el Bloque 2 (teoría) tendrás 25 minutos para instalar .NET 10.0. Aquí está el link: [ESCRIBIR EN PIZARRA]. Mientras tanto, sigue la teoría que es fundamental."

**No bloquees el taller** esperando instalaciones. La mayoría debe poder continuar.

---

## 🎬 Transición al Bloque 2

### Frase de Cierre y Transición

> "Perfecto. Ahora que sabemos QUÉ es MCP y PARA QUÉ sirve, vamos a entrar en el CÓMO. En los próximos 25 minutos veremos la arquitectura de MCP, el flujo de comunicación JSON-RPC, y por qué esto es diferente de los plugins tradicionales. ¿Listos?"

**Acción física**: Cambiar de diapositiva/documento a `02b-fundamentos.md` de forma visible.

---

## 📝 Notas de Experiencias Anteriores

### Lo que Funciona Bien

-   ✅ **Analogía USB-C**: Resonó muy bien en talleres pasados
-   ✅ **Mostrar cronómetro visible**: Genera disciplina de tiempo
-   ✅ **Pedir manos levantadas temprano**: Rompe barrera de participación

### Lo que NO Hacer

-   ❌ **No profundizar en JSON-RPC aquí**: Demasiado técnico, viene en Bloque 3
-   ❌ **No mostrar código todavía**: Genera ansiedad innecesaria
-   ❌ **No prometer Azure gratis**: Costos aplican, ser transparente

### Feedback de Asistentes

> "Me gustó que establecieran expectativas claras desde el inicio"  
> "El ejemplo de APIs duplicadas me hizo conectar inmediatamente con mi trabajo"  
> "Hubiera apreciado más tiempo para setup inicial" ← Resuelto con verify-setup.ps1

---

## 🔄 Mejora Continua

**Después del taller**, registra:

-   ¿Cuánto tiempo real tomó este bloque? \***\*\_\*\***
-   ¿Qué pregunta no anticipaste? **\*\*\*\***\_**\*\*\*\***
-   ¿Qué analogía funcionó mejor? **\*\*\*\***\_**\*\*\*\***
-   ¿Hubo problemas técnicos? **\*\*\*\***\_\_\_\_**\*\*\*\***

---

**Preparado por**: Instructor del taller MCP  
**Última revisión**: Noviembre 2025  
**Próxima actualización**: Después de cada taller
