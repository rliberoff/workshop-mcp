# MCP Workshop - Azure Infrastructure

This directory contains Terraform Infrastructure-as-Code for deploying the MCP Workshop environment to Azure, including all MCP servers, databases, storage, and monitoring components.

## Architecture Overview

The infrastructure deploys:

-   **Azure Container Apps Environment** - Hosts all 4 MCP servers with auto-scaling

    -   SQL MCP Server (port 5010) - SQL Database resource provider
    -   Cosmos MCP Server (port 5011) - Cosmos DB resource provider
    -   REST MCP Server (port 5012) - External REST API tool provider
    -   VirtualAnalyst Orchestrator (port 5004) - Multi-server orchestration engine

-   **Azure SQL Database** - Relational data storage with security features

    -   TLS 1.2 minimum encryption
    -   Azure AD authentication support
    -   Optional auditing and threat protection
    -   Sample data seeding for exercises

-   **Azure Cosmos DB** - NoSQL document storage

    -   Serverless capability for cost efficiency
    -   Two containers: sessions, cart_events (with TTL)
    -   Geo-replication support for production

-   **Azure Blob Storage** - Static resource storage

    -   Sample customer data (customers.json)
    -   CORS enabled for workshop scenarios

-   **Azure Monitor** - Observability and diagnostics
    -   Log Analytics Workspace for centralized logging
    -   Application Insights for distributed tracing

## Prerequisites

