# Outputs for Azure Container Apps Module

output "environment_id" {
  description = "ID of the Container Apps Environment"
  value       = azurerm_container_app_environment.mcp_workshop.id
}

output "environment_name" {
  description = "Name of the Container Apps Environment"
  value       = azurerm_container_app_environment.mcp_workshop.name
}

output "environment_default_domain" {
  description = "Default domain of the Container Apps Environment"
  value       = azurerm_container_app_environment.mcp_workshop.default_domain
}

output "sql_mcp_server_url" {
  description = "FQDN of the SQL MCP Server"
  value       = var.enable_sql_server ? "https://${azurerm_container_app.sql_mcp_server[0].latest_revision_fqdn}" : null
}

output "cosmos_mcp_server_url" {
  description = "FQDN of the Cosmos MCP Server"
  value       = var.enable_cosmos_server ? "https://${azurerm_container_app.cosmos_mcp_server[0].latest_revision_fqdn}" : null
}

output "rest_mcp_server_url" {
  description = "FQDN of the REST MCP Server"
  value       = var.enable_rest_server ? "https://${azurerm_container_app.rest_mcp_server[0].latest_revision_fqdn}" : null
}

output "virtual_analyst_url" {
  description = "FQDN of the Virtual Analyst Orchestrator"
  value       = var.enable_virtual_analyst ? "https://${azurerm_container_app.virtual_analyst[0].latest_revision_fqdn}" : null
}

output "sql_mcp_server_id" {
  description = "Resource ID of the SQL MCP Server Container App"
  value       = var.enable_sql_server ? azurerm_container_app.sql_mcp_server[0].id : null
}

output "cosmos_mcp_server_id" {
  description = "Resource ID of the Cosmos MCP Server Container App"
  value       = var.enable_cosmos_server ? azurerm_container_app.cosmos_mcp_server[0].id : null
}

output "rest_mcp_server_id" {
  description = "Resource ID of the REST MCP Server Container App"
  value       = var.enable_rest_server ? azurerm_container_app.rest_mcp_server[0].id : null
}

output "virtual_analyst_id" {
  description = "Resource ID of the Virtual Analyst Container App"
  value       = var.enable_virtual_analyst ? azurerm_container_app.virtual_analyst[0].id : null
}
