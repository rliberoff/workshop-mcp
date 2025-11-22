# MCP Workshop - Root Terraform Configuration
# Purpose: Orchestrate all infrastructure modules for Azure deployment
# Based on: research.md section 2 - Infrastructure architecture

terraform {
  required_version = ">= 1.14.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.54.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.7.2"
    }
  }

  # Using local backend for workshop simplicity
  # For production, consider using azurerm backend with Azure Storage
  # backend "azurerm" {
  #   resource_group_name  = "rg-terraform-state"
  #   storage_account_name = "sttfstatemcpworkshop"
  #   container_name       = "tfstate"
  #   key                  = "workshop.terraform.tfstate"
  # }
}

provider "azurerm" {
  features {
    app_configuration {
      purge_soft_delete_on_destroy = true
    }
    cognitive_account {
      purge_soft_delete_on_destroy = true
    }
    log_analytics_workspace {
      permanently_delete_on_destroy = true
    }
    key_vault {
      purge_soft_deleted_secrets_on_destroy = true
    }
    resource_group {
      # This flag is set to mitigate an open bug in Terraform.
      # For instance, the Resource Group is not deleted when a `Failure Anomalies` resource is present.
      # As soon as this is fixed, we should remove this.
      # Reference: https://github.com/hashicorp/terraform-provider-azurerm/issues/18026
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Generate random suffix for globally unique resource names
resource "random_id" "suffix" {
  byte_length = 4
}

locals {
  # Use random suffix or custom suffix
  suffix      = lower(var.use_random_suffix ? random_id.suffix.hex : var.suffix)
  name_suffix = local.suffix != "" ? "-${local.suffix}" : ""

  # Resource names with suffix
  resource_group_name  = "${var.resource_group_name}${local.name_suffix}"
  storage_account_name = lower(replace("${var.name_prefix}st${local.suffix}", "-", ""))
  sql_server_name      = "${var.name_prefix}-sql${local.name_suffix}"
  cosmos_account_name  = "${var.name_prefix}-cosmos${local.name_suffix}"
}

# Resource Group
resource "azurerm_resource_group" "mcp_workshop" {
  name     = local.resource_group_name
  location = var.location

  tags = merge(var.tags, {
    Suffix = local.suffix
  })
}

# Monitoring Module (Log Analytics + Application Insights)
module "monitoring" {
  source = "./modules/monitoring"

  workspace_name      = "${var.name_prefix}-law${local.name_suffix}"
  app_insights_name   = "${var.name_prefix}-ai${local.name_suffix}"
  resource_group_name = azurerm_resource_group.mcp_workshop.name
  location            = var.location
  retention_days      = var.log_retention_days

  tags = var.tags
}

# Storage Module (Blob Storage for sample data)
module "storage" {
  source = "./modules/storage"

  storage_account_name = local.storage_account_name
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

  server_name             = local.sql_server_name
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

  account_name        = local.cosmos_account_name
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

  environment_name           = "${var.name_prefix}-env${local.name_suffix}"
  name_prefix                = "${var.name_prefix}${local.name_suffix}"
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
