# Production Environment Configuration

resource_group_name = "rg-mcpworkshop-prod"
location            = "westeurope"
environment         = "prod"
name_prefix         = "mcpworkshop-prod"

# Monitoring
log_retention_days = 90

# Storage
storage_replication_type = "GRS"
upload_sample_data       = false

# SQL Database
sql_database_name        = "mcpworkshop-prod"
sql_admin_username       = "mcpadmin"
sql_enable_public_access = false
sql_sku_name             = "S1"

# Cosmos DB
cosmos_database_name     = "mcpworkshop-prod"
cosmos_consistency_level = "Session"
cosmos_enable_free_tier  = false

# MCP Servers
enable_sql_server      = true
enable_cosmos_server   = true
enable_rest_server     = true
enable_virtual_analyst = true

# Data Seeding
seed_sample_data = false

# Tags
tags = {
  Project     = "MCP Workshop"
  Environment = "Production"
  ManagedBy   = "Terraform"
  Owner       = "Workshop Team"
  CostCenter  = "Training"
}
