# Root Terraform Outputs for MCP Workshop

output "resource_suffix" {
  description = "Random suffix used for globally unique resource names"
  value       = local.suffix
}

output "resource_group_name" {
  description = "Name of the resource group (with suffix)"
  value       = azurerm_resource_group.mcp_workshop.name
}

output "location" {
  description = "Azure region"
  value       = var.location
}

# Monitoring Outputs
output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = module.monitoring.workspace_id
}

output "application_insights_instrumentation_key" {
  description = "Application Insights Instrumentation Key"
  value       = module.monitoring.app_insights_instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Application Insights Connection String"
  value       = module.monitoring.app_insights_connection_string
  sensitive   = true
}

# Storage Outputs
output "storage_account_name" {
  description = "Storage account name"
  value       = module.storage.storage_account_name
}

output "storage_primary_blob_endpoint" {
  description = "Primary blob storage endpoint"
  value       = module.storage.primary_blob_endpoint
}

# SQL Database Outputs
output "sql_server_fqdn" {
  description = "SQL Server fully qualified domain name"
  value       = module.sql_database.server_fqdn
}

output "sql_database_name" {
  description = "SQL Database name"
  value       = module.sql_database.database_name
}

output "sql_connection_string" {
  description = "SQL Database connection string"
  value       = module.sql_database.connection_string
  sensitive   = true
}

# Cosmos DB Outputs
output "cosmos_endpoint" {
  description = "Cosmos DB endpoint URL"
  value       = module.cosmos_db.endpoint
}

output "cosmos_database_name" {
  description = "Cosmos DB database name"
  value       = module.cosmos_db.database_name
}

output "cosmos_primary_key" {
  description = "Cosmos DB primary key"
  value       = module.cosmos_db.primary_key
  sensitive   = true
}

# Container Apps Outputs
output "sql_mcp_server_url" {
  description = "URL of the SQL MCP Server"
  value       = module.container_apps.sql_mcp_server_url
}

output "cosmos_mcp_server_url" {
  description = "URL of the Cosmos MCP Server"
  value       = module.container_apps.cosmos_mcp_server_url
}

output "rest_mcp_server_url" {
  description = "URL of the REST MCP Server"
  value       = module.container_apps.rest_mcp_server_url
}

output "virtual_analyst_url" {
  description = "URL of the Virtual Analyst Orchestrator"
  value       = module.container_apps.virtual_analyst_url
}

# Summary Output
output "deployment_summary" {
  description = "Summary of deployed resources"
  value = {
    resource_group = azurerm_resource_group.mcp_workshop.name
    location       = var.location
    environment    = var.environment
    mcp_servers = {
      sql_server      = module.container_apps.sql_mcp_server_url
      cosmos_server   = module.container_apps.cosmos_mcp_server_url
      rest_server     = module.container_apps.rest_mcp_server_url
      virtual_analyst = module.container_apps.virtual_analyst_url
    }
  }
}
