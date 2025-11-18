output "storage_account_id" {
  value = azurerm_storage_account.mcp_workshop.id
}

output "storage_account_name" {
  value = azurerm_storage_account.mcp_workshop.name
}

output "primary_blob_endpoint" {
  value = azurerm_storage_account.mcp_workshop.primary_blob_endpoint
}

output "primary_access_key" {
  value     = azurerm_storage_account.mcp_workshop.primary_access_key
  sensitive = true
}

output "sample_data_container_name" {
  value = azurerm_storage_container.sample_data.name
}
