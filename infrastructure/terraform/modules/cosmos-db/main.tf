# Azure Cosmos DB Module for MCP Workshop
# Purpose: NoSQL database for Exercise 4 Cosmos MCP Server
# Based on: research.md - Azure Cosmos DB for NoSQL scenarios

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

# Cosmos DB Account
resource "azurerm_cosmosdb_account" "mcp_workshop" {
  name                = var.account_name
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level       = var.consistency_level
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  dynamic "geo_location" {
    for_each = var.secondary_locations
    content {
      location          = geo_location.value.location
      failover_priority = geo_location.value.failover_priority
    }
  }

  tags = merge(
    var.tags,
    {
      Purpose = "MCP Workshop - NoSQL Database"
      Module  = "cosmos-db"
    }
  )
}

# Cosmos DB SQL Database
resource "azurerm_cosmosdb_sql_database" "mcp_workshop" {
  name                = var.database_name
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.mcp_workshop.name
}

# Container for customer sessions
resource "azurerm_cosmosdb_sql_container" "sessions" {
  name                  = "sessions"
  resource_group_name   = var.resource_group_name
  account_name          = azurerm_cosmosdb_account.mcp_workshop.name
  database_name         = azurerm_cosmosdb_sql_database.mcp_workshop.name
  partition_key_paths   = ["/userId"]
  partition_key_version = 1

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }
}

# Container for cart events
resource "azurerm_cosmosdb_sql_container" "cart_events" {
  name                  = "cart_events"
  resource_group_name   = var.resource_group_name
  account_name          = azurerm_cosmosdb_account.mcp_workshop.name
  database_name         = azurerm_cosmosdb_sql_database.mcp_workshop.name
  partition_key_paths   = ["/userId"]
  partition_key_version = 1

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }

  # TTL for automatic cleanup of old events
  default_ttl = var.cart_events_ttl_seconds
}

# Seed sample data (optional)
resource "null_resource" "seed_cosmos_data" {
  count = var.seed_sample_data ? 1 : 0

  triggers = {
    database_id = azurerm_cosmosdb_sql_database.mcp_workshop.id
  }

  provisioner "local-exec" {
    command = "pwsh -File ${path.module}/scripts/seed-cosmos.ps1 -Endpoint ${azurerm_cosmosdb_account.mcp_workshop.endpoint} -Key ${azurerm_cosmosdb_account.mcp_workshop.primary_key} -DatabaseName ${var.database_name}"
  }
}
