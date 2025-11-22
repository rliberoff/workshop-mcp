# Azure Deployment Guide - MCP Workshop

Guía completa para desplegar la infraestructura del workshop en Azure usando Terraform.

---

## 📋 Prerequisites

Antes de comenzar el despliegue, asegúrate de tener:

-   [ ] **Azure CLI** instalado y configurado

    ```powershell
    az --version  # Debe mostrar 2.80.0+
    az login
    az account set --subscription "<Your-Subscription-ID>"
    ```

-   [ ] **Terraform** instalado (1.14.0+)

    ```powershell
    terraform --version  # Debe mostrar 1.14.x+
    ```

-   [ ] **Permisos en Azure**:

    -   Contributor role en la suscripción
    -   Capacidad para crear Service Principals
    -   Acceso a crear Resource Groups

-   [ ] **Variables de entorno configuradas**:
    ```powershell
    $env:ARM_SUBSCRIPTION_ID="<your-subscription-id>"
    $env:ARM_TENANT_ID="<your-tenant-id>"
    ```

---

## 🚀 Quick Start Deployment

> **📌 NOTA IMPORTANTE**: El deployment se realiza en **dos fases**:
>
> **Fase 1 (Automática)**: Infraestructura base
>
> -   ✅ SQL Server + Database (vacía)
> -   ✅ Cosmos DB + Containers (vacíos)
> -   ✅ Storage Account (sin datos)
> -   ✅ Monitoring (Application Insights + Log Analytics)
> -   ✅ Container Apps Environment (sin apps)
>
> **Fase 2 (Opcional - Manual)**: MCP Servers
>
> -   ⚠️ Requiere construir imágenes Docker (ver Step 8)
> -   ⚠️ Requiere Azure Container Registry o Docker Hub
> -   ⚠️ Necesario solo para Exercise 4 (orquestación)
>
> **Los Exercises 1-3 pueden completarse sin Fase 2**, usando desarrollo local.

### Option A: One-Command Deployment (Automated)

```powershell
# Desde el directorio infrastructure
cd infrastructure
.\scripts\deploy.ps1

# Este script ejecuta:
# 1. Validación de herramientas (Terraform, Azure CLI)
# 2. Terraform init
# 3. Terraform plan
# 4. Terraform apply (pide confirmación)
# 5. Guarda outputs en outputs-workshop.json
```

**Para auto-aprobar sin confirmación**:

```powershell
.\scripts\deploy.ps1 -AutoApprove
```

**Tiempo estimado**: 8-12 minutos (solo Fase 1 - infraestructura base)

---

### Option B: Step-by-Step Deployment (Manual)

#### Step 1: Initialize Terraform

```powershell
cd infrastructure\terraform

# Inicializar Terraform (usa backend local para simplificar el workshop)
terraform init

# Verificar que no hay errores
# Output esperado: "Terraform has been successfully initialized!"
```

**Nota:** El workshop usa un backend local de Terraform. El estado se guarda en `terraform.tfstate` en el directorio actual y NO debe subirse a Git (ya está en `.gitignore`).

#### Step 2: Configure Environment

Editar el archivo de variables:

```powershell
code environments\workshop\terraform.tfvars
```

**Configuración mínima requerida**:

```hcl
# environments/workshop/terraform.tfvars
environment         = "workshop"
location            = "swedencentral"  # Región por defecto
name_prefix         = "mcpworkshop"
sql_admin_username  = "mcpadmin"

# Sufijo aleatorio para nombres únicos (habilitado por defecto)
use_random_suffix   = true
```

**Nota**: El sufijo aleatorio se genera automáticamente para evitar colisiones en nombres de recursos globales como Storage Accounts y Cosmos DB. Puedes deshabilitarlo estableciendo `use_random_suffix = false` y proporcionando un `suffix` personalizado.

#### Step 3: Generate Secrets

El script de deployment genera automáticamente:

-   JWT secret (64 caracteres)
-   SQL admin password (20 caracteres)

Archivo generado: `environments/workshop/secrets.auto.tfvars`

**📝 NOTA**: Para el workshop, Azure AD admin es **opcional**. Si lo necesitas, actualiza estos valores después de la generación:

```hcl
# environments/workshop/secrets.auto.tfvars (opcional)
azuread_admin_login     = "your-admin-email@domain.com"
azuread_admin_object_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

Para obtener tu Object ID:

```powershell
az ad user show --id your-admin-email@domain.com --query id -o tsv
```

#### Step 4: Review Infrastructure Plan

```powershell
# Crear plan de despliegue
terraform plan -var-file="environments\workshop\terraform.tfvars" -var-file="environments\workshop\secrets.auto.tfvars" -out=workshop.tfplan

