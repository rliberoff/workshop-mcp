# Outputs for Azure Cosmos DB Module

output "account_id" {
  description = "Resource ID of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.mcp_workshop.id
}

output "account_name" {
  description = "Name of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.mcp_workshop.name
}

output "endpoint" {
  description = "Endpoint URL of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.mcp_workshop.endpoint
}

output "primary_key" {
  description = "Primary master key for Cosmos DB account (sensitive)"
  value       = azurerm_cosmosdb_account.mcp_workshop.primary_key
  sensitive   = true
}

output "secondary_key" {
  description = "Secondary master key for Cosmos DB account (sensitive)"
  value       = azurerm_cosmosdb_account.mcp_workshop.secondary_key
  sensitive   = true
}

output "connection_strings" {
  description = "Connection strings for Cosmos DB account (sensitive)"
  value       = azurerm_cosmosdb_account.mcp_workshop.connection_strings
  sensitive   = true
}

output "database_id" {
  description = "Resource ID of the Cosmos DB database"
  value       = azurerm_cosmosdb_sql_database.mcp_workshop.id
}

output "database_name" {
  description = "Name of the Cosmos DB database"
  value       = azurerm_cosmosdb_sql_database.mcp_workshop.name
}

output "sessions_container_id" {
  description = "Resource ID of the sessions container"
  value       = azurerm_cosmosdb_sql_container.sessions.id
}

output "cart_events_container_id" {
  description = "Resource ID of the cart_events container"
  value       = azurerm_cosmosdb_sql_container.cart_events.id
}
