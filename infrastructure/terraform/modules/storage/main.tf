# Azure Storage Module for MCP Workshop
# Purpose: Blob storage for static resources and sample data files

resource "azurerm_storage_account" "mcp_workshop" {
  name                     = var.storage_account_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = var.replication_type
  min_tls_version          = "TLS1_2"

  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["GET", "HEAD", "OPTIONS"]
      allowed_origins    = var.cors_allowed_origins
      exposed_headers    = ["*"]
      max_age_in_seconds = 3600
    }
  }

  tags = merge(var.tags, { Module = "storage" })
}

resource "azurerm_storage_container" "sample_data" {
  name                  = "sample-data"
  storage_account_name  = azurerm_storage_account.mcp_workshop.name
  container_access_type = "blob"
}

resource "azurerm_storage_blob" "customers_json" {
  count                  = var.upload_sample_data ? 1 : 0
  name                   = "customers.json"
  storage_account_name   = azurerm_storage_account.mcp_workshop.name
  storage_container_name = azurerm_storage_container.sample_data.name
  type                   = "Block"
  source                 = "${var.sample_data_path}/customers.json"
}