# Revisar recursos que se crearán (deployment inicial):
# - Resource Group: rg-mcpworkshop-<suffix>
# - Container Apps Environment: mcpworkshop-env-<suffix> (sin apps todavía)
# - Azure SQL Database: sqldb-mcpworkshop
# - Azure SQL Server: mcpworkshop-sql-<suffix>
# - Cosmos DB Account: mcpworkshop-cosmos-<suffix>
# - Cosmos DB Containers: sessions, cart_events
# - Azure Blob Storage: mcpworkshopst<suffix> (sin datos todavía)
# - Application Insights: mcpworkshop-ai-<suffix>
# - Log Analytics Workspace: mcpworkshop-law-<suffix>
# Nota: <suffix> es un identificador aleatorio de 8 caracteres hexadecimales

# Los Container Apps (4 servidores MCP) NO se crean en el deployment inicial
# porque están deshabilitados por defecto (enable_sql_server=false, etc.)
# Se habilitarán después de construir y publicar las imágenes Docker
```

**Recursos estimados (deployment inicial)**: 8-10 recursos principales

**Costo mensual estimado** (deployment inicial, sin Container Apps):

-   Azure SQL (Basic tier): ~$5
-   Cosmos DB (Serverless): ~$0-25 (usage-based)
-   Blob Storage: ~$1-5
-   Application Insights: ~$0-10 (Free tier 5GB)
-   **Total estimado inicial**: $5-45/month

**Con Container Apps habilitados** (después de Step 8):

-   Container Apps: +$50-100 (depends on scale)
-   **Total con apps**: $60-145/month

#### Step 5: Apply Infrastructure

```powershell
# Aplicar el plan
terraform apply workshop.tfplan

# Confirmar cuando pregunte: "yes"
# Output esperado después de 8-12 min:
# Apply complete! Resources: 8-10 added, 0 changed, 0 destroyed.

# Nota: Este deployment inicial NO incluye Container Apps
# Los MCP servers se habilitarán después de construir las imágenes Docker (Step 8)
```

#### Step 6: Capture Output Variables

```powershell
# Ver todos los outputs
terraform output

# Guardar outputs en JSON
terraform output -json > outputs-workshop.json

# Ver valores específicos:
terraform output sql_connection_string
terraform output cosmos_endpoint
terraform output storage_connection_string
terraform output -json deployment_summary
```

**Ejemplo de output**:

```json
{
    "resource_suffix": {
        "sensitive": false,
        "type": "string",
        "value": "a1b2c3d4"
    },
    "sql_connection_string": {
        "sensitive": true,
        "type": "string",
        "value": "Server=tcp:mcpworkshop-sql-a1b2c3d4.database.windows.net,1433;..."
    },
    "cosmos_endpoint": {
        "sensitive": false,
        "type": "string",
        "value": "https://mcpworkshop-cosmos-a1b2c3d4.documents.azure.com:443/"
    },
    "sql_mcp_server_url": {
        "sensitive": false,
        "type": "string",
        "value": "https://sql-mcp-server-a1b2c3d4.azurecontainerapps.io"
    }
}
```

**Nota**: El sufijo aleatorio (ej: `a1b2c3d4`) se genera automáticamente y garantiza que los nombres de recursos sean únicos globalmente.

#### Step 7: Verify Initial Infrastructure

**Verifica que la infraestructura base se desplegó correctamente**:

```powershell
# Obtener el sufijo generado
$suffix = terraform output -raw resource_suffix
$rgName = "rg-mcpworkshop-$suffix"

# Listar recursos creados
az resource list --resource-group $rgName --output table

# Verificar SQL Server
az sql server show --name "mcpworkshop-sql-$suffix" --resource-group $rgName

# Verificar Cosmos DB
az cosmosdb show --name "mcpworkshop-cosmos-$suffix" --resource-group $rgName

