# Azure Deployment Guide - MCP Workshop

Guía completa para desplegar la infraestructura del workshop en Azure usando Terraform.

---

## 📋 Prerequisites

Antes de comenzar el despliegue, asegúrate de tener:

-   [ ] **Azure CLI** instalado y configurado

    ```powershell
    az --version  # Debe mostrar 2.50+
    az login
    az account set --subscription "<Your-Subscription-ID>"
    ```

-   [ ] **Terraform** instalado (1.5+)

    ```powershell
    terraform --version  # Debe mostrar 1.5.x+
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

### Option A: One-Command Deployment (Automated)

```powershell
# Desde la raíz del repositorio
.\scripts\deploy-azure.ps1 -Environment workshop -Region eastus2

# Este script ejecuta:
# 1. Terraform init
# 2. Terraform plan
# 3. Terraform apply (pide confirmación)
# 4. Configuración de connection strings en appsettings
```

**Tiempo estimado**: 15-20 minutos

---

### Option B: Step-by-Step Deployment (Manual)

#### Step 1: Initialize Terraform

```powershell
cd infrastructure\terraform\environments\workshop

# Inicializar backend de Terraform
terraform init

# Verificar que no hay errores
# Output esperado: "Terraform has been successfully initialized!"
```

#### Step 2: Review Infrastructure Plan

```powershell
# Crear plan de despliegue
terraform plan -out=workshop.tfplan

# Revisar recursos que se crearán:
# - Resource Group: rg-mcp-workshop-eastus2
# - Container Apps Environment: mcpworkshop-env
# - 7 Container Apps (Exercise1-4 servers + VirtualAnalyst)
# - Azure SQL Database: sqldb-mcpworkshop
# - Cosmos DB Account: cosmos-mcpworkshop
# - Azure Blob Storage: stmcpworkshop
# - Application Insights: ai-mcpworkshop
# - Log Analytics Workspace: law-mcpworkshop
```

**Recursos estimados**: 12 recursos principales + networking/security

**Costo mensual estimado** (con Free Tier donde aplique):

-   Container Apps: ~$50-100 (depends on scale)
-   Azure SQL (Basic tier): ~$5
-   Cosmos DB (Serverless): ~$0-25 (usage-based)
-   Blob Storage: ~$1-5
-   Application Insights: ~$0-10 (Free tier 5GB)
-   **Total estimado**: $60-145/month

#### Step 3: Apply Infrastructure

```powershell
# Aplicar el plan
terraform apply workshop.tfplan

# Confirmar cuando pregunte: "yes"
# Output esperado después de 15-20 min:
# Apply complete! Resources: 12 added, 0 changed, 0 destroyed.
```

#### Step 4: Capture Output Variables

```powershell
# Terraform output muestra información crítica:
terraform output -json > deployment-outputs.json

# Ver valores importantes:
terraform output sql_connection_string
terraform output cosmos_endpoint
terraform output blob_storage_connection_string
terraform output container_app_urls
```

**Ejemplo de output**:

```json
{
    "sql_connection_string": {
        "sensitive": true,
        "type": "string",
        "value": "Server=tcp:sql-mcpworkshop.database.windows.net,1433;..."
    },
    "cosmos_endpoint": {
        "sensitive": false,
        "type": "string",
        "value": "https://cosmos-mcpworkshop.documents.azure.com:443/"
    },
    "exercise1_url": {
        "sensitive": false,
        "type": "string",
        "value": "https://exercise1-static-resources.politecoast-12345.eastus2.azurecontainerapps.io"
    }
}
```

#### Step 5: Configure Applications

```powershell
# Actualizar connection strings en todos los appsettings.json
.\scripts\configure-azure-settings.ps1 -OutputFile deployment-outputs.json

