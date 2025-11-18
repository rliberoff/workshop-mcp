# Outputs for Azure SQL Database Module

output "server_id" {
  description = "Resource ID of the SQL Server"
  value       = azurerm_mssql_server.mcp_workshop.id
}

output "server_name" {
  description = "Name of the SQL Server"
  value       = azurerm_mssql_server.mcp_workshop.name
}

output "server_fqdn" {
  description = "Fully qualified domain name of the SQL Server"
  value       = azurerm_mssql_server.mcp_workshop.fully_qualified_domain_name
}

output "database_id" {
  description = "Resource ID of the SQL Database"
  value       = azurerm_mssql_database.mcp_workshop.id
}

output "database_name" {
  description = "Name of the SQL Database"
  value       = azurerm_mssql_database.mcp_workshop.name
}

output "connection_string" {
  description = "Connection string for the SQL Database (sensitive)"
  value       = "Server=tcp:${azurerm_mssql_server.mcp_workshop.fully_qualified_domain_name},1433;Initial Catalog=${var.database_name};Persist Security Info=False;User ID=${var.admin_username};Password=${var.admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  sensitive   = true
}

output "connection_string_azuread" {
  description = "Connection string for Azure AD authentication"
  value       = "Server=tcp:${azurerm_mssql_server.mcp_workshop.fully_qualified_domain_name},1433;Initial Catalog=${var.database_name};Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