# Verificar Container Apps Environment (sin apps todavía)
az containerapp env list --resource-group $rgName --output table
```

**✅ En este punto tienes**:

-   ✅ SQL Server + Database (vacía, sin datos)
-   ✅ Cosmos DB + Containers (vacíos)
-   ✅ Storage Account (sin blobs)
-   ✅ Monitoring (Application Insights + Log Analytics)
-   ❌ Container Apps (deshabilitados hasta Step 8)

#### Step 8: Build and Deploy Container Apps (Opcional)

**⚠️ IMPORTANTE**: Los Container Apps están **deshabilitados por defecto** en el deployment inicial porque requieren imágenes Docker que no existen todavía.

Para habilitar los MCP servers:

**Opción A: Crear Azure Container Registry y construir imágenes**

```powershell
# 1. Obtener sufijo
$suffix = terraform output -raw resource_suffix
$rgName = "rg-mcpworkshop-$suffix"

# 2. Crear ACR
az acr create `
  --name "mcpworkshop$suffix" `
  --resource-group $rgName `
  --sku Basic `
  --location swedencentral `
  --admin-enabled true

# 3. Construir y publicar imágenes directamente en ACR
# (az acr build hace build + push en un solo comando)
$acrName = "mcpworkshop$suffix"

# SQL MCP Server
cd src\McpWorkshop.Servers\Exercise4SqlMcpServer
az acr build --registry $acrName --image sql-mcp-server:latest .

# Cosmos MCP Server
cd ..\Exercise4CosmosMcpServer
az acr build --registry $acrName --image cosmos-mcp-server:latest .

# REST MCP Server
cd ..\Exercise4RestApiMcpServer
az acr build --registry $acrName --image rest-mcp-server:latest .

# Virtual Analyst
cd ..\Exercise4VirtualAnalyst
az acr build --registry $acrName --image virtual-analyst:latest .
```

**Opción B: Usar imágenes de Docker Hub** (si ya las publicaste)

```powershell
# Si tienes imágenes públicas en Docker Hub
# Solo actualiza las variables en terraform.tfvars
```

**4. Actualizar Terraform para habilitar Container Apps**:

Editar `environments/workshop/terraform.tfvars`:

```hcl
# MCP Servers - Habilitar después de construir imágenes
enable_sql_server      = true
enable_cosmos_server   = true
enable_rest_server     = true
enable_virtual_analyst = true
```

Si usas ACR, también necesitas actualizar las imágenes en `modules/container-apps/variables.tf` o crear un archivo de override:

```hcl
# Crear: environments/workshop/images.auto.tfvars
sql_server_image      = "mcpworkshop<suffix>.azurecr.io/sql-mcp-server:latest"
cosmos_server_image   = "mcpworkshop<suffix>.azurecr.io/cosmos-mcp-server:latest"
rest_server_image     = "mcpworkshop<suffix>.azurecr.io/rest-mcp-server:latest"
virtual_analyst_image = "mcpworkshop<suffix>.azurecr.io/virtual-analyst:latest"
```

**5. Re-deploy con Container Apps habilitados**:

```powershell
cd infrastructure\terraform
terraform plan -var-file="environments/workshop/terraform.tfvars" -var-file="environments/workshop/secrets.auto.tfvars" -out=workshop.tfplan
terraform apply workshop.tfplan
```

**Nota**: Este paso es **opcional para el workshop**. Puedes completar los ejercicios 1-3 sin Container Apps, usando solo local development.

#### Step 9: Seed Data (Opcional)

**⚠️ Este paso es opcional** - Solo necesario si quieres datos de ejemplo en las bases de datos.

Por defecto, `seed_sample_data = false` y `upload_sample_data = false` en el deployment inicial.

**Para habilitar seeding de datos**:

1. Actualizar `environments/workshop/terraform.tfvars`:

```hcl
# Data Seeding - Habilitar para cargar datos de ejemplo
seed_sample_data     = true  # Cosmos DB
upload_sample_data   = true  # Blob Storage
```

2. Re-aplicar Terraform:

```powershell
cd infrastructure\terraform
terraform apply -var-file="environments/workshop/terraform.tfvars" -var-file="environments/workshop/secrets.auto.tfvars" -auto-approve
```

**Para SQL Server**, usar el script existente:

```powershell
# Obtener connection string
$connectionString = terraform output -raw sql_connection_string

# Usar script de generación de datos
cd scripts
.\create-sample-data.ps1

# El script creará tablas y datos de ejemplo en Azure SQL
```

#### Step 10: Validate Deployment

**Validar infraestructura base** (sin Container Apps):