# Este script actualiza automáticamente:
# - ConnectionStrings:SalesDb en Exercise4SqlMcpServer
# - CosmosDb:Endpoint en Exercise4CosmosMcpServer
# - BlobStorage:ConnectionString en Exercise4RestApiMcpServer
# - ApplicationInsights:InstrumentationKey en todos los servidores
```

#### Step 6: Build and Push Container Images

```powershell
# Construir imágenes Docker para todos los servidores
.\scripts\build-docker-images.ps1

# Push a Azure Container Registry
$acrName = terraform output -raw acr_name
az acr login --name $acrName

docker tag exercise1-static-resources:latest $acrName.azurecr.io/exercise1-static-resources:latest
docker push $acrName.azurecr.io/exercise1-static-resources:latest

# Repetir para los 7 servidores
.\scripts\push-all-images.ps1 -AcrName $acrName
```

#### Step 7: Deploy to Container Apps

```powershell
# Terraform ya creó las Container Apps, ahora actualizamos con las imágenes:
terraform apply -var="deploy_images=true"

# O usar Azure CLI directamente:
az containerapp update \
  --name exercise1-static-resources \
  --resource-group rg-mcp-workshop-eastus2 \
  --image $acrName.azurecr.io/exercise1-static-resources:latest
```

#### Step 8: Seed Database

```powershell
# Ejecutar migraciones de EF Core
cd src\McpWorkshop.Servers\Exercise4SqlMcpServer

$connectionString = terraform output -raw sql_connection_string
dotnet ef database update --connection $connectionString

# Seed con datos de muestra
.\scripts\seed-azure-database.ps1 -ConnectionString $connectionString
```

#### Step 9: Validate Deployment

```powershell
# Probar cada endpoint en Azure
$exercise1Url = terraform output -raw exercise1_url

$body = @{ jsonrpc="2.0"; method="resources/list"; id=1 } | ConvertTo-Json
Invoke-RestMethod -Uri $exercise1Url -Method Post -Body $body -ContentType "application/json"

# Output esperado: Lista de 4 recursos (customers, orders, products, regions)

# Validar todos los ejercicios
.\scripts\validate-azure-deployment.ps1 -OutputFile deployment-outputs.json
# Esperado: ✅ 7/7 services healthy
```

---

## 🏗️ Infrastructure Details

### Resource Group Structure

```text
rg-mcp-workshop-eastus2/
├── Container Apps Environment
│   ├── exercise1-static-resources (Container App)
│   ├── exercise2-parametric-query (Container App)
│   ├── exercise3-secure-server (Container App)
│   ├── exercise4-sql-mcp-server (Container App)
│   ├── exercise4-cosmos-mcp-server (Container App)
│   ├── exercise4-rest-api-mcp-server (Container App)
│   └── exercise4-virtual-analyst (Container App)
├── Azure SQL Database
│   ├── sqldb-mcpworkshop (Database)
│   └── sql-mcpworkshop (Logical Server)
├── Cosmos DB Account
│   ├── cosmos-mcpworkshop (Account)
│   └── sessions-db (Database)
├── Blob Storage
│   └── stmcpworkshop (Storage Account)
├── Application Insights
│   ├── ai-mcpworkshop (Insights)
│   └── law-mcpworkshop (Log Analytics)
└── Networking
    ├── Virtual Network (VNet)
    ├── Subnets (for Container Apps)
    └── Private Endpoints (for SQL, Cosmos, Blob)
```

### Terraform Module Organization

```text
infrastructure/terraform/
├── modules/
│   ├── container-apps/       # Container Apps Environment + Apps
│   ├── sql-database/          # Azure SQL configuration
│   ├── cosmos-db/             # Cosmos DB Serverless
│   ├── blob-storage/          # Blob Storage with containers
│   ├── monitoring/            # App Insights + Log Analytics
│   └── networking/            # VNet, subnets, NSGs
└── environments/
    ├── workshop/              # Workshop deployment (non-prod)
    │   ├── main.tf
    │   ├── variables.tf
    │   ├── outputs.tf
    │   └── terraform.tfvars
    ├── dev/                   # Dev environment (optional)
    └── prod/                  # Production example (optional)
