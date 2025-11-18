# MCP Workshop - Root Terraform Configuration
# Purpose: Orchestrate all infrastructure modules for Azure deployment
# Based on: research.md section 2 - Infrastructure architecture

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }

  backend "azurerm" {
    # Backend configuration provided via backend-config file
    # Example: terraform init -backend-config=environments/dev/backend.hcl
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }

    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
}

# Resource Group
resource "azurerm_resource_group" "mcp_workshop" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}

# Monitoring Module (Log Analytics + Application Insights)
module "monitoring" {
  source = "./modules/monitoring"

  workspace_name      = "${var.name_prefix}-law"
  app_insights_name   = "${var.name_prefix}-ai"
  resource_group_name = azurerm_resource_group.mcp_workshop.name
  location            = var.location
  retention_days      = var.log_retention_days

  tags = var.tags
}

# Storage Module (Blob Storage for sample data)
module "storage" {
  source = "./modules/storage"

  storage_account_name = "${var.name_prefix}storage"
  resource_group_name  = azurerm_resource_group.mcp_workshop.name
  location             = var.location
  replication_type     = var.storage_replication_type
  upload_sample_data   = var.upload_sample_data
  cors_allowed_origins = var.cors_allowed_origins

  tags = var.tags
}

# SQL Database Module
module "sql_database" {
  source = "./modules/sql-database"

  server_name             = "${var.name_prefix}-sqlserver"
  database_name           = var.sql_database_name
  resource_group_name     = azurerm_resource_group.mcp_workshop.name
  location                = var.location
  admin_username          = var.sql_admin_username
  admin_password          = var.sql_admin_password
  azuread_admin_login     = var.azuread_admin_login
  azuread_admin_object_id = var.azuread_admin_object_id
  enable_public_access    = var.sql_enable_public_access
  sku_name                = var.sql_sku_name
  seed_sample_data        = var.seed_sample_data

  tags = var.tags
}

# Cosmos DB Module
module "cosmos_db" {
  source = "./modules/cosmos-db"

  account_name        = "${var.name_prefix}-cosmos"
  database_name       = var.cosmos_database_name
  resource_group_name = azurerm_resource_group.mcp_workshop.name
  location            = var.location
  consistency_level   = var.cosmos_consistency_level
  seed_sample_data    = var.seed_sample_data

  tags = var.tags
}

# Container Apps Module (MCP Servers)
module "container_apps" {
  source = "./modules/container-apps"

  environment_name           = "${var.name_prefix}-env"
  name_prefix                = var.name_prefix
  resource_group_name        = azurerm_resource_group.mcp_workshop.name
  location                   = var.location
  log_analytics_workspace_id = module.monitoring.workspace_id
  environment                = var.environment

  # SQL MCP Server
  enable_sql_server     = var.enable_sql_server
  sql_connection_string = module.sql_database.connection_string

  # Cosmos MCP Server
  enable_cosmos_server = var.enable_cosmos_server
  cosmos_endpoint      = module.cosmos_db.endpoint
  cosmos_key           = module.cosmos_db.primary_key
  cosmos_database_id   = var.cosmos_database_name

  # REST MCP Server
  enable_rest_server = var.enable_rest_server

  # Virtual Analyst
  enable_virtual_analyst = var.enable_virtual_analyst

  # Authentication
  jwt_secret   = var.jwt_secret
  jwt_issuer   = var.jwt_issuer
  jwt_audience = var.jwt_audience

  # Monitoring
  app_insights_connection_string = module.monitoring.app_insights_connection_string

  tags = var.tags
}