```powershell
# Verificar que todos los recursos existen
$suffix = terraform output -raw resource_suffix
$rgName = "rg-mcpworkshop-$suffix"

# Contar recursos
$resourceCount = az resource list --resource-group $rgName --query "length(@)"
Write-Host "✅ Recursos desplegados: $resourceCount"

# Verificar SQL Database
$sqlServer = "mcpworkshop-sql-$suffix"
az sql db show --name "mcpworkshop" --server $sqlServer --resource-group $rgName --query "status"

# Verificar Cosmos DB
$cosmosAccount = "mcpworkshop-cosmos-$suffix"
az cosmosdb show --name $cosmosAccount --resource-group $rgName --query "provisioningState"

# Verificar Storage
$storageAccount = "mcpworkshopst$suffix"
az storage account show --name $storageAccount --resource-group $rgName --query "provisioningState"
```

**Validar Container Apps** (si completaste Step 8):

```powershell
# Probar health checks
$suffix = terraform output -raw resource_suffix

# Verificar que las apps existen
az containerapp list --resource-group "rg-mcpworkshop-$suffix" --query "[].{Name:name, Status:properties.runningStatus}" --output table

# Probar endpoints (si las apps están corriendo)
$sqlUrl = terraform output -raw sql_mcp_server_url
if ($sqlUrl) {
    try {
        Invoke-RestMethod -Uri "$sqlUrl/health" -Method Get -TimeoutSec 5
        Write-Host "✅ SQL MCP Server is healthy"
    } catch {
        Write-Host "❌ SQL MCP Server no responde: $_"
    }
}
```

**✅ Deployment Completo**

En este punto tienes:

-   ✅ Infraestructura base desplegada (SQL, Cosmos, Storage, Monitoring)
-   ✅ Sufijo único generado para evitar colisiones de nombres
-   ✅ Secrets generados y configurados
-   ⚠️ Container Apps: Opcionales (solo si completaste Step 8)
-   ⚠️ Data seeding: Opcional (solo si completaste Step 9)

---

## 🏗️ Infrastructure Details

### Resource Group Structure (Deployment Inicial)

```text
rg-mcpworkshop-<suffix>/
├── Container Apps Environment
│   └── mcpworkshop-env-<suffix> (sin Container Apps todavía)
├── Azure SQL Database
│   ├── sqldb-mcpworkshop (Database - vacía)
│   └── mcpworkshop-sql-<suffix> (Logical Server)
├── Cosmos DB Account
│   ├── mcpworkshop-cosmos-<suffix> (Account)
│   ├── workshop-db (Database)
│   ├── sessions (Container - vacío)
│   └── cart_events (Container - vacío)
├── Blob Storage
│   ├── mcpworkshopst<suffix> (Storage Account)
│   └── sample-data (Container - vacío)
├── Application Insights
│   ├── mcpworkshop-ai-<suffix> (Insights)
│   └── mcpworkshop-law-<suffix> (Log Analytics)

Nota: <suffix> es un identificador aleatorio de 8 caracteres (ej: a1b2c3d4)
```

### Resource Group Structure (Después de Step 8 - Con Container Apps)

```text
rg-mcpworkshop-<suffix>/
├── Container Apps Environment
│   ├── mcpworkshop-env-<suffix>
│   ├── sql-mcp-server-<suffix> (Container App) ✅
│   ├── cosmos-mcp-server-<suffix> (Container App) ✅
│   ├── rest-mcp-server-<suffix> (Container App) ✅
│   └── virtual-analyst-<suffix> (Container App) ✅
├── Azure SQL Database
│   ├── sqldb-mcpworkshop (Database)
│   └── mcpworkshop-sql-<suffix> (Logical Server)
├── Cosmos DB Account
│   ├── mcpworkshop-cosmos-<suffix> (Account)
│   └── workshop-db (Database + Containers)
├── Blob Storage
│   └── mcpworkshopst<suffix> (Storage Account)
├── Application Insights
│   ├── mcpworkshop-ai-<suffix> (Insights)
│   └── mcpworkshop-law-<suffix> (Log Analytics)
└── (Opcional) Azure Container Registry
    └── mcpworkshop<suffix> (ACR para imágenes Docker)

Nota: <suffix> es un identificador aleatorio de 8 caracteres (ej: a1b2c3d4)
```

### Terraform Module Organization

```text
infrastructure/terraform/
├── main.tf                    # Root module
├── variables.tf               # Input variables
├── outputs.tf                 # Output values
├── modules/
│   ├── container-apps/        # Container Apps Environment + Apps
│   ├── sql-database/          # Azure SQL configuration
│   ├── cosmos-db/             # Cosmos DB Serverless
│   ├── storage/               # Blob Storage with containers
│   └── monitoring/            # App Insights + Log Analytics
└── environments/
    └── workshop/
        ├── terraform.tfvars   # Workshop configuration
        └── secrets.auto.tfvars # Auto-generated secrets (git-ignored)
```

