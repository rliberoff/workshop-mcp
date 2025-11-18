# Development Environment Configuration

resource_group_name = "rg-mcpworkshop-dev"
location            = "westeurope"
environment         = "dev"
name_prefix         = "mcpworkshop-dev"

# Monitoring
log_retention_days = 7

# Storage
storage_replication_type = "LRS"
upload_sample_data       = true

# SQL Database
sql_database_name        = "mcpworkshop-dev"
sql_admin_username       = "mcpadmin"
sql_enable_public_access = true
sql_sku_name             = "Basic"

# Cosmos DB
cosmos_database_name     = "mcpworkshop-dev"
cosmos_consistency_level = "Session"
cosmos_enable_free_tier  = true

# MCP Servers
enable_sql_server      = true
enable_cosmos_server   = true
enable_rest_server     = true
enable_virtual_analyst = true

# Data Seeding
seed_sample_data = true

# Tags
tags = {
  Project     = "MCP Workshop"
  Environment = "Development"
  ManagedBy   = "Terraform"
  Owner       = "Workshop Team"
}