```

---

## 🔒 Security Configuration

### 1. Network Security

**Private Endpoints** (enabled by default for prod):

```hcl
# En terraform.tfvars:
enable_private_endpoints = true  # SQL, Cosmos, Blob no son accesibles públicamente
```

**VNet Integration**:

-   Container Apps en subnet dedicada (10.0.1.0/24)
-   SQL en subnet con Service Endpoint (10.0.2.0/24)
-   Cosmos/Blob con Private Link

### 2. Secrets Management

**Azure Key Vault** (opcional, para producción):

```powershell
# Crear Key Vault
az keyvault create \
  --name kv-mcpworkshop \
  --resource-group rg-mcp-workshop-eastus2 \
  --location eastus2

# Almacenar connection strings
az keyvault secret set \
  --vault-name kv-mcpworkshop \
  --name "SqlConnectionString" \
  --value "<connection-string>"

# Referenciar desde Container Apps
az containerapp update \
  --name exercise4-sql-mcp-server \
  --secrets sql-conn=keyvaultref:https://kv-mcpworkshop.vault.azure.net/secrets/SqlConnectionString
```

### 3. Authentication & Authorization

**Managed Identity** (recomendado):

```hcl
# En Terraform module container-apps/main.tf:
identity {
  type = "SystemAssigned"
}