---

## 🔒 Security Configuration

### 1. Network Security

**Private Endpoints** (opcional para el workshop):

```hcl
# En environments/workshop/terraform.tfvars:
enable_private_endpoints = false  # Deshabilitado por defecto para simplificar el workshop
# Cambiar a true para escenarios de producción
```

### 2. Secrets Management

**Usando Terraform-generated secrets**:

Los secrets se generan automáticamente en `secrets.auto.tfvars` y se pasan como variables de entorno a los Container Apps.

**Para producción, considera Azure Key Vault**:

```powershell
# Obtener el sufijo del output de Terraform
$suffix = terraform output -raw resource_suffix

# Crear Key Vault
az keyvault create \
  --name "kv-mcpworkshop-$suffix" \
  --resource-group "rg-mcpworkshop-$suffix" \
  --location swedencentral

# Almacenar connection strings
az keyvault secret set \
  --vault-name "kv-mcpworkshop-$suffix" \
  --name "SqlConnectionString" \
  --value "$connectionString"

# Referenciar desde Container Apps (requiere actualizar Terraform)
az containerapp update \
  --name "sql-mcp-server-$suffix" \
  --secrets "sql-conn=keyvaultref:https://kv-mcpworkshop-$suffix.vault.azure.net/secrets/SqlConnectionString"
```

### 3. Authentication & Authorization

**Managed Identity** (ya configurado en módulo container-apps):

```hcl
# Ya implementado en modules/container-apps/main.tf:
identity {
  type = "SystemAssigned"
}
```

### 4. Firewall Rules

**SQL Server Firewall** (ya configurado):

-   Permite servicios de Azure (Container Apps)
-   Requiere configurar IPs adicionales para acceso desde localhost durante desarrollo

```powershell
# Obtener el sufijo y agregar tu IP temporalmente
$suffix = terraform output -raw resource_suffix
$myIp = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()

az sql server firewall-rule create \
  --resource-group "rg-mcpworkshop-$suffix" \
  --server "mcpworkshop-sql-$suffix" \
  --name "MyWorkstation" \
  --start-ip-address $myIp \
  --end-ip-address $myIp
```

---

## 📊 Monitoring & Diagnostics

### Application Insights Integration

Los Container Apps están configurados para enviar telemetría a Application Insights mediante variable de entorno `APPLICATIONINSIGHTS_CONNECTION_STRING`.

**Queries útiles en Log Analytics**:

```kusto
// Requests por servidor (últimas 24h)
requests
| where timestamp > ago(24h)
| summarize count() by cloud_RoleName
| render piechart

// Errores críticos
exceptions
| where severityLevel >= 3
| project timestamp, type, outerMessage, cloud_RoleName
| order by timestamp desc

// Performance de orquestación (Exercise 4)
dependencies
| where cloud_RoleName == "Exercise4VirtualAnalyst"
| where target contains "sql" or target contains "cosmos"
| summarize avg(duration), percentile(duration, 95) by target
| render timechart
```

### Health Checks

Todos los Container Apps deben exponer endpoint `/health`:

```powershell
# Verificar health de todos los servicios
terraform output -json deployment_summary | ConvertFrom-Json |
  Select-Object -ExpandProperty mcp_servers |
  ForEach-Object {
    $name = $_.PSObject.Properties.Name
    $url = $_.PSObject.Properties.Value.Value + "/health"

    try {
        Invoke-RestMethod -Uri $url -TimeoutSec 5 | Out-Null
        Write-Host "✅ $name is healthy" -ForegroundColor Green
    } catch {
        Write-Host "❌ $name is DOWN: $_" -ForegroundColor Red
    }
}
```

---

## 🧹 Cleanup & Teardown

### Remove All Resources

```powershell
# Opción A: Usando script de teardown (RECOMENDADO)
cd infrastructure
.\scripts\teardown.ps1

# Confirmar escribiendo: "destroy"
# Tiempo estimado: 10-15 minutos

# Para auto-aprobar:
.\scripts\teardown.ps1 -Force

# Para preservar logs:
.\scripts\teardown.ps1 -KeepLogs
```

```powershell
# Opción B: Terraform destroy manual
cd infrastructure\terraform
terraform destroy -var-file="environments\workshop\terraform.tfvars" -var-file="environments\workshop\secrets.auto.tfvars"
```

```powershell
# Opción C: Delete Resource Group (más rápido pero menos seguro)
az group delete --name rg-mcpworkshop --yes --no-wait
```