1. **Azure Subscription** - Active Azure subscription with Contributor permissions
2. **Azure CLI** - Version 2.50+ ([Install](https://learn.microsoft.com/cli/azure/install-azure-cli))
3. **Terraform** - Version 1.5.0+ ([Install](https://www.terraform.io/downloads))
4. **PowerShell** - Version 7+ recommended ([Install](https://learn.microsoft.com/powershell/scripting/install/installing-powershell))

## Quick Start

### 1. Authenticate to Azure

```powershell
az login
az account set --subscription "<your-subscription-id>"
```

### 2. Generate Secrets (First-Time Only)

The deployment script will auto-generate `secrets.auto.tfvars` on first run:

```powershell
# Auto-generated secrets:
# - jwt_secret: 64-character random string
# - sql_admin_password: 20-character password with special characters
```

**Important:** After generation, edit `secrets.auto.tfvars` to add Azure AD admin details:

```hcl
azuread_admin_login = "your-admin-email@domain.com"
azuread_admin_object_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

To get your Azure AD Object ID:

```powershell
az ad user show --id your-admin-email@domain.com --query id -o tsv
```

### 3. Deploy Dev Environment

```powershell
cd infrastructure
.\scripts\deploy.ps1 -Environment dev
```

The script will:

-   ✅ Validate Terraform and Azure CLI installation
-   ✅ Check Azure authentication
-   ✅ Generate secrets if missing
-   ✅ Initialize Terraform with backend
-   ✅ Validate configuration syntax
-   ✅ Create execution plan
-   ⚠️ Prompt for confirmation before applying
-   ✅ Deploy all resources
-   ✅ Save outputs to `outputs-dev.json`

**Auto-approve (skip confirmation):**

```powershell
.\scripts\deploy.ps1 -Environment dev -AutoApprove
```

### 4. Retrieve Deployment Outputs

After deployment, get MCP server URLs:

```powershell
# View all outputs
terraform output -json

# Get deployment summary
terraform output -json deployment_summary | ConvertFrom-Json | ConvertTo-Json -Depth 10

# Example output:
# {
#   "mcp_servers": {
#     "sql_mcp": "https://sql-mcp-server-dev.azurecontainerapps.io",
#     "cosmos_mcp": "https://cosmos-mcp-server-dev.azurecontainerapps.io",
#     "rest_mcp": "https://rest-mcp-server-dev.azurecontainerapps.io",
#     "virtual_analyst": "https://virtual-analyst-dev.azurecontainerapps.io"
#   }
# }
```

### 5. Build and Deploy Container Images

After infrastructure is provisioned, build Docker images for each exercise:

```powershell
# Example: Build SQL MCP Server image
cd src/Exercises/Exercise1/SqlMcpServer
docker build -t mcpworkshop.azurecr.io/sql-mcp-server:latest .

# Push to Azure Container Registry (requires ACR credentials)
az acr login --name mcpworkshop
docker push mcpworkshop.azurecr.io/sql-mcp-server:latest

# Update Container App to use new image
az containerapp update \
  --name sql-mcp-server-dev \
  --resource-group rg-mcpworkshop-dev \
  --image mcpworkshop.azurecr.io/sql-mcp-server:latest
```

**Note:** Repeat for all 4 MCP servers (sql-mcp-server, cosmos-mcp-server, rest-mcp-server, virtual-analyst).

### 6. Teardown Environment

To destroy all resources after the workshop:

```powershell
.\scripts\teardown.ps1 -Environment dev
```

The script will:

-   ⚠️ Prompt for confirmation (type "destroy" to confirm)
-   📋 List all resources to be destroyed
-   🗑️ Execute Terraform destroy
-   🧹 Clean up plan files

**Force teardown (skip confirmation):**

```powershell
.\scripts\teardown.ps1 -Environment dev -Force
```

**Preserve Log Analytics (keep monitoring data):**

```powershell
.\scripts\teardown.ps1 -Environment dev -KeepLogs
```

## Directory Structure

```
infrastructure/
├── terraform/
│   ├── main.tf              # Root module - orchestrates all sub-modules
│   ├── variables.tf         # Root variables (40+ configuration parameters)
│   ├── outputs.tf           # Root outputs (MCP URLs, connection strings)
│   ├── secrets.auto.tfvars  # Auto-generated secrets (git-ignored)
│   │
│   ├── modules/
│   │   ├── container-apps/  # Azure Container Apps hosting
│   │   │   ├── main.tf      # 4 Container Apps + shared environment
│   │   │   ├── variables.tf # Image names, connection strings, JWT config
│   │   │   └── outputs.tf   # MCP server URLs and resource IDs
│   │   │
│   │   ├── sql-database/    # Azure SQL Database
│   │   │   ├── main.tf      # SQL Server, database, firewall, auditing
│   │   │   ├── variables.tf # Server config, admin credentials, SKU
│   │   │   └── outputs.tf   # Connection strings (SQL + Azure AD)
│   │   │
│   │   ├── cosmos-db/       # Azure Cosmos DB
│   │   │   ├── main.tf      # Cosmos account, database, 2 containers
│   │   │   ├── variables.tf # Consistency level, geo-replication, TTL
│   │   │   └── outputs.tf   # Endpoint, keys, connection strings
│   │   │
│   │   ├── storage/         # Azure Blob Storage
│   │   │   ├── main.tf      # Storage account, container, sample data
│   │   │   ├── variables.tf # Replication type, CORS, upload config
│   │   │   └── outputs.tf   # Blob endpoint, access key
│   │   │
│   │   └── monitoring/      # Azure Monitor
│   │       ├── main.tf      # Log Analytics, Application Insights
│   │       ├── variables.tf # Retention days, workspace config
│   │       └── outputs.tf   # Workspace ID, instrumentation key
│   │
│   └── environments/
│       ├── dev/
│       │   └── terraform.tfvars  # Dev config (Basic SKU, LRS, free tier)
│       └── prod/
│           └── terraform.tfvars  # Prod config (S1 SKU, GRS, no free tier)
│
└── scripts/
    ├── deploy.ps1    # Deployment orchestration (validate, plan, apply)
    └── teardown.ps1  # Teardown orchestration (destroy, cleanup)
```

## Configuration

### Environment-Specific Variables

**Dev Environment** (`environments/dev/terraform.tfvars`):

-   SQL SKU: `Basic` (5 DTU, 2 GB)
-   Storage Replication: `LRS` (Locally Redundant)
-   Cosmos Free Tier: Enabled
-   Log Retention: 7 days
-   Sample Data Seeding: Enabled
-   Public Access: Enabled (for workshop connectivity)

**Prod Environment** (`environments/prod/terraform.tfvars`):

-   SQL SKU: `S1` (20 DTU, 250 GB)
-   Storage Replication: `GRS` (Geo-Redundant)
-   Cosmos Free Tier: Disabled
-   Log Retention: 90 days
-   Sample Data Seeding: Disabled
-   Public Access: Disabled (private endpoints recommended)

### Key Variables

| Variable                 | Description                     | Default        | Required        |
| ------------------------ | ------------------------------- | -------------- | --------------- |
| `name_prefix`            | Prefix for all resource names   | `mcpworkshop`  | Yes             |
| `environment`            | Environment name (dev/prod)     | -              | Yes             |
| `location`               | Azure region                    | `westeurope`   | Yes             |
| `sql_admin_username`     | SQL admin username              | `mcpadmin`     | Yes             |
| `sql_admin_password`     | SQL admin password              | Auto-generated | Yes (sensitive) |
| `jwt_secret`             | JWT signing secret              | Auto-generated | Yes (sensitive) |
| `jwt_issuer`             | JWT issuer claim                | `mcp-workshop` | Yes             |
| `jwt_audience`           | JWT audience claim              | `mcp-servers`  | Yes             |
| `enable_sql_server`      | Deploy SQL MCP Server           | `true`         | No              |
| `enable_cosmos_server`   | Deploy Cosmos MCP Server        | `true`         | No              |
| `enable_rest_server`     | Deploy REST MCP Server          | `true`         | No              |
| `enable_virtual_analyst` | Deploy VirtualAnalyst           | `true`         | No              |
| `seed_sample_data`       | Load sample data into databases | `true` (dev)   | No              |

### Customization

To override defaults, edit the environment-specific `terraform.tfvars` file:

```hcl
# environments/dev/terraform.tfvars
name_prefix = "myworkshop"
location = "eastus"
sql_sku_name = "S0"  # Upgrade from Basic to S0 (10 DTU)
log_retention_days = 30  # Increase retention from 7 to 30 days
```

## Security Best Practices

1. **Secrets Management**

    - Never commit `secrets.auto.tfvars` to version control (already in `.gitignore`)
    - Rotate JWT secret and SQL password after workshops
    - Use Azure Key Vault for production deployments

2. **Azure AD Authentication**

    - Always configure Azure AD admin for SQL Database
    - Prefer Azure AD authentication over SQL authentication in production
    - Use managed identities for Container Apps in production

3. **Network Security**

    - Dev environment allows public access for workshop convenience
    - Prod environment should use private endpoints (not implemented yet)
    - Configure firewall rules to allow only workshop IP ranges

4. **Monitoring and Auditing**
    - Enable SQL Database auditing in production (`enable_auditing = true`)
    - Enable threat protection for sensitive workloads (`enable_threat_protection = true`)
    - Review Log Analytics logs regularly

## Terraform State Management

Currently, Terraform state is stored **locally** in `terraform.tfstate`.

**For team collaboration or production**, configure remote backend in `main.tf`:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "stterraformstate"
    container_name       = "tfstate"
    key                  = "mcpworkshop.tfstate"
  }
}
```

Initialize with backend configuration:

```powershell
terraform init -backend-config="key=mcpworkshop-dev.tfstate"
```

## Cost Estimation

**Dev Environment (per month):**

-   Container Apps: ~$20 (0.5 vCPU, 1 GB RAM × 4 servers)
-   SQL Database (Basic): ~$5
-   Cosmos DB (Serverless): ~$1 (free tier eligible)
-   Blob Storage (LRS): <$1
-   Log Analytics: ~$3 (7-day retention)
-   **Total: ~$30/month**

**Prod Environment (per month):**

-   Container Apps: ~$80 (1 vCPU, 2 GB RAM × 4 servers)
-   SQL Database (S1): ~$30
-   Cosmos DB (Serverless): ~$25 (no free tier)
-   Blob Storage (GRS): ~$5
-   Log Analytics: ~$50 (90-day retention)
-   **Total: ~$190/month**

**Cost Optimization Tips:**

-   Teardown dev environments when not in use
-   Use Azure Dev/Test pricing for non-production
-   Enable auto-shutdown for Container Apps during off-hours
-   Use Cosmos DB free tier (one per subscription)

## Troubleshooting

### Terraform Errors

**Error: "Backend initialization required"**

```powershell
terraform init -reconfigure
```

**Error: "Resource already exists"**

```powershell
# Import existing resource
terraform import azurerm_resource_group.workshop /subscriptions/.../resourceGroups/rg-mcpworkshop-dev
```

**Error: "Invalid credentials"**

```powershell
# Re-authenticate
az login
az account set --subscription "<your-subscription-id>"
```

### Container App Errors

**Error: "Container image pull failed"**

-   Check Azure Container Registry credentials
-   Verify image exists: `az acr repository show-tags --name mcpworkshop --repository sql-mcp-server`
-   Rebuild and push image

**Error: "Container app not responding"**

-   Check logs: `az containerapp logs show --name sql-mcp-server-dev --resource-group rg-mcpworkshop-dev`
-   Verify environment variables are set correctly
-   Check Application Insights for exceptions

### Database Connection Errors

**Error: "Cannot connect to SQL Database"**

-   Verify firewall rules: `az sql server firewall-rule list --server <server-name> --resource-group <rg-name>`
-   Check connection string in Container App environment variables
-   Test connection from Azure Cloud Shell:
    ```powershell
    sqlcmd -S <server-fqdn> -U mcpadmin -P <password> -d mcpworkshop -Q "SELECT @@VERSION"
    ```

**Error: "Cosmos DB authentication failed"**

-   Verify Cosmos DB key in Container App environment variables
-   Test connection with Azure Cosmos DB Data Explorer
-   Check endpoint URL format: `https://<account-name>.documents.azure.com:443/`

## Next Steps

After deploying infrastructure:

1. **Build Container Images** - Create Dockerfiles for each exercise and push to Azure Container Registry
2. **Run Integration Tests** - Execute test suites to verify MCP protocol compliance
3. **Configure Monitoring** - Set up dashboards and alerts in Application Insights
4. **Workshop Delivery** - Use deployed URLs in workshop exercises

## Support

For issues or questions:

-   Check [Terraform Azure Provider documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
-   Review [Azure Container Apps documentation](https://learn.microsoft.com/azure/container-apps/)
-   Consult workshop troubleshooting guide (coming in Phase 8)

## License

This infrastructure code is part of the MCP Workshop project. See root LICENSE file for details.