# Grant permissions to SQL
resource "azurerm_sql_active_directory_administrator" "admin" {
  login               = azurerm_container_app.sql_mcp_server.identity[0].principal_id
  object_id           = azurerm_container_app.sql_mcp_server.identity[0].principal_id
}
```

### 4. Firewall Rules

**SQL Server Firewall**:

```hcl
# Permitir Azure services (Container Apps):
resource "azurerm_sql_firewall_rule" "allow_azure_services" {
  name                = "AllowAzureServices"
  resource_group_name = azurerm_resource_group.workshop.name
  server_name         = azurerm_sql_server.sql.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

# Para workshop: permitir IPs de asistentes (temporalmente)
resource "azurerm_sql_firewall_rule" "workshop_ips" {
  count               = length(var.workshop_allowed_ips)
  name                = "WorkshopIP-${count.index}"
  resource_group_name = azurerm_resource_group.workshop.name
  server_name         = azurerm_sql_server.sql.name
  start_ip_address    = var.workshop_allowed_ips[count.index]
  end_ip_address      = var.workshop_allowed_ips[count.index]
}
```

---

## 📊 Monitoring & Diagnostics

### Application Insights Integration

Todos los servidores envían telemetría a Application Insights:

```csharp
// Ya configurado en Program.cs:
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

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

Todos los Container Apps exponen endpoint `/health`:

```powershell
# Verificar health de todos los servicios:
$urls = terraform output -json container_app_urls | ConvertFrom-Json

$urls.PSObject.Properties | ForEach-Object {
    $name = $_.Name
    $url = $_.Value + "/health"

    try {
        $response = Invoke-RestMethod -Uri $url -TimeoutSec 5
        Write-Host "✅ $name is healthy" -ForegroundColor Green
    } catch {
        Write-Host "❌ $name is DOWN: $_" -ForegroundColor Red
    }
}
```

---

## 🔄 Continuous Deployment (Optional)

### GitHub Actions Workflow

Crear `.github/workflows/deploy-azure.yml`:

```yaml
name: Deploy to Azure

on:
    push:
        branches: [main]
    workflow_dispatch:

jobs:
    deploy:
        runs-on: ubuntu-latest
        steps:
            - uses: actions/checkout@v3

            - name: Azure Login
              uses: azure/login@v1
              with:
                  creds: ${{ secrets.AZURE_CREDENTIALS }}

            - name: Setup Terraform
              uses: hashicorp/setup-terraform@v2
              with:
                  terraform_version: 1.5.0

            - name: Terraform Init
              run: |
                  cd infrastructure/terraform/environments/workshop
                  terraform init

            - name: Terraform Apply
              run: |
                  cd infrastructure/terraform/environments/workshop
                  terraform apply -auto-approve

            - name: Build Docker Images
              run: |
                  .\scripts\build-docker-images.ps1

            - name: Push to ACR
              run: |
                  az acr login --name ${{ secrets.ACR_NAME }}
                  .\scripts\push-all-images.ps1 -AcrName ${{ secrets.ACR_NAME }}

            - name: Update Container Apps
              run: |
                  .\scripts\update-container-apps.ps1
```

**Secrets necesarios en GitHub**:

-   `AZURE_CREDENTIALS`: Service Principal JSON
-   `ACR_NAME`: Nombre del Azure Container Registry

---

## 🧹 Cleanup & Teardown

### Remove All Resources

```powershell
# Opción A: Terraform destroy (RECOMENDADO)
cd infrastructure\terraform\environments\workshop
terraform destroy

# Confirmar cuando pregunte: "yes"
# Tiempo estimado: 10-15 minutos

# Opción B: Delete Resource Group (más rápido pero menos seguro)
az group delete --name rg-mcp-workshop-eastus2 --yes --no-wait
```

### Cost Optimization

**Para workshops de 1 día**:

1. **Scale to Zero** después del workshop:

    ```powershell
    az containerapp update --name exercise1-static-resources --min-replicas 0
    ```

2. **Pause SQL Database**:

    ```powershell
    az sql db pause --name sqldb-mcpworkshop --server sql-mcpworkshop
    ```

3. **Disable Cosmos DB** (no soporta pause, pero escala a 0):
    - Cosmos Serverless ya escala automáticamente
    - Solo pagas por RU/s consumidos

---

## 🐛 Troubleshooting

### Issue: Terraform Init Fails

**Error**:

```
Error: Failed to get existing workspaces: containers.Client#ListBlobs: Failure responding to request
```

**Solución**:

```powershell
# Verificar permisos en Storage Account del backend
az storage account show --name <backend-storage> --query "id"

# Re-autenticar:
az login
az account set --subscription "<subscription-id>"
```

---

### Issue: Container App Not Responding

**Diagnóstico**:

```powershell
# Ver logs en tiempo real:
az containerapp logs show \
  --name exercise1-static-resources \
  --resource-group rg-mcp-workshop-eastus2 \
  --follow

# Revisar replica count:
az containerapp revision list \
  --name exercise1-static-resources \
  --resource-group rg-mcp-workshop-eastus2
```

**Solución**:

```powershell
# Restart container app:
az containerapp revision restart \
  --name exercise1-static-resources \
  --resource-group rg-mcp-workshop-eastus2 \
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
# Verificar firewall rules:
az sql server firewall-rule list \
  --resource-group rg-mcp-workshop-eastus2 \
  --server sql-mcpworkshop

# Test connectivity desde Container App:
az containerapp exec \
  --name exercise4-sql-mcp-server \
  --resource-group rg-mcp-workshop-eastus2 \
  --command "/bin/sh" -- -c "nc -zv sql-mcpworkshop.database.windows.net 1433"
```

**Solución**:

```powershell
# Agregar regla para Container Apps subnet:
az sql server firewall-rule create \
  --resource-group rg-mcp-workshop-eastus2 \
  --server sql-mcpworkshop \
  --name AllowContainerApps \
  --start-ip-address 10.0.1.0 \
  --end-ip-address 10.0.1.255
```

---

## 📚 Additional Resources

| Resource                  | URL                                                                         |
| ------------------------- | --------------------------------------------------------------------------- |
| Azure Container Apps Docs | https://learn.microsoft.com/azure/container-apps/                           |
| Terraform Azure Provider  | https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs       |
| Azure SQL Best Practices  | https://learn.microsoft.com/azure/azure-sql/database/security-best-practice |
| Cosmos DB Serverless      | https://learn.microsoft.com/azure/cosmos-db/serverless                      |

---

**Deployment Complete!** 🎉

Para soporte adicional, consultar [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) o abrir issue en GitHub.