### Cost Optimization

**Para workshops de 1 día**:

1. **Scale to Zero** después del workshop:

    ```powershell
    az containerapp update \
      --name sql-mcp-server \
      --resource-group rg-mcpworkshop \
      --min-replicas 0 \
      --max-replicas 0
    ```

2. **Pause SQL Database**:

    ```powershell
    az sql db pause \
      --name sqldb-mcpworkshop \
      --server sql-mcpworkshop \
      --resource-group rg-mcpworkshop
    ```

3. **Cosmos DB Serverless**: Ya escala automáticamente a 0, solo pagas por RU/s consumidos

---

## 🐛 Troubleshooting

### Issue: Terraform Init Fails

**Error**:

```
Error: Failed to get existing workspaces: containers.Client#ListBlobs: Failure responding to request
```

**Solución**:

```powershell
# Re-autenticar en Azure
az login
az account set --subscription "<subscription-id>"

# Limpiar estado local y reinicializar
Remove-Item .terraform -Recurse -Force
terraform init -reconfigure
```

---

### Issue: Container App Not Responding

**Diagnóstico**:

```powershell
# Ver logs en tiempo real
az containerapp logs show \
  --name sql-mcp-server \
  --resource-group rg-mcpworkshop \
  --follow

# Revisar replicas
az containerapp revision list \
  --name sql-mcp-server \
  --resource-group rg-mcpworkshop \
  --query "[].{Name:name, Active:properties.active, Replicas:properties.replicas}" \
  --output table
```

**Solución**:

```powershell
# Restart container app
az containerapp revision restart \
  --name sql-mcp-server \
  --resource-group rg-mcpworkshop \
  --revision <revision-name>
```

---

### Issue: SQL Connection Timeout

**Error**:

```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred
```

**Diagnóstico**:

```powershell
# Verificar firewall rules
az sql server firewall-rule list \
  --resource-group rg-mcpworkshop \
  --server sql-mcpworkshop \
  --output table

# Test connectivity desde Container App
az containerapp exec \
  --name sql-mcp-server \
  --resource-group rg-mcpworkshop \
  --command "/bin/sh" -- -c "nc -zv sql-mcpworkshop.database.windows.net 1433"
```

**Solución**:

```powershell
# Verificar que "Allow Azure services" está habilitado
az sql server firewall-rule show \
  --resource-group rg-mcpworkshop \
  --server sql-mcpworkshop \
  --name AllowAllWindowsAzureIps

# Si no existe, crearlo
az sql server firewall-rule create \
  --resource-group rg-mcpworkshop \
  --server sql-mcpworkshop \
  --name AllowAllWindowsAzureIps \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

### Issue: Secrets Not Generated

**Error**:

```
Error: Missing required variable: sql_admin_password
```

**Solución**:

El script `deploy.ps1` genera automáticamente `secrets.auto.tfvars` si no existe. Si falla:

```powershell
# Generar manualmente
cd infrastructure\terraform\environments\workshop

# Crear archivo secrets.auto.tfvars
$jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object { [char]$_ })
$sqlPassword = -join ((65..90) + (97..122) + (48..57) + (33, 35, 36, 37, 38, 42, 43, 45, 61) | Get-Random -Count 20 | ForEach-Object { [char]$_ })

@"
jwt_secret          = "$jwtSecret"
sql_admin_password  = "$sqlPassword"

azuread_admin_login     = "admin@example.com"
azuread_admin_object_id = "00000000-0000-0000-0000-000000000000"
"@ | Out-File -FilePath "secrets.auto.tfvars" -Encoding UTF8

Write-Host "✓ secrets.auto.tfvars creado. Actualiza los valores de Azure AD."
```

---

## 📚 Additional Resources

| Resource                  | URL                                                                         |
| ------------------------- | --------------------------------------------------------------------------- |
| Azure Container Apps Docs | https://learn.microsoft.com/azure/container-apps/                           |
| Terraform Azure Provider  | https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs       |
| Azure SQL Best Practices  | https://learn.microsoft.com/azure/azure-sql/database/security-best-practice |
| Cosmos DB Serverless      | https://learn.microsoft.com/azure/cosmos-db/serverless                      |
| Infrastructure README     | [infrastructure/README.md](../infrastructure/README.md)                     |

---

**Deployment Complete!** 🎉

Para soporte adicional, consultar [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) o los scripts en [`infrastructure/scripts/`](../infrastructure/scripts/).
