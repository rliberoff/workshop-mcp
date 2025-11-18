# Azure SQL Database Module for MCP Workshop
# Purpose: Relational database for Exercise 4 SQL MCP Server
# Based on: research.md - Azure SQL Database for relational data scenarios

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

# Azure SQL Server
resource "azurerm_mssql_server" "mcp_workshop" {
  name                         = var.server_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.admin_username
  administrator_login_password = var.admin_password
  minimum_tls_version          = "1.2"

  azuread_administrator {
    login_username = var.azuread_admin_login
    object_id      = var.azuread_admin_object_id
  }

  public_network_access_enabled = var.enable_public_access

  tags = merge(
    var.tags,
    {
      Purpose = "MCP Workshop - SQL Database Server"
      Module  = "sql-database"
    }
  )
}

# Azure SQL Database
resource "azurerm_mssql_database" "mcp_workshop" {
  name           = var.database_name
  server_id      = azurerm_mssql_server.mcp_workshop.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = var.max_size_gb
  sku_name       = var.sku_name
  zone_redundant = var.zone_redundant

  tags = merge(
    var.tags,
    {
      Purpose  = "MCP Workshop - Exercise 4 Data"
      Exercise = "Exercise4"
    }
  )
}

# Firewall rule to allow Azure services
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.mcp_workshop.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Firewall rule for workshop attendees (optional)
resource "azurerm_mssql_firewall_rule" "workshop_access" {
  count            = var.workshop_ip_range != null ? 1 : 0
  name             = "WorkshopAccess"
  server_id        = azurerm_mssql_server.mcp_workshop.id
  start_ip_address = var.workshop_ip_range.start
  end_ip_address   = var.workshop_ip_range.end
}

# Transparent Data Encryption (enabled by default in Azure SQL)
# Auditing (optional for production scenarios)
resource "azurerm_mssql_server_extended_auditing_policy" "mcp_workshop" {
  count                      = var.enable_auditing ? 1 : 0
  server_id                  = azurerm_mssql_server.mcp_workshop.id
  storage_endpoint           = var.auditing_storage_endpoint
  storage_account_access_key = var.auditing_storage_key
  retention_in_days          = var.audit_retention_days
  log_monitoring_enabled     = true
}

# Advanced Threat Protection (optional for production)
resource "azurerm_mssql_server_security_alert_policy" "mcp_workshop" {
  count               = var.enable_threat_protection ? 1 : 0
  resource_group_name = var.resource_group_name
  server_name         = azurerm_mssql_server.mcp_workshop.name
  state               = "Enabled"

  email_account_admins = var.threat_protection_email_admins
  email_addresses      = var.threat_protection_emails
}

# Sample data initialization (optional)
# This would typically be done via a deployment script or Azure Data Factory
resource "null_resource" "seed_database" {
  count = var.seed_sample_data ? 1 : 0

  triggers = {
    database_id = azurerm_mssql_database.mcp_workshop.id
  }

  provisioner "local-exec" {
    command = "pwsh -File ${path.module}/scripts/seed-database.ps1 -ServerName ${azurerm_mssql_server.mcp_workshop.fully_qualified_domain_name} -DatabaseName ${var.database_name} -AdminUsername ${var.admin_username} -AdminPassword ${var.admin_password}"
  }
}
