# Azure Log Analytics Module for MCP Workshop

resource "azurerm_log_analytics_workspace" "mcp_workshop" {
  name                = var.workspace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.retention_days

  tags = merge(var.tags, { Module = "monitoring" })
}

resource "azurerm_application_insights" "mcp_workshop" {
  name                = var.app_insights_name
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.mcp_workshop.id
  application_type    = "web"

  tags = merge(var.tags, { Module = "monitoring" })
}
