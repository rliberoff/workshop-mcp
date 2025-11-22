# Root Terraform Variables for MCP Workshop

# General Configuration
variable "name_prefix" {
  description = "Prefix for all resource names"
  type        = string
  default     = "mcpworkshop"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "workshop"
}

variable "resource_group_name" {
  description = "Name of the Azure resource group (suffix will be added automatically)"
  type        = string
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "swedencentral"
}

variable "use_random_suffix" {
  description = "Use random suffix for globally unique resource names"
  type        = bool
  default     = true
}

variable "suffix" {
  description = "Custom suffix for resource names (only used if use_random_suffix is false)"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Project     = "MCP Workshop"
    Environment = "Development"
    ManagedBy   = "Terraform"
  }
}

# Monitoring Configuration
variable "log_retention_days" {
  description = "Log Analytics retention period in days"
  type        = number
  default     = 30
}

# Storage Configuration
variable "storage_replication_type" {
  description = "Storage replication type (LRS, GRS, RAGRS)"
  type        = string
  default     = "LRS"
}

variable "upload_sample_data" {
  description = "Upload sample JSON data files to storage"
  type        = bool
  default     = true
}

variable "cors_allowed_origins" {
  description = "CORS allowed origins for storage"
  type        = list(string)
  default     = ["*"]
}

# SQL Database Configuration
variable "sql_database_name" {
  description = "Name of the SQL database"
  type        = string
  default     = "mcpworkshop"
}

variable "sql_admin_username" {
  description = "SQL Server admin username"
  type        = string
  default     = "mcpadmin"
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}

variable "azuread_admin_login" {
  description = "Azure AD admin login name (optional, leave empty for workshop)"
  type        = string
  default     = null
}

variable "azuread_admin_object_id" {
  description = "Azure AD admin object ID (optional, leave empty for workshop)"
  type        = string
  default     = null
}

variable "sql_enable_public_access" {
  description = "Enable public network access to SQL Server"
  type        = bool
  default     = true
}

variable "sql_sku_name" {
  description = "SQL Database SKU (Basic, S0, S1, P1, etc.)"
  type        = string
  default     = "S0"
}

# Cosmos DB Configuration
variable "cosmos_database_name" {
  description = "Name of the Cosmos DB database"
  type        = string
  default     = "mcpworkshop"
}

variable "cosmos_consistency_level" {
  description = "Cosmos DB consistency level"
  type        = string
  default     = "Session"
}

variable "cosmos_enable_free_tier" {
  description = "Enable Cosmos DB free tier"
  type        = bool
  default     = false
}

# MCP Server Configuration
variable "enable_sql_server" {
  description = "Deploy SQL MCP Server"
  type        = bool
  default     = true
}

variable "enable_cosmos_server" {
  description = "Deploy Cosmos MCP Server"
  type        = bool
  default     = true
}

variable "enable_rest_server" {
  description = "Deploy REST API MCP Server"
  type        = bool
  default     = true
}

variable "enable_virtual_analyst" {
  description = "Deploy Virtual Analyst Orchestrator"
  type        = bool
  default     = true
}

# Authentication Configuration
variable "jwt_secret" {
  description = "JWT secret key for authentication"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer URL"
  type        = string
  default     = "https://mcpworkshop.example.com"
}

variable "jwt_audience" {
  description = "JWT audience identifier"
  type        = string
  default     = "api://mcpworkshop"
}

# Data Seeding
variable "seed_sample_data" {
  description = "Seed databases with sample data"
  type        = bool
  default     = true
}
