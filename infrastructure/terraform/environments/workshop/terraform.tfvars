# Workshop Environment Configuration

resource_group_name = "rg-mcpworkshop"
location            = "swedencentral"
environment         = "workshop"
name_prefix         = "mcpworkshop"

# Random suffix for globally unique names (enabled by default)
use_random_suffix = true

# Monitoring
log_retention_days = 30

# Storage
storage_replication_type = "LRS"
upload_sample_data       = false # Disable for initial deployment

# SQL Database
sql_database_name        = "mcpworkshop"
sql_admin_username       = "mcpadmin"
sql_enable_public_access = true
sql_sku_name             = "Basic"

# Cosmos DB
cosmos_database_name     = "mcpworkshop"
cosmos_consistency_level = "Session"
cosmos_enable_free_tier  = true

# MCP Servers - Using placeholder images initially
# TODO: Update with actual MCP server images after building
enable_sql_server      = false # Disable until images are built
enable_cosmos_server   = false # Disable until images are built
enable_rest_server     = false # Disable until images are built
enable_virtual_analyst = false # Disable until images are built

# Data Seeding - Disable for initial deployment
seed_sample_data = false

# Tags
tags = {
  Project     = "MCP Workshop"
  Environment = "Workshop"
  ManagedBy   = "Terraform"
  Owner       = "Workshop Team"
}
