# Workshop Environment Configuration

Este directorio contiene la configuración de Terraform para el entorno del workshop MCP.

## Archivos

### `terraform.tfvars`

Contiene toda la configuración del entorno del workshop:

-   **Resource Group**: `rg-mcpworkshop`
-   **Location**: `westeurope`
-   **Environment**: `workshop`
-   **SQL SKU**: Basic (para costos mínimos)
-   **Cosmos DB**: Free tier habilitado
-   **Storage**: LRS replication
-   **Data Seeding**: Habilitado

### `secrets.auto.tfvars`

Este archivo es **auto-generado** por el script `deploy.ps1` y contiene:

-   `jwt_secret`: Secret para autenticación JWT (64 caracteres)
-   `sql_admin_password`: Contraseña del administrador de SQL (20 caracteres)
-   `azuread_admin_login`: Email del administrador de Azure AD (debe ser actualizado manualmente)
-   `azuread_admin_object_id`: Object ID del administrador de Azure AD (debe ser actualizado manualmente)

⚠️ **IMPORTANTE**: Este archivo está en `.gitignore` y **NO debe ser sincronizado** al repositorio.

## Uso

### Desplegar

```powershell
cd infrastructure
.\scripts\deploy.ps1
```

### Destruir

```powershell
cd infrastructure
.\scripts\teardown.ps1
```

## Personalización

Para personalizar la configuración del workshop, edita `terraform.tfvars` según tus necesidades:

```hcl
# Cambiar ubicación
location = "eastus2"

# Cambiar tier de SQL
sql_sku_name = "S1"

# Deshabilitar free tier de Cosmos
cosmos_enable_free_tier = false

# Habilitar private endpoints
enable_private_endpoints = true
```

## Costos Estimados

Con la configuración por defecto:

-   SQL Database (Basic): ~$5/mes
-   Cosmos DB (Serverless, free tier): $0-$25/mes
-   Container Apps: ~$50-100/mes
-   Storage: ~$1-5/mes
-   Application Insights: ~$0-10/mes (5GB gratis)

**Total**: ~$60-145/mes

Para reducir costos después del workshop, ejecuta:

```powershell
.\scripts\teardown.ps1
```
