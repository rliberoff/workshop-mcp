output "workspace_id" {
  value = azurerm_log_analytics_workspace.mcp_workshop.id
}

output "workspace_name" {
  value = azurerm_log_analytics_workspace.mcp_workshop.name
}

output "app_insights_id" {
  value = azurerm_application_insights.mcp_workshop.id
}

output "app_insights_instrumentation_key" {
  value     = azurerm_application_insights.mcp_workshop.instrumentation_key
  sensitive = true
}

output "app_insights_connection_string" {
  value     = azurerm_application_insights.mcp_workshop.connection_string
  sensitive = true
}
